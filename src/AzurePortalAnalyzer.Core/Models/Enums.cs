namespace AzurePortalAnalyzer.Core.Models;

public enum DocumentStatus
{
    Draft,
    Processing,
    Generated,
    Failed,
    Archived
}

public enum DocumentType
{
    PortalAnalysisReport,
    ComplianceReport,
    SecurityAudit,
    CustomReport
}