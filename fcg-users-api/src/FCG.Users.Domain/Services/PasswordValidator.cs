using System.Text.RegularExpressions;
using FCG.Users.Domain.Exceptions;

namespace FCG.Users.Domain.Services;

public static class PasswordValidator
{
    private static readonly Regex PasswordRegex =
        new(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&_\-#^])[A-Za-z\d@$!%*?&_\-#^]{8,}$",
            RegexOptions.Compiled);

    public static void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("Password cannot be empty.");

        if (!PasswordRegex.IsMatch(password))
            throw new DomainException(
                "Password must be at least 8 characters and contain letters, numbers, and special characters.");
    }

    public static bool IsValid(string password) => PasswordRegex.IsMatch(password ?? "");
}
