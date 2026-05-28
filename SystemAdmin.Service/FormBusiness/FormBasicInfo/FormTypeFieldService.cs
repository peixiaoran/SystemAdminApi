using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Commands;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Dto;
using SystemAdmin.Repository.FormBusiness.FormBasicInfo;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Queries;

namespace SystemAdmin.Service.FormBusiness.FormBasicInfo
{
    public class FormTypeFieldService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<FormTypeFieldService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FormTypeFieldRepository _formTypeFieldRepo;
        private readonly LocalizationService _localization;
        private readonly string _this = "FormBusiness.FormBasicInfo.FormTypeField";

        public FormTypeFieldService(CurrentUser loginuser, ILogger<FormTypeFieldService> logger, SqlSugarScope db, FormTypeFieldRepository formTypeFieldRepo, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _formTypeFieldRepo = formTypeFieldRepo;
            _localization = localization;
        }

        /// <summary>
        /// 查询表单组别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            try
            {
                var drop = await _formTypeFieldRepo.GetFormGroupDrop();
                return Result<List<FormGroupDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormGroupDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 表单类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop(string formGroupId)
        {
            try
            {
                var drop = await _formTypeFieldRepo.GetFormTypeDrop(long.Parse(formGroupId));
                return Result<List<FormTypeDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormTypeDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 新增表单类别栏位
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertFormTypeField(FormTypeFieldUpsert upsert)
        {
            try
            {
                var entity = new FormTypeFieldEntity()
                {
                    FormTypeId = long.Parse(upsert.FormTypeId),
                    FieldId = SnowFlakeSingle.Instance.NextId(),
                    FieldKey = upsert.FieldKey,
                    FieldNameCn = upsert.FieldNameCn,
                    FieldNameEn = upsert.FieldNameEn,
                    SortOrder = upsert.SortOrder,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now,
                };

                await _db.BeginTranAsync();
                int count = await _formTypeFieldRepo.InsertFormTypeField(entity);
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}InsertSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}InsertFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 删除表单栏位
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteFormTypeField(string fieldId)
        {
            try
            {
                await _db.BeginTranAsync();
                int count = await _formTypeFieldRepo.DeleteFormTypeField(long.Parse(fieldId));
                await _db.CommitTranAsync();
                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}DeleteSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}DeleteFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 修改表单栏位
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdateFormTypeField(FormTypeFieldUpsert upsert)
        {
            try
            {
                var entity = new FormTypeFieldEntity()
                {
                    FormTypeId = long.Parse(upsert.FormTypeId),
                    FieldId = long.Parse(upsert.FieldId),
                    FieldKey = upsert.FieldKey,
                    FieldNameCn = upsert.FieldNameCn,
                    FieldNameEn = upsert.FieldNameEn,
                    SortOrder = upsert.SortOrder,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now,
                };
                await _db.BeginTranAsync();
                int count = await _formTypeFieldRepo.UpdateFormTypeField(entity);
                await _db.CommitTranAsync();
                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}UpdateSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}UpdateFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询表单栏位实体
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public async Task<Result<FormTypeFieldDto>> GetFormTypeFieldEntity(string fieldId)
        {
            try
            {
                var entity = await _formTypeFieldRepo.GetFormTypeFieldEntity(long.Parse(fieldId));
                return Result<FormTypeFieldDto>.Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<FormTypeFieldDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询表单栏位分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<FormTypeFieldDto>> GetFormTypeFieldPage(GetFormTypeFieldPage getPage)
        {
            try
            {
                return await _formTypeFieldRepo.GetFormTypeFieldPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<FormTypeFieldDto>.Failure(500, ex.Message);
            }
        }
    }
}
