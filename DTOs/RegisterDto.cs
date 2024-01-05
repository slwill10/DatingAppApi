#nullable disable

using System.ComponentModel.DataAnnotations;

namespace API;

public class RegisterDto
{
    [Required]
    public string Username {get; set;}
    
    [Required]
    public string password {get; set;}

}
