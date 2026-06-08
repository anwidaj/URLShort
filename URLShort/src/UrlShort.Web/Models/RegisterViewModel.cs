using System.ComponentModel.DataAnnotations;

namespace UrlShort.Web.Models;

public class RegisterViewModel{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Username must be between 2 and 30 characters")]
    [Display(Name = "Username")]
    public string Username {get; set;} = string.Empty;


    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password {get; set;} = string.Empty;

    [Required(ErrorMessage = "Confirm Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Confirm Password must be between 8 and 100 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword {get; set;} = string.Empty;
    
}