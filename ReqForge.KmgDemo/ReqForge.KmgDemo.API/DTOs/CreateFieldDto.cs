using System.ComponentModel.DataAnnotations;

namespace ReqForge.KmgDemo.DTOs;

public class CreateFieldDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(120)]
    public string Region { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
