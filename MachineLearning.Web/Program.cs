using Government.Module;
using Company.Module;
using Household.Module;
using Household.Salary.Distribution.Module;
using MachineLearning.Web.Components;
using MachineLearning.Web.Models.Simulation;
using Price.Module;
var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Enregistrer les modules métier comme services injectables
builder.Services.AddScoped<IGovernmentModule, GovernmentModule>();
builder.Services.AddScoped<ICompanyModule, CompanyModule>();
builder.Services.AddScoped<IPriceModule, PriceModule>();
builder.Services.AddScoped<IHouseholdModule, HouseholdModule>();
builder.Services.AddScoped<IHouseholdSalaryDistributionModule>(sp =>
    new HouseholdSalaryDistributionModule(
        salaireMedian: 170_000,
        sigma: 0.85,
        salairePlancher: 50_000
    ));

builder.Services.AddScoped<EconomicSimulatorViewModel>();
builder.Services.AddOutputCache();
/*
builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });
*/
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
