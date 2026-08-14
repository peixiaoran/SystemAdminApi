using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;
using SystemAdmin.Model.CustMat.SalesMgmt.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.CustMat.SalesMgmt
{
    public class SalesUserRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public SalesUserRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 新增业务人员信息
        /// </summary>
        /// <param name="salesUserEntity"></param>
        /// <returns></returns>
        public async Task<int> InsertSalesUser(SalesUserEntity salesUserEntity)
        {
            return await _db.Insertable(salesUserEntity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除业务人员信息
        /// </summary>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<int> DeleteSalesUser(long salesUserId)
        {
            return await _db.Deleteable<SalesUserEntity>()
                            .Where(salesUser => salesUser.SalesUserId == salesUserId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改业务人员信息
        /// </summary>
        /// <param name="salesUserEntity"></param>
        /// <returns></returns>
        public async Task<int> UpdateSalesUser(SalesUserEntity salesUserEntity)
        {
            return await _db.Updateable(salesUserEntity)
                            .IgnoreColumns(salesUser => new
                            {
                                salesUser.SalesUserId,
                                salesUser.CreatedBy,
                                salesUser.CreatedDate,
                            }).Where(salesUser => salesUser.SalesUserId == salesUserEntity.SalesUserId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 业务人员是否已存在
        /// </summary>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<bool> ExistsSalesUser(long salesUserId)
        {
            return await _db.Queryable<SalesUserEntity>()
                            .With(SqlWith.NoLock)
                            .Where(salesUser => salesUser.SalesUserId == salesUserId)
                            .AnyAsync();
        }

        /// <summary>
        /// 查询业务人员信息实体
        /// </summary>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<SalesUserDto> GetSalesUserEntity(long salesUserId)
        {
            var entity = await _db.Queryable<SalesUserEntity>()
                                  .With(SqlWith.NoLock)
                                  .InnerJoin<UserInfoEntity>((salesUser, user) => salesUser.SalesUserId == user.UserId)
                                  .InnerJoin<DepartmentInfoEntity>((salesUser, user, dept) => salesUser.SalesDeptId == dept.DepartmentId)
                                  .LeftJoin<DictionaryInfoEntity>((salesUser, user, dept, dic) => salesUser.SalesType == dic.DicCode && dic.DicType == "SalesType")
                                  .Where(salesUser => salesUser.SalesUserId == salesUserId)
                                  .Select((salesUser, user, dept, dic) => new SalesUserDto
                                  {
                                      SalesUserId = salesUser.SalesUserId,
                                      SalesDeptId = salesUser.SalesDeptId,
                                      DepartmentName = _lang.Locale == "zh-CN" ? dept.DepartmentNameCn : dept.DepartmentNameEn,
                                      UserNo = user.UserNo,
                                      UserNameCn = user.UserNameCn,
                                      UserNameEn = user.UserNameEn,
                                      SalesType = salesUser.SalesType,
                                      SalesTypeName = _lang.Locale == "zh-CN" ? dic.DicNameCn : dic.DicNameEn,
                                      Description = salesUser.Description ?? string.Empty,
                                  })
                                  .FirstAsync();
            return entity;
        }

        /// <summary>
        /// 查询业务人员信息分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesUserDto>> GetSalesUserPage(GetSalesUserPage getPage)
        {
            var query = _db.Queryable<SalesUserEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<UserInfoEntity>((salesUser, user) => salesUser.SalesUserId == user.UserId)
                           .InnerJoin<DepartmentInfoEntity>((salesUser, user, dept) => salesUser.SalesDeptId == dept.DepartmentId)
                           .LeftJoin<DictionaryInfoEntity>((salesUser, user, dept, dic) => salesUser.SalesType == dic.DicCode && dic.DicType == "SalesType");

            // 部门Id（"0"表示不筛选）
            if (getPage.SalesDeptId != "0")
            {
                query = query.Where(salesUser => salesUser.SalesDeptId == long.Parse(getPage.SalesDeptId));
            }

            // 业务类型
            if (!string.IsNullOrEmpty(getPage.SalesType))
            {
                query = query.Where(salesUser => salesUser.SalesType == getPage.SalesType);
            }

            // 用户工号
            if (!string.IsNullOrEmpty(getPage.UserNo))
            {
                query = query.Where((salesUser, user) => user.UserNo.Contains(getPage.UserNo));
            }

            // 用户姓名（中英文模糊匹配）
            if (!string.IsNullOrEmpty(getPage.UserName))
            {
                query = query.Where((salesUser, user) => user.UserNameCn.Contains(getPage.UserName) || user.UserNameEn.Contains(getPage.UserName));
            }

            RefAsync<int> totalCount = 0;
            var page = await query.OrderBy(salesUser => salesUser.CreatedDate)
                                  .Select((salesUser, user, dept, dic) => new SalesUserDto
                                  {
                                      SalesUserId = salesUser.SalesUserId,
                                      SalesDeptId = salesUser.SalesDeptId,
                                      DepartmentName = _lang.Locale == "zh-CN" ? dept.DepartmentNameCn : dept.DepartmentNameEn,
                                      UserNo = user.UserNo,
                                      UserNameCn = user.UserNameCn,
                                      UserNameEn = user.UserNameEn,
                                      SalesType = salesUser.SalesType,
                                      SalesTypeName = _lang.Locale == "zh-CN" ? dic.DicNameCn : dic.DicNameEn,
                                      Description = salesUser.Description ?? string.Empty,
                                  }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<SalesUserDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 业务类型下拉（字典表 DicType = SalesType）
        /// </summary>
        /// <returns></returns>
        public async Task<List<SalesTypeDropDto>> GetSalesTypeDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "SalesType")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new SalesTypeDropDto
                            {
                                SalesType = dic.DicCode,
                                SalesTypeName = _lang.Locale == "zh-CN" ? dic.DicNameCn : dic.DicNameEn,
                            }).ToListAsync();
        }
    }
}
