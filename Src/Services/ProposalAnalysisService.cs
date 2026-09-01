using Microsoft.Extensions.AI;
using ProposalIQ.Web.Models;

namespace ProposalIQ.Web.Services;

public class ProposalAnalysisService(IChatClient chatClient) : IProposalAnalysisService
{
    private readonly IChatClient _chatClient = chatClient;

    public async Task<ProposalAnalysisResult> AnalyzeAsync(
        string proposalText,
        AnalyzeProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(proposalText))
        {
            throw new ArgumentException(
                "Proposal text cannot be empty.",
                nameof(proposalText));
        }

        var dealContext = BuildDealContext(request);

        var systemPrompt = """
            You are ProposalIQ, an AI proposal risk analyst.

            Your role is to review a business proposal from the perspective
            of the person SELLING the work.

            Analyze the proposal for potential risks in these areas:

            - Scope
            - Commercial terms
            - Timeline
            - Delivery
            - Dependencies
            - Ambiguous commitments
            - Missing protections

            Pay particular attention to:

            - Unlimited revisions
            - Vague deliverables
            - Undefined acceptance criteria
            - Payment only after completion
            - Unrealistic timelines
            - Customer dependencies
            - Undefined responsibilities
            - Unclear change-request handling
            - Vague support commitments
            - Pricing that appears inconsistent with the expected effort

            Rules:

            1. Only use information available in the proposal and deal context.
            2. Never invent facts.
            3. If something cannot be determined, use "Unknown".
            4. Every issue must include evidence from the proposal.
            5. Keep recommendations practical and concise.
            6. Analyze the proposal from the seller's perspective.
            7. Do not provide legal advice.
            """;

        var userPrompt = $"""
            DEAL CONTEXT
            {dealContext}

            PROPOSAL
            {proposalText}
            """;

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userPrompt)
        };

        var options = new ChatOptions
        {
            ModelId = "gpt-4o-mini",
            Temperature = 0.1f
        };

        var response =
            await _chatClient.GetResponseAsync<ProposalAnalysisResult>(
                messages,
                options,
                cancellationToken: cancellationToken);

        return response.Result;
    }

    private static string BuildDealContext(
        AnalyzeProposalRequest request)
    {
        var context = new List<string>();

        if (request.ProjectValue.HasValue)
        {
            context.Add(
                $"Proposed project value: {request.ProjectValue.Value:N2}");
        }

        if (request.HourlyRate.HasValue)
        {
            context.Add(
                $"Target hourly rate: {request.HourlyRate.Value:N2} per hour");
        }

        if (!string.IsNullOrWhiteSpace(request.AdditionalContext))
        {
            context.Add(
                $"Additional context: {request.AdditionalContext}");
        }

        return context.Count == 0
            ? "No additional deal context was provided."
            : string.Join(Environment.NewLine, context);
    }
}