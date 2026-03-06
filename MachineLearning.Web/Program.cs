using Government.Module;
using Company.Module;
using Household.Module;
using Household.Leisure.Spending.Module;
using Household.Remittance.Module;
using Household.Salary.Distribution.Module;
using MachineLearning.Web.Components;
using Price.Module;
using Bank.Module;
using Transportation.Module;
using Simulation.Module;
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
    (IHouseholdSalaryDistributionModule)sp.GetRequiredService<IHouseholdModule>());
builder.Services.AddScoped<IHouseholdLeisureSpendingModule, HouseholdLeisureSpendingModule>();
builder.Services.AddScoped<IHouseholdRemittanceModule, HouseholdRemittanceModule>();
builder.Services.AddScoped<IBankModule, BankModule>();
builder.Services.AddScoped<ITransportationModule, TransportationModule>();

builder.Services.AddScoped<ISimulationModule, SimulationModule>();
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
