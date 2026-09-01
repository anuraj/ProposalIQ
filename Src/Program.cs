using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using ProposalIQ.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var openAiApiKey = builder.Configuration["OpenRouter:ApiKey"];

if (string.IsNullOrWhiteSpace(openAiApiKey))
{
    throw new InvalidOperationException("OpenAI API key is not configured.");
}

var chatClient = new ChatClient("openrouter/auto",
                    new ApiKeyCredential(openAiApiKey),
                    new OpenAIClientOptions
                    {
                        Endpoint = new Uri("https://openrouter.ai/api/v1/")
                    }).AsIChatClient();

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
