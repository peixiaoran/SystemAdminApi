using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Commands;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Repository.FormBusiness.FormBasicInfo;

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
        /// 查询表单类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            try
            {
                var drop = await _formTypeRepo.GetFormGroupDrop();
                return Result<List<FormGroupDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormGroupDropDto>>.Failure(500, ex.Message);
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
                    FieldNameCn = upsert.FieldNameCn,
                    FieldNameEn = upsert.FieldNameEn,
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
    }
}
