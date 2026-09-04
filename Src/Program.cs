using ProposalIQ.Web.Configuration;
using ProposalIQ.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var aiOptions = builder.Configuration.GetSection("Ai").Get<AiProviderOptions>() ?? new AiProviderOptions();

// Backward compatibility with the legacy top-level OpenRouter:ApiKey user-secret.
if (string.IsNullOrWhiteSpace(aiOptions.OpenRouter.ApiKey))
{
    aiOptions.OpenRouter.ApiKey = builder.Configuration["OpenRouter:ApiKey"] ?? string.Empty;
}

var chatClient = ChatClientFactory.Create(aiOptions);

builder.Services.AddChatClient(chatClient);

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
