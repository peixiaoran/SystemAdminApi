using SqlSugar;

namespace SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Entity
{
    /// <summary>
    /// 用户权限实体
    /// </summary>
    [SugarTable("[Basic].[UserRole]")]
    public class SysUserRoleEntity
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        public long RoleId { get; set; }
    }
}
