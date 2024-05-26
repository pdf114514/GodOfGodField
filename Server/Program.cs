using GodOfGodField.Server.Hubs;
using GodOfGodField.Shared;
using Microsoft.AspNetCore.ResponseCompression;

namespace GodOfGodField;

public class Program {
    public static void Main(string[] args) {
        if (args.Contains("--update")) {
            Console.WriteLine("Updating resources...");
            Resources.UpdateResources().Wait();
            Console.WriteLine("Updated resources.");
            return;
        } else if (args.Contains("--supported-languages")) {
            Console.WriteLine(string.Join(", ", Client.Localization.SupportedLanguages));
            return;
        } else if (args.Contains("--show-resources")) {
            Console.WriteLine(string.Join('\n', typeof(Resources).Assembly.GetManifestResourceNames()));
            return;
        }

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddSignalR();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.UseWebAssemblyDebugging();
        } else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();


        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");
        app.MapHub<HiddenGameHub>(HiddenGameHub.HubUrl);

        app.Run();
    }
}
