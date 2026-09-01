# Proposal IQ

ProposalIQ is an ASP.NET Core MVC web app that uses AI to review business proposals for risk before you send them to a client. Upload a proposal document and get back a structured risk analysis from the seller's perspective — covering scope, commercial terms, timeline, and delivery risk.

## Features

- **Document upload & text extraction** — supports `.txt`, `.docx`, and `.pdf` proposal files (via `DocumentFormat.OpenXml` and `UglyToad.PdfPig`).
- **AI-powered risk analysis** — sends the extracted proposal text, along with optional deal context (project value, hourly rate, additional notes), to an LLM and returns a structured result via `Microsoft.Extensions.AI`.
- **Structured findings** — overall risk rating, executive summary, per-category risk (scope, commercial, timeline, delivery), and a detailed list of issues with evidence and recommendations.
- **Seller-focused review** — flags common pitfalls such as unlimited revisions, vague deliverables, undefined acceptance criteria, payment-only-on-completion terms, unrealistic timelines, and undefined responsibilities.

## Project structure

```
Src/                      ASP.NET Core MVC web application (ProposalIQ.Web)
  Controllers/            HomeController — handles upload & analysis requests
  Models/                 Request/response models (AnalyzeProposalRequest, ProposalAnalysisResult, etc.)
  Services/               Text extraction (IProposalTextExtractor) and AI analysis (IProposalAnalysisService)
  Views/                  Razor views (Index, Analysis, Privacy, shared layout)
  wwwroot/                Static assets (CSS, JS, Bootstrap, jQuery)
Tests/                    ProposalIQ.Web.Tests test project
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- An [OpenRouter](https://openrouter.ai/) API key (used for AI chat completions)

## Configuration

The app requires an OpenRouter API key, configured via .NET user secrets (or another configuration provider) under `OpenRouter:ApiKey`.

From the `Src` folder:

```powershell
dotnet user-secrets set "OpenRouter:ApiKey" "<your-api-key>"
```

## Running the app

```powershell
cd Src
dotnet run
```

Then open the URL shown in the console (see [Properties/launchSettings.json](Src/Properties/launchSettings.json)) in your browser, upload a proposal file, optionally add deal context, and submit for analysis.

## Running tests

```powershell
cd Tests
dotnet test
```

## Tech stack

- ASP.NET Core MVC (.NET 10)
- `Microsoft.Extensions.AI` / `Microsoft.Extensions.AI.OpenAI` for LLM integration (via OpenRouter)
- `DocumentFormat.OpenXml` for `.docx` parsing
- `UglyToad.PdfPig` for `.pdf` parsing
- Bootstrap for UI