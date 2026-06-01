using System.ComponentModel.DataAnnotations;

namespace OurBankApi.Models.Configurations;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32)]
    public string Secret { get; set; } = string.Empty;
    [Required]
    public string ValidIssuer { get; set; } = string.Empty;
    [Required]
    public string ValidAudience { get; set; } = string.Empty;
}