using SqlSugar;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Workflow.PersonResolver.Dto;

public class PersonResolver
{
    private readonly CurrentUser _loginuser;
    private readonly SqlSugarScope _db;
    private readonly Language _lang;

    // guidance(方法名) -> 实际方法
    private readonly Dictionary<string, Func<long, Task<CustomUser>>> _registry;

    public PersonResolver(CurrentUser loginuser, SqlSugarScope db, Language lang)
    {
        _loginuser = loginuser;
        _db = db;
        _lang = lang;

        // 登记所有自定义取人方法，新增方法只需在这里加一行
        _registry = new Dictionary<string, Func<long, Task<CustomUser>>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Ran)] = Ran,
        };
    }

    /// <summary>
    /// 按 guidance(配置的方法名) 分发到对应的自定义取人方法
    /// </summary>
    public async Task<CustomUser> Resolve(string guidance, long formId)
    {
        if (string.IsNullOrWhiteSpace(guidance) || !_registry.TryGetValue(guidance, out var handler))
            throw new InvalidOperationException($"未配置的取人方法: {guidance}");

        return await handler(formId);
    }

    #region 请假单
    public async Task<CustomUser> Ran(long formId)
    {
        return new CustomUser();
    }
    #endregion
}