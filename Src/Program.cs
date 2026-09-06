using Microsoft.Extensions.AI;
using ProposalIQ.Web.Configuration;
using ProposalIQ.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AiProviderOptions>(
    builder.Configuration.GetSection("Ai"));

var foundryLocalState = new FoundryLocalModelState();
builder.Services.AddSingleton(foundryLocalState);
builder.Services.AddSingleton<IAiConfigurationService, JsonFileAiConfigurationService>();
builder.Services.AddSingleton<IFoundryLocalModelManager, FoundryLocalModelManager>();
builder.Services.AddSingleton<IChatClient, DynamicChatClient>();

builder.Services.AddHostedService<FoundryLocalModelPreparationService>();

builder.Services.Configure<AnalysisCacheOptions>(
    builder.Configuration.GetSection(AnalysisCacheOptions.SectionName));
builder.Services.AddSingleton<IProposalAnalysisCache, SqliteProposalAnalysisCache>();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IProposalAnalysisService, ProposalAnalysisService>();
builder.Services.AddScoped<IProposalTextExtractor, ProposalTextExtractor>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
