namespace ReqForge.KmgDemo.DTOs;

public class DashboardSummaryDto
{
    public int ActiveFields { get; set; }
    public int TotalProductionReports { get; set; }
    public decimal TotalOilTonsLast7Days { get; set; }
    public decimal TotalGasThousandM3Last7Days { get; set; }
    public int OpenIncidents { get; set; }
    public IReadOnlyList<IncidentBriefDto> RecentIncidents { get; set; } = Array.Empty<IncidentBriefDto>();
}

public class IncidentBriefDto
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
