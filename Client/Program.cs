using GodOfGodField.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class Program {
    public static async Task Main(string[] args) {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddScoped(sp => new Localization());
        builder.Services.AddScoped(sp => new ApplicationState(sp.GetRequiredService<IJSRuntime>(), sp.GetRequiredService<NavigationManager>()));
        builder.Services.AddScoped(sp => new ApiClient(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ApplicationState>()));
        builder.Services.AddScoped(sp => new FirestoreDB(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ApiClient>(), sp.GetRequiredService<ApplicationState>()));

        await builder.Build().RunAsync();
    }
}
