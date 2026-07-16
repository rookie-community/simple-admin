using Admin.Localization;
using Admin.MongoDB;
using Admin.MultiTenancy;
using Admin.Web.HealthChecks;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Autofac;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace Admin.Web
{
    [DependsOn(
        typeof(AdminHttpApiModule),
        typeof(AdminApplicationModule),
        //typeof(AdminEntityFrameworkCoreModule),
        typeof(AdminMongoDbModule),
        typeof(AbpAutofacModule),
        typeof(AbpIdentityAspNetCoreModule),
        typeof(AbpOpenIddictAspNetCoreModule),
        typeof(AbpSwashbuckleModule),
        typeof(AbpAspNetCoreSerilogModule),
        typeof(AbpAspNetCoreSignalRModule)
    )]
    public class AdminWebModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            var hostingEnvironment = context.Services.GetHostingEnvironment();
            var configuration = context.Services.GetConfiguration();

            context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
            {
                options.AddAssemblyResource(
                    typeof(AdminResource),
                    typeof(AdminDomainModule).Assembly,
                    typeof(AdminDomainSharedModule).Assembly,
                    typeof(AdminApplicationModule).Assembly,
                    typeof(AdminApplicationContractsModule).Assembly,
                    typeof(AdminWebModule).Assembly
                );
            });

            PreConfigure<OpenIddictBuilder>(builder =>
            {
                builder.AddValidation(options =>
                {
                    options.AddAudiences("Admin");
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });
            });

            if (!hostingEnvironment.IsDevelopment())
            {
                PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
                {
                    options.AddDevelopmentEncryptionAndSigningCertificate = false;
                });

                PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
                {
                    serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
                    serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
                });
            }
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var hostingEnvironment = context.Services.GetHostingEnvironment();
            var configuration = context.Services.GetConfiguration();

            if (!configuration.GetValue<bool>("App:DisablePII"))
            {
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
            }

            if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
            {
                Configure<OpenIddictServerAspNetCoreOptions>(options =>
                {
                    options.DisableTransportSecurityRequirement = true;
                });

                Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                    options.KnownIPNetworks.Clear();
                    options.KnownProxies.Clear();
                });
            }

            if (hostingEnvironment.IsDevelopment())
            {
                context.Services.AddRazorPages()
                    .AddRazorRuntimeCompilation();
            }

            ConfigureUrls(configuration);
            ConfigureHealthChecks(context);
            ConfigureAuthentication(context);
            ConfigureVirtualFileSystem(hostingEnvironment);
            ConfigureAutoApiControllers();
            ConfigureSwaggerServices(context.Services);

            Configure<PermissionManagementOptions>(options =>
            {
                options.IsDynamicPermissionStoreEnabled = true;
            });

            Configure<AbpMvcLibsOptions>(options =>
            {
                options.CheckLibs = false;
            });
        }


        private void ConfigureHealthChecks(ServiceConfigurationContext context)
        {
            context.Services.AddAbpSolution1HealthChecks();
        }

        private void ConfigureUrls(IConfiguration configuration)
        {
            Configure<AppUrlOptions>(options =>
            {
                options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            });
        }

        private void ConfigureAuthentication(ServiceConfigurationContext context)
        {
            context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
            {
                options.IsDynamicClaimsEnabled = true;
            });
        }

        private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<AdminWebModule>();

                if (hostingEnvironment.IsDevelopment())
                {
                    options.FileSets.ReplaceEmbeddedByPhysical<AdminDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}Admin.Domain.Shared", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<AdminDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}Admin.Domain", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<AdminApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}Admin.Application.Contracts", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<AdminApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}Admin.Application", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<AdminHttpApiModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Admin.HttpApi", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<AdminWebModule>(hostingEnvironment.ContentRootPath);
                }
            });
        }

        private void ConfigureAutoApiControllers()
        {
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                //自动 API 控制器
                //options.ConventionalControllers.Create(typeof(AdminApplicationModule).Assembly);
            });
        }

        private void ConfigureSwaggerServices(IServiceCollection services)
        {
            services.AddAbpSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin API", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                }
            );
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAbpRequestLocalization();

            if (!env.IsDevelopment())
            {
                app.UseErrorPage();
                app.UseHsts();
            }

            app.UseCorrelationId();
            app.UseRouting();
            app.MapAbpStaticAssets();

            app.UseAbpSecurityHeaders();
            app.UseAuthentication();
            app.UseAbpOpenIddictValidation();

            if (MultiTenancyConsts.IsEnabled)
            {
                app.UseMultiTenancy();
            }

            app.UseUnitOfWork();
            app.UseDynamicClaims();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseAbpSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin API");
            });
            app.UseAuditing();
            app.UseAbpSerilogEnrichers();
            app.UseConfiguredEndpoints();
        }
    }
}
