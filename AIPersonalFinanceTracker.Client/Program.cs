using AIPersonalFinanceTracker.Client;
using AIPersonalFinanceTracker.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://jubilant-funicular-775r7vv6x6cwrrr-5000.app.github.dev/") 
});

builder.Services.AddScoped<FinanceApiService>();

await builder.Build().RunAsync();
