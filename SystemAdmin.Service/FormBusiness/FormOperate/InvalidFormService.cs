using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Repository.FormBusiness.FormOperate;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.FormOperate
{
    public class InvalidFormService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<InvalidFormService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FormPermissionChecker _formChecker;
        private readonly InvalidFormRepository _invalidFormRepo;
        private readonly LocalizationService _localization;
        private readonly string _this = "FormBusiness.FormOperate.InvalidForm";

        public InvalidFormService(CurrentUser loginuser, ILogger<InvalidFormService> logger, SqlSugarScope db, FormPermissionChecker formChecker, InvalidFormRepository invalidFormRepo, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _formChecker = formChecker;
            _invalidFormRepo = invalidFormRepo;
            _localization = localization;
        }

        /// <summary>
        /// 表单组别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            try
            {
                var drop = await _invalidFormRepo.GetFormGroupDrop();
                return Result<List<FormGroupDropDto>>.Ok(drop);
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
                var drop = await _invalidFormRepo.GetFormTypeDrop(long.Parse(formGroupId));
                return Result<List<FormTypeDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormTypeDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 表单状态下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            try
            {
                var drop = await _invalidFormRepo.GetFormStatusDrop();
                return Result<List<FormStatusDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormStatusDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询已作废分页
        /// </summary>
        /// <returns></returns>
        public async Task<ResultPaged<InvalidFormDto>> GetInvalidFormsPage(GetInvalidFormsPage getpage)
        {
            try
            {
                return await _invalidFormRepo.GetInvalidFormsPage(getpage, _loginuser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<InvalidFormDto>.Failure(500, ex.Message);
            }
        }
    }
}
