using ReqForge.KmgDemo.Models;

namespace ReqForge.KmgDemo.Data;

public class InMemoryStore
{
    private readonly object _sync = new();

    public List<Field> Fields { get; } =
    [
        new Field { Id = 1, Name = "Тенгиз", Region = "Атырауская область", IsActive = true },
        new Field { Id = 2, Name = "Кашаган", Region = "Каспийский шельф", IsActive = true },
        new Field { Id = 3, Name = "Карачаганак", Region = "Западно-Казахстанская область", IsActive = true }
    ];

    public List<ProductionReport> ProductionReports { get; } =
    [
        new ProductionReport
        {
            Id = 1,
            FieldId = 1,
            Date = DateTime.UtcNow.Date.AddDays(-1),
            OilTons = 12500,
            GasThousandM3 = 840,
            Comment = "План выполнен"
        }
    ];

    public List<Incident> Incidents { get; } =
    [
        new Incident
        {
            Id = 1,
            FieldId = 2,
            Title = "Скачок давления на линии",
            Severity = "medium",
            Status = "in_progress",
            CreatedAt = DateTime.UtcNow.AddHours(-5)
        }
    ];

    public Field? GetField(int id) => Fields.FirstOrDefault(f => f.Id == id);

    public ProductionReport? GetProductionReport(int id) => ProductionReports.FirstOrDefault(r => r.Id == id);

    public Incident? GetIncident(int id) => Incidents.FirstOrDefault(i => i.Id == id);

    public ProductionReport AddProductionReport(ProductionReport report)
    {
        lock (_sync)
        {
            report.Id = ProductionReports.Count == 0 ? 1 : ProductionReports.Max(r => r.Id) + 1;
            ProductionReports.Add(report);
            return report;
        }
    }

    public Incident AddIncident(Incident incident)
    {
        lock (_sync)
        {
            incident.Id = Incidents.Count == 0 ? 1 : Incidents.Max(i => i.Id) + 1;
            Incidents.Add(incident);
            return incident;
        }
    }

    public bool TryUpdateIncidentStatus(int id, string status, out Incident? incident)
    {
        lock (_sync)
        {
            incident = Incidents.FirstOrDefault(i => i.Id == id);
            if (incident == null)
                return false;

            incident.Status = status;
            return true;
        }
    }
}