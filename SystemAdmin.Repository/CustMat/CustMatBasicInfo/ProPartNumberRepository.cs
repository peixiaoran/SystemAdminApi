using Mapster;
using SqlSugar;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries;

namespace SystemAdmin.Repository.CustMat.CustMatBasicInfo
{
    public class ProductionNumberRepository
    {
        private readonly SqlSugarScope _db;

        public ProductionNumberRepository(SqlSugarScope db)
        {
            _db = db;
        }

        /// <summary>
        /// 新增料号信息
        /// </summary>
        /// <param name="partNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> InsertPartNumberInfo(ProductionNumberEntity partNumberEntity)
        {
            return await _db.Insertable(partNumberEntity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除料号信息
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<int> DeletePartNumberInfo(long partNumberId)
        {
            return await _db.Deleteable<ProductionNumberEntity>()
                            .Where(partNumber => partNumber.PartNumberId == partNumberId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改料号信息
        /// </summary>
        /// <param name="partNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> UpdatePartNumberInfo(ProductionNumberEntity partNumberEntity)
        {
            return await _db.Updateable(partNumberEntity)
                            .IgnoreColumns(partNumber => new
                            {
                                partNumber.PartNumberId,
                                partNumber.CreatedBy,
                                partNumber.CreatedDate,
                            }).Where(partNumber => partNumber.PartNumberId == partNumberEntity.PartNumberId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询料号实体
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<ProductionNumberDto> GetPartNumberInfoEntity(long partNumberId)
        {
            var partNumberEntity = await _db.Queryable<ProductionNumberEntity>()
                                            .With(SqlWith.NoLock)
                                            .Where(partNumber => partNumber.PartNumberId == partNumberId)
                                            .FirstAsync();
            return partNumberEntity.Adapt<ProductionNumberDto>();
        }

        /// <summary>
        /// 查询料号分页
        /// </summary>
        /// <param name="getPartNumberPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<ProductionNumberDto>> GetPartNumberInfoPage(GetProductionNumberPage getPartNumberPage)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<ProductionNumberEntity>()
                           .With(SqlWith.NoLock);

            // 料号
            if (!string.IsNullOrEmpty(getPartNumberPage.PartNumberNo))
            {
                query = query.Where(partnumber => partnumber.PartNumberNo.Contains(getPartNumberPage.PartNumberNo));
            }

            // 排序
            query = query.OrderBy(partNumber => partNumber.CreatedDate);

            var partNumberPage = await query.ToPageListAsync(getPartNumberPage.PageIndex, getPartNumberPage.PageSize, totalCount);
            return ResultPaged<ProductionNumberDto>.Ok(partNumberPage.Adapt<List<ProductionNumberDto>>(), totalCount, "");
        }

        /// <summary>
        /// 批量新增料号信息列表
        /// </summary>
        /// <param name="partNumberInfoList"></param>
        /// <returns></returns>
        public async Task<int> InsertPartNumberInfoList(List<ProductionNumberEntity> partNumberInfoList)
        {
            return await _db.Insertable(partNumberInfoList).ExecuteCommandAsync();
        }
    }
}
