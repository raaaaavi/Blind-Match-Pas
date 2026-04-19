namespace BlindMatchPAS.Web.ViewModels.Account;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
// New changes

public class RegisterViewModel
{
    [Required, StringLength(120)]
    [Display(Name = "Group lead / student name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(40)]
    [Display(Name = "Student registration number")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Programme { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Group / team name")]
    public string GroupName { get; set; } = string.Empty;

    [Required, StringLength(200)]
    [Display(Name = "Team members")]
    public string TeamMemberNames { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
