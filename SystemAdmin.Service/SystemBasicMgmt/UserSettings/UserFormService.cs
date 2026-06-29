using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Commands;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Dto;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Queries;
using SystemAdmin.Repository.SystemBasicMgmt.UserSettings;

namespace SystemAdmin.Service.SystemBasicMgmt.UserSettings
{
    public class UserFormService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<UserFormService> _logger;
        private readonly SqlSugarScope _db;
        private readonly UserFormRepository _userFormBindRepo;
        private readonly LocalizationService _localization;
        private readonly string _this = "SystemBasicMgmt.UserSettings.UserForm";

        public UserFormService(CurrentUser loginuser, ILogger<UserFormService> logger, SqlSugarScope db, UserFormRepository userFormBindRepo, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _userFormBindRepo = userFormBindRepo;
            _localization = localization;
        }

        /// <summary>
        /// 部门下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            try
            {
                var drop = await _userFormBindRepo.GetDepartmentDrop();
                return Result<List<DepartmentDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<DepartmentDropDto>>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 查询用户分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<UserFormDto>> GetUserInfoPage(GetUserFormPage getPage)
        {
            try
            {
                return await _userFormBindRepo.GetUserInfoPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<UserFormDto>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 查询用户绑定表单树
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result<List<UserFormViewTreeDto>>> GetUserFormViewTree(string userId)
        {
            try
            {
                var tree = await _userFormBindRepo.GetUserFormViewTree(long.Parse(userId));
                return Result<List<UserFormViewTreeDto>>.Ok(tree, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<UserFormViewTreeDto>>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 更新用户表单绑定
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdateUserForm(UserFormUpsert upsert)
        {
            try
            {
                var userId = long.Parse(upsert.UserId);
                var newIds = upsert.FormGroupTypeId.Select(long.Parse).ToHashSet();
                var now = DateTime.Now;

                await _db.BeginTranAsync();

                var existing = await _userFormBindRepo.GetUserFormList(userId);
                var existingIds = existing.Select(x => x.FormGroupTypeId).ToHashSet();

                // 删除：旧有但新列表中没有
                var toDelete = existing.Where(x => !newIds.Contains(x.FormGroupTypeId)).ToList();

                // 新增：新列表中有但旧记录没有
                var toInsert = newIds.Where(id => !existingIds.Contains(id))
                                     .Select(id => new UserFormEntity
                                     {
                                         UserId = userId,
                                         FormGroupTypeId = id,
                                         CreatedBy = _loginuser.UserId,
                                         CreatedDate = now,
                                         ModifiedBy = _loginuser.UserId,
                                         ModifiedDate = now
                                     }).ToList();

                // 更新：两边都有，只改修改字段
                var toUpdate = existing.Where(x => newIds.Contains(x.FormGroupTypeId)).ToList();
                foreach (var item in toUpdate)
                {
                    item.ModifiedBy = _loginuser.UserId;
                    item.ModifiedDate = now;
                }

                if (toDelete.Count > 0)
                    await _userFormBindRepo.DeleteUserFormBatch(toDelete);

                if (toUpdate.Count > 0)
                    await _userFormBindRepo.UpdateUserFormModified(toUpdate);

                if (toInsert.Count > 0)
                    await _userFormBindRepo.InsertUserForm(toInsert);

                await _db.CommitTranAsync();

                var count = toInsert.Count + toUpdate.Count;
                return count > 0
                    ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}UpdateSuccess"))
                    : Result<int>.Failure(400, _localization.ReturnMsg($"{_this}UpdateFailure"));
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
