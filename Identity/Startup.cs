using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.ClaimProviders;
using Identity.CustomValidation;
using Identity.Data;
using Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using static Identity.Requirements;

namespace Identity
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("Sqlite")));

            services.AddAuthorization(options =>
            {
                // [Authorize(Policy = "IstanbulPolicy")] kullanılan action/controller'a 
                // city claim'i İstanbul değerine sahip olmayan kullanıcılar erişemez.
                options.AddPolicy("IstanbulPolicy", policy => policy.RequireClaim("city", "İstanbul"));
                options.AddPolicy("ViolencePolicy", policy => policy.RequireClaim("violence"));
                options.AddPolicy("ExchangePolicy", policy =>
                    policy.AddRequirements(new ExchangeExpireDateRequirement()));
            });

            services.AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                })
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                })
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                });

            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;

                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcçdefgğhıijklmnoöpqrsştuüvwxyzABCÇDEFGĞHIİJKLMNOÖPQRSTUÜVWXYZ0123456789_-";
            })
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddPasswordValidator<CustomPasswordValidator>()
                .AddUserValidator<CustomUserValidator>()
                .AddErrorDescriber<TurkishErrorDescriber>()
                // NotSupportedException: No IUserTwoFactorTokenProvider<TUser> named 'Default' is registered.
                // hatası gönderir eğer eklenmezse.
                .AddDefaultTokenProviders();

            var cookie = new CookieBuilder()
            {
                Name = "MyBlog",
                HttpOnly = false,
                // Sadece site üzerinden gelen cookie'leri kaydeder
                // Çok güvenlik gerekmeyen siteler için Lax modu
                // Güvenliğin çok önemli olduğu sitelerde Strict modu
                SameSite = SameSiteMode.Lax,
                // Cookie'nin https üzerinden gelip gelmeyeceğini ayarlar
                SecurePolicy = CookieSecurePolicy.SameAsRequest
            };

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/home/login");
                // Bu action çağırılırken belirtilen return url'e otomatik gitmesini sağlar.
                options.LogoutPath = new PathString("/member/logout");
                options.AccessDeniedPath = new PathString("/member/accessdenied");

                options.Cookie = cookie;
                // Cookie ömrü bitmeden istek yaparsa cookie ömrü kadar daha uzar.
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(60);
            });

            services.AddMailKit(options =>
            {
                options.UseMailKit(new MailKitOptions
                {
                    Server = "127.0.0.1",
                    Port = 25,
                    SenderName = "Identity Uyelik Sistemi",
                    SenderEmail = "admin@a.b"
                });
            });

            services.AddScoped<IClaimsTransformation, ClaimProvider>();

            services.AddTransient<IAuthorizationHandler, ExchangeExpireDateHandler>();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
