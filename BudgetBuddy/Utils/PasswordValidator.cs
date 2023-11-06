using System;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class PasswordValidator
    {
        public static Tuple<bool, string> Validate(string username, string password)
        {
            StringBuilder errors = new StringBuilder();
            bool isValid = true;

            if (password.Contains(username))
            {
                errors.Append("The password cannot contain your username.<br>");
                isValid = false;
            }

            if (password.Length < 8)
            {
                errors.Append("The password must be at least 8 characters long.<br>");
                isValid = false;
            }

            if (!password.Any(char.IsDigit))
            {
                errors.Append("The password must contain at least one digit.<br>");
                isValid = false;
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                errors.Append("The password must contain at least one non-alphanumeric character.<br>");
                isValid = false;
            }

            if (!(password.Any(char.IsUpper) && password.Any(char.IsLower)))
            {
                errors.Append("The password must include both lower and upper case letters.<br>");
                isValid = false;
            }

            if (password.Distinct().Count() < 5)
            {
                errors.Append("The password must contain at least five unique characters.<br>");
                isValid = false;
            }

            // Do not encode the error string as it contains HTML that needs to be rendered
            return Tuple.Create(isValid, errors.ToString());
        }
    }
}
