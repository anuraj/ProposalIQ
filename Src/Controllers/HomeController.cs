using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProposalIQ.Web.Models;
using ProposalIQ.Web.Services;

namespace ProposalIQ.Web.Controllers;

public class HomeController(IProposalTextExtractor textExtractor,
    IProposalAnalysisService analysisService) : Controller
{
    private readonly IProposalTextExtractor _textExtractor = textExtractor;
    private readonly IProposalAnalysisService _analysisService = analysisService;

    public IActionResult Index()
    {
        return View();
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
