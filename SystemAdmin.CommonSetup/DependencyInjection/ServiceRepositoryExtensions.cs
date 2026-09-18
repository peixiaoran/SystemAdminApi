using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>Service / Repository 批量注册扩展</summary>
    public static class ServiceRepositoryExtensions
    {
        /// <summary>把 Service、Repository 程序集中的公共类按 Scoped 注册</summary>
        public static IServiceCollection AddProjectClasses(this IServiceCollection services)
        {
            RegisterAllClasses(services, "SystemAdmin.Service");
            RegisterAllClasses(services, "SystemAdmin.Repository");
            return services;
        }

        private static void RegisterAllClasses(IServiceCollection services, string assemblyName)
        {
            var types = Assembly.Load(assemblyName).GetTypes()
                .Where(t => t.IsClass
                            && t.IsPublic
                            && !t.IsAbstract
                            && !t.IsGenericTypeDefinition
                            && !t.Name.StartsWith('<')
                            && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)));

            foreach (var type in types)
                services.AddScoped(type);
        }
    }
}
