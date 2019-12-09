using Bit.Core;
using Bit.Core.Contracts;
using Bit.Core.Implementations;
using Bit.Model.Implementations;
using Bit.OData.Contracts;
using Bit.Owin.Implementations;
using Bit.Owin.Middlewares;
using Bit.OwinCore;
using Bit.OwinCore.Contracts;
using Bit.OwinCore.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[assembly: ODataModule("SampleApp")]

namespace BitAspNetCoreAngularSSR
{
    public class Startup : AutofacAspNetCoreAppStartup
    {
        public Startup(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            AspNetCoreAppEnvironmentsProvider.Current.Init();
        }

        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            DefaultAppModulesProvider.Current = new SampleAppModulesProvider();

            return base.ConfigureServices(services);
        }
    }

    public class SampleAppModulesProvider : IAppModule, IAppModulesProvider
    {
        public IEnumerable<IAppModule> GetAppModules()
        {
            yield return this;
        }

        public virtual void ConfigureDependencies(IServiceCollection services, IDependencyManager dependencyManager)
        {
            AssemblyContainer.Current.Init();

            dependencyManager.RegisterMinimalDependencies();

            dependencyManager.RegisterDefaultLogger(typeof(DebugLogStore).GetTypeInfo(), typeof(ConsoleLogStore).GetTypeInfo());

            dependencyManager.RegisterDefaultAspNetCoreApp();

            services.AddResponseCompression(options => options.EnableForHttps = true);
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist/";
            });

            var appEnv = DefaultAppEnvironmentsProvider.Current.GetActiveAppEnvironment();

            dependencyManager.RegisterAspNetCoreMiddlewareUsing(aspNetCoreApp =>
            {
                aspNetCoreApp.UseResponseCompression();
                aspNetCoreApp.UseHttpsRedirection();
                aspNetCoreApp.UseStaticFiles();
                aspNetCoreApp.UseSpaStaticFiles();
            });

            dependencyManager.RegisterMinimalAspNetCoreMiddlewares();

            dependencyManager.RegisterMetadata();

            dependencyManager.RegisterOwinMiddleware<ClientAppProfileMiddlewareConfiguration>(); // https://github.com/bit-foundation/bit-framework/issues/165

            dependencyManager.RegisterDefaultWebApiAndODataConfiguration();

            dependencyManager.RegisterWebApiMiddleware(webApiDependencyManager =>
            {
                webApiDependencyManager.RegisterWebApiMiddlewareUsingDefaultConfiguration();

                webApiDependencyManager.RegisterGlobalWebApiCustomizerUsing(httpConfiguration =>
                {
                    httpConfiguration.EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", $"Swagger-Api");
                        c.ApplyDefaultApiConfig(httpConfiguration);
                    }).EnableBitSwaggerUi();
                });
            });

            dependencyManager.RegisterODataMiddleware(odataDependencyManager =>
            {
                odataDependencyManager.RegisterGlobalWebApiCustomizerUsing(httpConfiguration =>
                {
                    httpConfiguration.EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", $"Swagger-Api");
                        c.ApplyDefaultODataConfig(httpConfiguration);
                    }).EnableBitSwaggerUi();
                });

                odataDependencyManager.RegisterWebApiODataMiddlewareUsingDefaultConfiguration();
            });

            dependencyManager.RegisterDtoEntityMapper();
            dependencyManager.RegisterMapperConfiguration<DefaultMapperConfiguration>();

            dependencyManager.RegisterAspNetCoreMiddlewareUsing(aspNetCoreApp =>
            {
                aspNetCoreApp.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

#if BuildServerSideRenderer
                    spa.UseSpaPrerendering(options =>
                    {
                        options.BootModulePath = $"{spa.Options.SourcePath}/dist/main.js";
                        options.BootModuleBuilder = appEnv.DebugMode
                            ? new AngularCliBuilder(npmScript: "build:ssr")
                            : null;
                        options.ExcludeUrls = new[] { "/sockjs-node" };

                        options.SupplyData = (httpContext, data) =>
                        {
                            string[] SeachEnginesUserAgents = new string[] {
                                "google",
                                "bing",
                                "linkedin",
                            };

                            string agent = httpContext.Request.Headers["User-Agent"];

                            bool isSearchEngine = SeachEnginesUserAgents.Any(a => agent.Contains(a, StringComparison.InvariantCultureIgnoreCase));

                            data.Add("isSearchEngine", isSearchEngine); // see main.server.ts
                        };
                    });
#endif
                    if (appEnv.DebugMode)
                        spa.UseAngularCliServer(npmScript: "start");
                });
            }, MiddlewarePosition.AfterOwinMiddlewares);
        }
    }
}
