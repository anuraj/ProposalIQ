using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProposalIQ.Web.Configuration;
using ProposalIQ.Web.Models;
using ProposalIQ.Web.Services;

namespace ProposalIQ.Web.Controllers;

public class HomeController(
    IProposalTextExtractor textExtractor,
    IProposalAnalysisService analysisService,
    AiProviderOptions? aiOptions = null,
    FoundryLocalModelState? foundryLocalState = null) : Controller
{
    private readonly IProposalTextExtractor _textExtractor = textExtractor;
    private readonly IProposalAnalysisService _analysisService = analysisService;
    private readonly AiProviderOptions _aiOptions = aiOptions ?? new AiProviderOptions();
    private readonly FoundryLocalModelState _foundryLocalState = foundryLocalState ?? new FoundryLocalModelState();

    public IActionResult Index()
    {
        ViewBag.ModelStatus = GetCurrentModelStatus();
        return View();
    }

    [HttpGet]
    public IActionResult ModelStatus()
    {
        return Json(GetCurrentModelStatus());
    }

    private AiModelStatusViewModel GetCurrentModelStatus()
    {
        if (_aiOptions.Provider != AiProvider.FoundryLocal)
        {
            return new AiModelStatusViewModel
            {
                Provider = _aiOptions.Provider.ToString(),
                IsReady = true,
                Stage = "Ready",
                ProgressPercent = 100,
                Message = "Ready"
            };
        }

        var isReady = _foundryLocalState.IsReady;
        var fault = _foundryLocalState.Fault;
        var stage = _foundryLocalState.Stage;
        var progress = _foundryLocalState.ProgressPercent;
        var message = _foundryLocalState.StatusMessage;

        return new AiModelStatusViewModel
        {
            Provider = "FoundryLocal",
            IsReady = isReady,
            IsFaulted = fault != null,
            Stage = stage.ToString(),
            ProgressPercent = progress,
            Message = message
        };
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Analyze(
    AnalyzeProposalRequest request,
    CancellationToken cancellationToken)
    {
        if (request.ProposalFile == null ||
            request.ProposalFile.Length == 0)
        {
            ModelState.AddModelError(
                "ProposalFile",
                "Please upload a proposal.");

            return View("Index", request);
        }

        try
        {
            var proposalText =
                await _textExtractor.ExtractTextAsync(
                    request.ProposalFile,
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(proposalText))
            {
                ModelState.AddModelError(
                    "ProposalFile",
                    "No readable text was found in the proposal.");

                return View("Index", request);
            }

            const int maxCharacters = 100_000;

            if (proposalText.Length > maxCharacters)
            {
                proposalText = proposalText[..maxCharacters];
            }

            var result =
                await _analysisService.AnalyzeAsync(
                    proposalText,
                    request,
                    cancellationToken);

            return View("Analysis", result);
        }
        catch (OperationCanceledException)
        {
            return BadRequest("Analysis was cancelled.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                "ProposalFile",
                $"Analysis failed: {ex.Message}");

            return View("Index", request);
        }
    }
}
