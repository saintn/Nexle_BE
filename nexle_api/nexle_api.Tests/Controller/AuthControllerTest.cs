using Xunit;
using Microsoft.AspNetCore.Mvc;
using nexle_api.Controllers;
using nexle_api.Data;
using nexle_api.Models;
using nexle_api.Models.Dtos.Signup;
using nexle_api.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Configuration;

namespace nexle_api.Tests.Controllers
{
    public class AuthControllerTest
    {
        private JwtTokenService GetJwtService()
        {
            var inMemorySettings = new Dictionary<string, string> {
        {"JwtSettings:Secret", "test-secret-key"}
        };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            return new JwtTokenService(configuration);
        }

        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            return context;
        }

        [Fact]
        public void Signup_ReturnsBadRequest_WhenEmailAlreadyExists()
        {
            var context = GetDbContext();
            context.Users.Add(new User
            {
                Email = "existing@example.com",
                FirstName = "John",
                LastName = "Doe",
                Hash = "hashed-password"
            });
            context.SaveChanges();

            var jwtService = GetJwtService();
            var controller = new AuthController(context, jwtService);

            var request = new SignupRequest
            {
                Email = "existing@example.com",
                Password = "Password123!",
                FirstName = "Jane",
                LastName = "Smith"
            };

            var result = controller.Signup(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email is already in use.", badRequest.Value);
        }
    }
}
