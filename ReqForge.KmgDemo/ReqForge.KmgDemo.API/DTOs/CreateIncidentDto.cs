using System.ComponentModel.DataAnnotations;

namespace ReqForge.KmgDemo.DTOs;

public class CreateIncidentDto
{
    [Range(1, int.MaxValue)]
    public int FieldId { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [RegularExpression("^(low|medium|high)$", ErrorMessage = "Severity must be low, medium, or high.")]
    public string Severity { get; set; } = "low";
}