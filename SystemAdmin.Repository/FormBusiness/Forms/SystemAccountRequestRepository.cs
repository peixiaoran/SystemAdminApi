using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Dto;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Dto;
using SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Entity;
using DepartmentDropDto = SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto.DepartmentDropDto;
using NationalityDropDto = SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto.NationalityDropDto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Repository.FormBusiness.Forms
{
    public class SystemAccountRequestRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;
        private readonly FormManager _formManager;

        public SystemAccountRequestRepository(SqlSugarScope db, Language lang, FormManager formManager)
        {
            _db = db;
            _lang = lang;
            _formManager = formManager;
        }

        /// <summary>
        /// 厂区下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<SiteDropDto>> GetSiteDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "Sites")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new SiteDropDto()
                            {
                                Site = dic.DicCode,
                                SiteName = _lang.Locale == "zh-CN"
                                              ? dic.DicNameCn
                                              : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 部门树下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<DepartmentDropDto>> GetDepartmentDrop()
        {
            return await _db.Queryable<DepartmentInfoEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<DepartmentLevelEntity>((dept, deptlevel) => dept.DepartmentLevelId == deptlevel.DepartmentLevelId)
                            .OrderBy((dept, deptlevel) => dept.SortOrder)
                            .Select((dept, deptlevel) => new DepartmentDropDto
                            {
                                DepartmentId = dept.DepartmentId,
                                DepartmentName = _lang.Locale == "zh-CN"
                                                 ? dept.DepartmentNameCn
                                                 : dept.DepartmentNameEn,
                                ParentId = dept.ParentId,
                            }).ToTreeAsync(menu => menu.DepartmentChildList, menu => menu.ParentId, null);
        }

        /// <summary>
        /// 国籍下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<NationalityDropDto>> GetNationalityDrop()
        {
            return await _db.Queryable<NationalityInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Select(nation => new NationalityDropDto
                            {
                                NationalityId = nation.NationalityId,
                                NationName = _lang.Locale == "zh-CN"
                                             ? nation.NationNameCn
                                             : nation.NationNameEn
                            }).ToListAsync();
        }

        /// <summary>
        /// 系统下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<SystemDropDto>> GetSystemDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "ITSystem")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new SystemDropDto()
                            {
                                System = dic.DicCode,
                                SystemName = _lang.Locale == "zh-CN"
                                             ? dic.DicNameCn
                                             : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 按系统和厂区查询权限下拉
        /// </summary>
        /// <param name="system"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<List<PermissionsDropDto>> GetPermissionsDrop(string system, string site)
        {
            return await _db.Queryable<SystemPermissionsInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(info => info.System == system && info.Site == site)
                            .OrderBy(info => info.PermissionsCode)
                            .Select(info => new PermissionsDropDto()
                            {
                                Permissions = info.PermissionsCode,
                                PermissionsName = _lang.Locale == "zh-CN"
                                                  ? info.PermissionsNameCn
                                                  : info.PermissionsNameEn,
                                PermissionsNotes = _lang.Locale == "zh-CN"
                                                   ? info.PermissionsNotesCn
                                                   : info.PermissionsNotesEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 初始化系统账号申请单单头
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InitSystemAccountRequest(SystemAccountRequestEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 保存系统账号申请单
        /// </summary>
        /// <param name="header"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<int> SaveSystemAccountRequest(SystemAccountRequestEntity header, List<SystemAccountRequestDetailsEntity> entities)
        {
            var formId = header.FormId;

            var headerCount = await _db.Updateable(header)
                                       .IgnoreColumns(info => new
                                       {
                                           info.FormId,
                                           info.CreatedBy,
                                           info.CreatedDate,
                                       }).Where(info => info.FormId == formId)
                                       .ExecuteCommandAsync();

            var existing = await _db.Queryable<SystemAccountRequestDetailsEntity>()
                                    .Where(info => info.FormId == formId)
                                    .ToListAsync();

            foreach (var entity in entities)
            {
                var old = existing.FirstOrDefault(info => info.System == entity.System);
                if (old != null)
                {
                    entity.CreatedBy = old.CreatedBy;
                    entity.CreatedDate = old.CreatedDate;
                }
            }

            await _db.Deleteable<SystemAccountRequestDetailsEntity>()
                     .Where(info => info.FormId == formId)
                     .ExecuteCommandAsync();

            if (entities.Count > 0)
            {
                await _db.Insertable(entities).ExecuteCommandAsync();
            }

            var detailValues = new List<string?> { header.RequesterUserNo, header.RequesterUserNameCn, header.RequesterUserNameEn, header.RequesterEmail, header.Reason };
            foreach (var entity in entities)
            {
                detailValues.Add(entity.System);
                detailValues.Add(entity.Permissions);
            }

            var siteRefs = entities.SelectMany(entity => entity.Sites.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                   .Distinct()
                                   .Select(site => ("Sites", (string?)site))
                                   .ToList();

            await _formManager.SaveFormSearch(formId, detailValues, siteRefs);

            return headerCount;
        }

        /// <summary>
        /// 查询系统账号申请单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<SystemAccountRequestDto> GetSystemAccountRequest(long formId)
        {
            var form = await _db.Queryable<FormInstanceEntity>()
                                .With(SqlWith.NoLock)
                                .InnerJoin<UserInfoEntity>((form, user) => form.ApplicantUserId == user.UserId)
                                .InnerJoin<DepartmentInfoEntity>((form, user, dept) => user.DepartmentId == dept.DepartmentId)
                                .InnerJoin<DictionaryInfoEntity>((form, user, dept, dic) => dic.DicType == "FormStatus" && form.FormStatus == dic.DicCode)
                                .Where((form, user, dept, dic) => form.FormId == formId)
                                .Select((form, user, dept, dic) => new SystemAccountRequestDto()
                                {
                                    FormTypeId = form.FormTypeId,
                                    RuleId = form.RuleId,
                                    CurrentStepId = form.CurrentStepId,
                                    FormStatus = form.FormStatus,
                                    FormStatusName = _lang.Locale == "zh-CN"
                                                     ? dic.DicNameCn
                                                     : dic.DicNameEn,
                                    FormId = form.FormId,
                                    FormNo = form.FormNo,
                                    ApplicantUserNo = user.UserNo,
                                    ApplicantUserName = _lang.Locale == "zh-CN"
                                                     ? user.UserNameCn
                                                     : user.UserNameEn,
                                    ApplicantDeptName = _lang.Locale == "zh-CN"
                                                     ? dept.DepartmentNameCn
                                                     : dept.DepartmentNameEn,
                                    ApplicantDate = form.ApplicantDate,
                                }).FirstAsync();

            var header = await _db.Queryable<SystemAccountRequestEntity>()
                                  .With(SqlWith.NoLock)
                                  .Where(info => info.FormId == formId)
                                  .FirstAsync();
            if (header != null)
            {
                form.RequesterUserDept = header.RequesterUserDept;
                form.RequesterUserNo = header.RequesterUserNo;
                form.RequesterUserNameCn = header.RequesterUserNameCn;
                form.RequesterUserNameEn = header.RequesterUserNameEn;
                form.RequesterGender = header.RequesterGender;
                form.NationalityId = header.NationalityId;
                form.RequesterEmail = header.RequesterEmail;
                form.Reason = header.Reason;
            }

            var rows = await _db.Queryable<SystemAccountRequestDetailsEntity>()
                                .With(SqlWith.NoLock)
                                .Where(info => info.FormId == formId)
                                .OrderBy(info => info.CreatedDate)
                                .OrderBy(info => info.System)
                                .ToListAsync();

            if (rows.Count > 0)
            {
                form.Systems = rows.Select(row => new SystemAccountSystemDto()
                {
                    System = row.System,
                    Sites = SplitMulti(row.Sites),
                    Permissions = SplitMulti(row.Permissions),
                    Rating = row.Rating,
                }).ToList();
            }

            return form;
        }

        /// <summary>
        /// 查询打印用名称：部门、国籍、系统、厂区、权限的编码转当前语言名称，查不到时回退为原值
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public async Task<SystemAccountRequestPrintDto> GetSystemAccountRequestPrint(SystemAccountRequestDto form)
        {
            var print = new SystemAccountRequestPrintDto();

            if (form.RequesterUserDept.HasValue)
            {
                print.DeptName = await _db.Queryable<DepartmentInfoEntity>()
                                          .With(SqlWith.NoLock)
                                          .Where(dept => dept.DepartmentId == form.RequesterUserDept.Value)
                                          .Select(dept => _lang.Locale == "zh-CN"
                                                          ? dept.DepartmentNameCn
                                                          : dept.DepartmentNameEn)
                                          .FirstAsync() ?? string.Empty;
            }

            if (form.NationalityId.HasValue)
            {
                print.NationalityName = await _db.Queryable<NationalityInfoEntity>()
                                                 .With(SqlWith.NoLock)
                                                 .Where(nation => nation.NationalityId == form.NationalityId.Value)
                                                 .Select(nation => _lang.Locale == "zh-CN"
                                                                   ? nation.NationNameCn
                                                                   : nation.NationNameEn)
                                                 .FirstAsync() ?? string.Empty;
            }

            if (form.Systems.Count == 0)
                return print;

            var systemNames = (await GetSystemDrop()).ToDictionary(item => item.System, item => item.SystemName);
            var siteNames = (await GetSiteDrop()).ToDictionary(item => item.Site, item => item.SiteName);

            var systems = form.Systems.Select(item => item.System).Distinct().ToList();
            var permissionList = await _db.Queryable<SystemPermissionsInfoEntity>()
                                          .With(SqlWith.NoLock)
                                          .Where(info => systems.Contains(info.System))
                                          .Select(info => new
                                          {
                                              info.System,
                                              info.PermissionsCode,
                                              Name = _lang.Locale == "zh-CN"
                                                     ? info.PermissionsNameCn
                                                     : info.PermissionsNameEn
                                          }).ToListAsync();

            // 同系统下不同厂区的同一权限编码名称一致，取第一条
            var permissionNames = permissionList.GroupBy(item => (item.System, item.PermissionsCode))
                                                .ToDictionary(group => group.Key, group => group.First().Name);

            print.Systems = form.Systems.Select(item => new SystemAccountSystemPrintDto()
            {
                SystemName = systemNames.GetValueOrDefault(item.System, item.System),
                SiteNames = item.Sites.Select(site => siteNames.GetValueOrDefault(site, site)).ToList(),
                PermissionNames = item.Permissions.Select(code => permissionNames.GetValueOrDefault((item.System, code), code)).ToList(),
                Rating = item.Rating,
            }).ToList();

            return print;
        }

        /// <summary>
        /// 逗号分隔的多选值转集合
        /// </summary>
        private static List<string> SplitMulti(string? value)
        {
            return (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
    }
}
