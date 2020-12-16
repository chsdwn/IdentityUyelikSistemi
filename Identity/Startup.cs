using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.CustomValidation;
using Identity.Data;
using Identity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            var cookieBuilder = new CookieBuilder()
            {
                Name = "MyBlog",
                HttpOnly = false,
                Expiration = TimeSpan.FromDays(60),
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
                options.Cookie = cookieBuilder;
                // Cookie ömrü bitmeden istek yaparsa cookie ömrü kadar daha uzar.
                options.SlidingExpiration = true;
            });

            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;

                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
            })
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddPasswordValidator<CustomPasswordValidator>()
                .AddUserValidator<CustomUserValidator>()
                .AddErrorDescriber<TurkishErrorDescriber>();

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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
