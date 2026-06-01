using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace OurBankApi.Models;

[PrimaryKey("Id")]
public class ApplicationUser : IdentityUser
{
    public override string Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}