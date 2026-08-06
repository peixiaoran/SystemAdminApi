using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Entity;

namespace SystemAdmin.Repository.SystemBasicMgmt.SystemAuth
{
    public class SysModuleMenuRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public SysModuleMenuRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 查询一级菜单列表
        /// </summary>
        /// <param name="loginUserId"></param>
        /// <returns></returns>
        public async Task<List<SysModuleInfoDto>> GetModuleList(long loginUserId)
        {
            return await _db.Queryable<SysUserInfoEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<SysUserRoleEntity>((user, userrole) => user.UserId == userrole.UserId)
                            .InnerJoin<SysRoleInfoEntity>((user, userrole, role) => userrole.RoleId == role.RoleId)
                            .InnerJoin<SysRoleModuleEntity>((user, userrole, role, rolemodule) => role.RoleId == rolemodule.RoleId)
                            .InnerJoin<SysModuleInfoEntity>((user, userrole, role, rolemodule, module) => rolemodule.ModuleId == module.ModuleId)
                            .Where((user, userrole, role, rolemodule, module) => user.UserId == loginUserId)
                            .OrderBy((user, userrole, role, rolemodule, module) => module.SortOrder)
                            .Select((user, userrole, role, rolemodule, module) => new SysModuleInfoDto
                            {
                                ModuleId = module.ModuleId,
                                ModuleNameCn = module.ModuleNameCn,
                                ModuleNameEn = module.ModuleNameEn,
                                ModuleIcon = module.ModuleIcon,
                                Path = module.Path,
                                Remark = _lang.Locale == "zh-CN"
                                         ? module.RemarkCh
                                         : module.RemarkEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 查询菜单树
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<SysMenuInfoDto>> GetMenuTreeList(long moduleId, long userId)
        {
            return await _db.Queryable<SysMenuInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(menu => menu.ModuleId == moduleId
                                        && (SqlFunc.Subqueryable<SysRoleMenuEntity>()
                                                    .InnerJoin<SysUserRoleEntity>((rolemenu, userrole) => rolemenu.RoleId == userrole.RoleId)
                                                    .Where((rolemenu, userrole) => userrole.UserId == userId && rolemenu.MenuId == menu.MenuId)
                                                    .Any()
                                            || (menu.ParentMenuId == null
                                                && SqlFunc.Subqueryable<SysMenuInfoEntity>()
                                                          .InnerJoin<SysRoleMenuEntity>((child, rolemenu) => child.MenuId == rolemenu.MenuId)
                                                          .InnerJoin<SysUserRoleEntity>((child, rolemenu, userrole) => rolemenu.RoleId == userrole.RoleId)
                                                          .Where((child, rolemenu, userrole) => child.ParentMenuId == menu.MenuId && userrole.UserId == userId)
                                                          .Any())))
                            .OrderBy(menu => menu.SortOrder)
                            .Select(menu => new SysMenuInfoDto
                            {
                                MenuId = menu.MenuId,
                                ParentMenuId = menu.ParentMenuId,
                                MenuCode = menu.MenuCode,
                                MenuName = _lang.Locale == "zh-CN"
                                           ? menu.MenuNameCn
                                           : menu.MenuNameEn,
                                Path = menu.Path,
                                MenuIcon = menu.MenuIcon
                            }).ToTreeAsync(menu => menu.MenuChildList, menu => menu.ParentMenuId, null);
        }
    }
}
