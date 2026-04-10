using System.ComponentModel.DataAnnotations;

namespace ReqForge.KmgDemo.DTOs;

public class UpdateIncidentStatusDto
{
    [Required]
    [RegularExpression("^(open|in_progress|resolved)$", ErrorMessage = "Status must be open, in_progress, or resolved.")]
    public string Status { get; set; } = string.Empty;
}
