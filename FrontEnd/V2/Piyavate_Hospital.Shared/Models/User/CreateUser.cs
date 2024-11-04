namespace Piyavate_Hospital.Shared.Models.User;

// public record CreateUser(
//     string Account="",
//     string Password=""
//     );
public class CreateUser
{
    public string Account { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}