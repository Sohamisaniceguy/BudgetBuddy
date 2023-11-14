using BudgetBuddy.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebGoatCore.Utils;

namespace BudgetBuddy.Test
{
    public class Argon
    {
        private readonly Argon2Hasher<User> _hasher = new Argon2Hasher<User>();

        [Fact]
        public void HashPassword_ShouldGenerateHash()
        {
            // Arrange
            var user = new User { /* User properties */ };
            var password = "TestPassword123";

            // Act
            var hash = _hasher.HashPassword(user, password);

            // Assert
            Assert.NotNull(hash);
        }

        [Fact]
        public void VerifyHashedPassword_ShouldVerifyCorrectPassword()
        {
            // Arrange
            var user = new User { /* User properties */ };
            var password = "TestPassword123";
            var hash = _hasher.HashPassword(user, password);

            // Act
            var result = _hasher.VerifyHashedPassword(user, hash, password);

            // Assert
            Assert.Equal(PasswordVerificationResult.Success, result);
        }

        [Fact]
        public void VerifyHashedPassword_ShouldNotVerifyIncorrectPassword()
        {
            // Arrange
            var user = new User { /* User properties */ };
            var password = "TestPassword123";
            var wrongPassword = "WrongPassword123";
            var hash = _hasher.HashPassword(user, password);

            // Act
            var result = _hasher.VerifyHashedPassword(user, hash, wrongPassword);

            // Assert
            Assert.NotEqual(PasswordVerificationResult.Success, result);
        }
    }

}
