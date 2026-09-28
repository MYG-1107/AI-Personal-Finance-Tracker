import os, subprocess

os.makedirs('AIPersonalFinanceTracker.Client/wwwroot', exist_ok=True)
os.makedirs('AIPersonalFinanceTracker.Client/Layout', exist_ok=True)
os.makedirs('AIPersonalFinanceTracker.Client/Pages', exist_ok=True)

# 1. index.html
with open('AIPersonalFinanceTracker.Client/wwwroot/index.html', 'w') as f:
    f.write('''<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>AI Personal Finance Tracker</title>
    <base href="/" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" />
</head>
<body>
    <div id="app">
        <div class="d-flex justify-content-center align-items-center vh-100">
            <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
                <span class="visually-hidden">Loading Blazor WebAssembly...</span>
            </div>
        </div>
    </div>

    <div id="blazor-error-ui" style="display: none;">
        An unhandled error has occurred.
        <a href="" class="reload">Reload</a>
    </div>
    <script src="_framework/blazor.webassembly.js"></script>
</body>
</html>
''')

# 2. Program.cs
with open('AIPersonalFinanceTracker.Client/Program.cs', 'w') as f:
    f.write('''using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AIPersonalFinanceTracker.Client;
using AIPersonalFinanceTracker.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<CurrencyService>();

await builder.Build().RunAsync();
''')

# 3. App.razor
with open('AIPersonalFinanceTracker.Client/App.razor', 'w') as f:
    f.write('''@using Microsoft.AspNetCore.Components.Routing

<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(Layout.MainLayout)">
            <p role="alert" class="p-4">Page not found.</p>
        </LayoutView>
    </NotFound>
</Router>
''')

# 4. Layout/MainLayout.razor
with open('AIPersonalFinanceTracker.Client/Layout/MainLayout.razor', 'w') as f:
    f.write('''@inherits LayoutComponentBase

<div class="page">
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark mb-4">
        <div class="container-fluid">
            <a class="navbar-brand fw-bold" href="#">AI Personal Finance Tracker</a>
        </div>
    </nav>
    <main class="container-fluid px-4">
        @Body
    </main>
</div>
''')

# 5. Add page routing for '/' and '/transactions' in Transactions.razor
transactions_file = 'AIPersonalFinanceTracker.Client/Pages/Transactions.razor'
if os.path.exists(transactions_file):
    with open(transactions_file, 'r') as f:
        content = f.read()
    if '@page "/"' not in content:
        content = '@page "/"\n' + content
    with open(transactions_file, 'w') as f:
        f.write(content)

print("Client Blazor WASM frontend files created successfully.")
