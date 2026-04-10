using System.ComponentModel.DataAnnotations;

namespace ReqForge.KmgDemo.DTOs;

public class CreateProductionReportDto
{
    [Range(1, int.MaxValue)]
    public int FieldId { get; set; }

    public DateTime Date { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OilTons { get; set; }

    [Range(0, double.MaxValue)]
    public decimal GasThousandM3 { get; set; }

    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;
}