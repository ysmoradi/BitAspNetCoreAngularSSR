using Bit.Core;
using Bit.Core.Contracts;
using Bit.Model.Implementations;
using Bit.OData.Contracts;
using Bit.Owin;
using Bit.Owin.Contracts;
using Bit.Owin.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.Application;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Web.Http;

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

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
            }).Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            }).Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist/";
            });

            dependencyManager.RegisterAspNetCoreMiddlewareUsing(aspNetCoreApp =>
            {
                aspNetCoreApp.UseHttpsRedirection();
                aspNetCoreApp.UseResponseCompression();
                aspNetCoreApp.UseStaticFiles();
                aspNetCoreApp.UseSpaStaticFiles();
            });

            dependencyManager.RegisterMinimalAspNetCoreMiddlewares();

            dependencyManager.RegisterMetadata();

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
                    httpConfiguration.EnableMultiVersionWebApiSwaggerWithUI();
                });

                odataDependencyManager.RegisterWebApiODataMiddlewareUsingDefaultConfiguration();
            });

            dependencyManager.RegisterDtoEntityMapper();
            dependencyManager.RegisterMapperConfiguration<DefaultMapperConfiguration>();

            dependencyManager.RegisterAspNetCoreMiddlewareUsing(aspNetCoreApp =>
            {
                /*aspNetCoreApp.Use(async (cntx, next) =>
                {
                    cntx.Response.OnStarting(async () =>
                    {
                        cntx.Response.Headers.Add("Link", "<https://localhost:5001/styles.f5fe6687c43885ea4695.css>; rel=preload; as=style, <https://localhost:5001/runtime-es2015.e59a6cd8f1b6ab0c3f29.js>; rel=preload; as=script, <https://localhost:5001/polyfills-es2015.58725a5910daef768ca8.js>; rel=preload; as=script, <https://localhost:5001/main-es2015.79bc87a12189b9a95f11.js>; rel=preload; as=script");
                        // index.html: <link rel="stylesheet" href="https://localhost:5001/styles.f5fe6687c43885ea4695.css">
                    });

                    await next();
                });*/

                aspNetCoreApp.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

#if BuildServerSideRenderer
                    spa.UseSpaPrerendering(options =>
                    {
                        options.BootModulePath = $"{spa.Options.SourcePath}/dist/main.js";
                        options.BootModuleBuilder = AspNetCoreAppEnvironmentsProvider.Current.WebHostEnvironment.IsDevelopment()
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
                    if (AspNetCoreAppEnvironmentsProvider.Current.WebHostEnvironment.IsDevelopment())
                        spa.UseAngularCliServer(npmScript: "start");
                });
            }, MiddlewarePosition.AfterOwinMiddlewares);
        }
    }
}
