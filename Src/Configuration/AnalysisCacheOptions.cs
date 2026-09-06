namespace ProposalIQ.Web.Configuration;

public class AnalysisCacheOptions
{
    public const string SectionName = "AnalysisCache";

    public bool Enabled { get; set; } = true;

    public string ConnectionString { get; set; } = "Data Source=proposaliq_cache.db";
}
