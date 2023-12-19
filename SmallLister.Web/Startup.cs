using System;
using System.IO;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Feed;
using SmallLister.Security;
using SmallLister.Webhook;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SmallLister.Web
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);

            services
                .ConfigureApplicationCookie(c => c.Cookie.Name = "smalllister")
                .AddAuthentication(o =>
                {
                    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(o =>
                {
                    o.LoginPath = "/signin";
                    o.LogoutPath = "/signout";
                    o.Cookie.Name = "smalllister";
                    o.Cookie.HttpOnly = true;
                    o.Cookie.MaxAge = TimeSpan.FromDays(1);
                    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    o.Cookie.IsEssential = true;
                    o.ExpireTimeSpan = TimeSpan.FromDays(1);
                    o.SlidingExpiration = true;
                })
                .AddJwtBearer("ApiJwt", options =>
                {
                    var tokenOptions = Configuration.GetSection("SmallListerApiJwt");
                    var signingKey = tokenOptions.GetValue("SigningKey", "");
                    var issuer = tokenOptions.GetValue("Issuer", "");
                    var audience = tokenOptions.GetValue("Audience", "");

                    options.SaveToken = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                        ValidIssuer = issuer,
                        ValidAudience = audience
                    };
                });
            services.AddAuthorization(options =>
                options.AddPolicy("ApiJwt", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("ApiJwt")
                    .Build()));

            services
                .AddDataProtection()
                .SetApplicationName(typeof(Startup).Namespace)
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Environment.ContentRootPath, ".keys")));

            services.AddLogging(logging =>
            {
                logging.AddSimpleConsole(o => o.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ");
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Trace);
            });

            services.Configure<CookiePolicyOptions>(o =>
            {
                o.CheckConsentNeeded = context => false;
                o.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<SqliteDataContext>((serviceProvider, options) =>
            {
                var sqliteConnectionString = Configuration.GetConnectionString("SmallLister");
                serviceProvider.GetRequiredService<ILogger<Startup>>().LogInformation($"Using connection string: {sqliteConnectionString}");
                options.UseSqlite(sqliteConnectionString);
            });
            services
                .AddScoped(sp => (ISqliteDataContext)sp.GetRequiredService<SqliteDataContext>())
                .AddScoped<IUserAccountRepository, UserAccountRepository>()
                .AddScoped<IUserListRepository, UserListRepository>()
                .AddScoped<IUserItemRepository, UserItemRepository>()
                .AddScoped<IUserAccountApiAccessRepository, UserAccountApiAccessRepository>()
                .AddScoped<IUserAccountTokenRepository, UserAccountTokenRepository>()
                .AddScoped<IUserFeedRepository, UserFeedRepository>()
                .AddScoped<IApiClientRepository, ApiClientRepository>()
                .AddScoped<IJwtService, JwtService>()
                .AddScoped<IFeedGenerator, AtomFeedGenerator>()
                .AddScoped<IUserActionsService, UserActionsService>()
                .AddScoped<IUserActionHandler<IUserAction>, AddItemActionHandler>()
                .AddScoped<IUserActionHandler<IUserAction>, UpdateItemActionHandler>()
                .AddScoped<IUserActionHandler<IUserAction>, ReorderItemsActionHandler>()
                .AddScoped<IUserActionRepository, UserActionRepository>()
                .AddScoped<IUserWebhookRepository, UserWebhookRepository>()
                .AddScoped<IWebhookQueueRepository, WebhookQueueRepository>()
                .AddScoped<IWebhookChecker, WebhookChecker>()
                .AddScoped<IWebhookSender, WebhookSender>()
                .AddHttpClient(WebhookSender.WebhookSenderHttpClient).ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(120)).Services
                .AddSingleton<IWebhookNotification, WebhookNotification>()
                .AddHostedService<WebhookBackgroundService>();

            services.AddMediatR(typeof(Startup));
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddApiVersioning();
            services.AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();
            var builder = services.AddRazorPages();
#if DEBUG
            if (Environment.IsDevelopment())
                builder.AddRazorRuntimeCompilation();
#endif
            services.AddCors();
            services.AddDistributedMemoryCache();
            services
                .AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(5))
                .AddFido2(options =>
                {
                    options.ServerName = "Small:Lister";
                    options.ServerDomain = Configuration.GetValue<string>("FidoDomain");
                    options.Origins = [Configuration.GetValue<string>("FidoOrigins")];
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Home/Error");

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(options => options.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"));
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
            });

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetRequiredService<ISqliteDataContext>().Migrate();
        }
    }
}
