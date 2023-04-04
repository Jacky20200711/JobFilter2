using JobFilter2.Models.Entities;
using JobFilter2.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobFilter2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<JobFilterContext>();
            services.AddControllersWithViews();
            services.AddSession();
            services.AddHttpContextAccessor();
            services.AddScoped<BackupService>();
            services.AddScoped<CrawlService>();

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = ".AspNetCore.Antiforgery.JobFilter2";    // 修改預設名稱，避免同樣 Domain 的 Cookie 互相干擾
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = ".AspNetCore.Authentication.JobFilter2"; // 修改預設名稱，避免同樣 Domain 的 Cookie 互相干擾
            });

            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.Name = ".AspNetCore.TempData.JobFilter2";       // 修改預設名稱，避免同樣 Domain 的 Cookie 互相干擾
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession(new SessionOptions()
            {
                Cookie = new CookieBuilder()
                {
                    Name = ".AspNetCore.Session.JobFilter2"
                }
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=CrawlSetting}/{action=Index}/{id?}");
            });
        }
    }
}
