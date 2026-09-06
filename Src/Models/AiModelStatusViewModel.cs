namespace ProposalIQ.Web.Models;

public class AiModelStatusViewModel
{
    public string Provider { get; set; } = string.Empty;
    public bool IsReady { get; set; } = true;
    public string Stage { get; set; } = "Ready";
    public int ProgressPercent { get; set; } = 100;
    public string Message { get; set; } = string.Empty;
    public bool IsFaulted { get; set; } = false;
}
