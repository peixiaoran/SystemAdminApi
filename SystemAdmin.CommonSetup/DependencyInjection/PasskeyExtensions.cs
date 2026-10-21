using Fido2NetLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>Passkey（WebAuthn / FIDO2）注册扩展</summary>
    public static class PasskeyExtensions
    {
        /// <summary>绑定 Passkey 配置并注册 IFido2</summary>
        public static IServiceCollection AddPasskeySetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PasskeyOptions>(configuration.GetSection("Passkey"));

            var options = configuration.GetSection("Passkey").Get<PasskeyOptions>() ?? new PasskeyOptions();

            services.AddSingleton<IFido2>(_ => new Fido2(new Fido2Configuration
            {
                RPID = options.RpId,
                RPName = options.RpName,
                Origins = new HashSet<string>(options.Origins)
            }));

            return services;
        }
    }
}
