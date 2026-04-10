using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReqForge.KmgDemo.Data;
using ReqForge.KmgDemo.DTOs;

namespace ReqForge.KmgDemo.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "operator,manager")]
public class DashboardController : ControllerBase
{
    private readonly InMemoryStore _store;

    public DashboardController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpGet("summary")]
    public ActionResult<DashboardSummaryDto> GetSummary()
    {
        var from = DateTime.UtcNow.Date.AddDays(-7);

        var reportsInWindow = _store.ProductionReports
            .Where(r => r.Date >= from)
            .ToList();

        var openStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "open", "in_progress" };

        var recent = _store.Incidents
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .Select(i => new IncidentBriefDto
            {
                Id = i.Id,
                FieldId = i.FieldId,
                Title = i.Title,
                Severity = i.Severity,
                Status = i.Status,
                CreatedAt = i.CreatedAt
            })
            .ToList();

        var dto = new DashboardSummaryDto
        {
            ActiveFields = _store.Fields.Count(f => f.IsActive),
            TotalProductionReports = _store.ProductionReports.Count,
            TotalOilTonsLast7Days = reportsInWindow.Sum(r => r.OilTons),
            TotalGasThousandM3Last7Days = reportsInWindow.Sum(r => r.GasThousandM3),
            OpenIncidents = _store.Incidents.Count(i => openStatuses.Contains(i.Status)),
            RecentIncidents = recent
        };

        return Ok(dto);
    }
}
