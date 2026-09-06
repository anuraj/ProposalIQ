using ProposalIQ.Web.Configuration;
using ProposalIQ.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var aiOptions = builder.Configuration.GetSection("Ai")
    .Get<AiProviderOptions>() ?? new AiProviderOptions();

var foundryLocalState = new FoundryLocalModelState();
var chatClient = ChatClientFactory.Create(aiOptions, foundryLocalState);

builder.Services.AddChatClient(chatClient);
builder.Services.AddSingleton(aiOptions);
builder.Services.AddSingleton(foundryLocalState);

builder.Services.Configure<AnalysisCacheOptions>(
    builder.Configuration.GetSection(AnalysisCacheOptions.SectionName));
builder.Services.AddSingleton<IProposalAnalysisCache, SqliteProposalAnalysisCache>();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IProposalAnalysisService, ProposalAnalysisService>();
builder.Services.AddScoped<IProposalTextExtractor, ProposalTextExtractor>();

if (aiOptions.Provider == AiProvider.FoundryLocal)
{
    builder.Services.AddHostedService(sp => new FoundryLocalModelPreparationService(
        aiOptions.FoundryLocal,
        foundryLocalState,
        sp.GetRequiredService<ILogger<FoundryLocalModelPreparationService>>()));
}

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
