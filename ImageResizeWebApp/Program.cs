using ImageResizeWebApp.Models;
using ImageResizeWebApp.Services;
using Azure.Identity;

namespace ImageResizeWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddApplicationInsightsTelemetry();
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(new Uri(Environment.GetEnvironmentVariable("ConfigEndpoint")), new DefaultAzureCredential());
            });
            // Add Storage Interop/Helper to the project
            var storageConfig = builder.Configuration.GetSection("AzureStorageConfig");
            builder.Services.AddScoped<IStorageInterop, StorageHelper>().Configure<AzureStorageConfig>(storageConfig);
            builder.Services.AddAzureAppConfiguration();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}