using Mapster;
using SqlSugar;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;

namespace SystemAdmin.Repository.SystemBasicMgmt.SystemBasicData
{
    public class NationalityInfoRepository
    {
        private readonly SqlSugarScope _db;

        public NationalityInfoRepository(SqlSugarScope db)
        {
            _db = db;
        }

        /// <summary>
        /// 新增国籍
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InsertNationalityInfo(NationalityInfoEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除国籍
        /// </summary>
        /// <param name="nationalityId"></param>
        /// <returns></returns>
        public async Task<int> DeleteNationalityInfo(long nationalityId)
        {
            return await _db.Deleteable<NationalityInfoEntity>()
                            .Where(Nationality => Nationality.NationalityId == nationalityId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改国籍
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> UpdateNationalityInfo(NationalityInfoEntity entity)
        {
            return await _db.Updateable(entity)
                            .IgnoreColumns(Nationality => new
                            {
                                Nationality.NationalityId,
                                Nationality.CreatedBy,
                                Nationality.CreatedDate,
                            }).Where(Nationality => Nationality.NationalityId == entity.NationalityId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询国籍实体
        /// </summary>
        /// <param name="nationalityId"></param>
        /// <returns></returns>
        public async Task<NationalityInfoDto> GetNationalityEntity(long nationalityId)
        {
            return await _db.Queryable<NationalityInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(userPos => userPos.NationalityId == nationalityId)
                            .Select(nation => new NationalityInfoDto
                            {
                                NationalityId = nation.NationalityId,
                                NationNameCn = nation.NationNameCn,
                                NationNameEn = nation.NationNameEn,
                                Remark = nation.Remark,
                            }).FirstAsync();
        }

        /// <summary>
        /// 查询国籍列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<NationalityInfoDto>> GetNationalityInfoList()
        {
            return await _db.Queryable<NationalityInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Select(nation => new NationalityInfoDto
                            {
                                NationalityId = nation.NationalityId,
                                NationNameCn = nation.NationNameCn,
                                NationNameEn = nation.NationNameEn,
                                Remark = nation.Remark,
                            }).ToListAsync();
        }
    }
}
