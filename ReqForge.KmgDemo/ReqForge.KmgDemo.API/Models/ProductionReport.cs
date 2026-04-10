namespace ReqForge.KmgDemo.Models;

public class ProductionReport
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public DateTime Date { get; set; }
    public decimal OilTons { get; set; }
    public decimal GasThousandM3 { get; set; }
    public string Comment { get; set; } = string.Empty;
}