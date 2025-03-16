using dj_api.Controllers;
using dj_api.Models;
using dj_api.Repositories;
using dj_api.Authentication;
using dj_api.ApiModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;
using System.Text.Json;

namespace DjTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly AuthController _controller;
        private readonly JsonSerializerOptions _jsonOptions;

        private class LoginResponse
        {
            public string token { get; set; }
            public UserResponse user { get; set; }
        }

        private class UserResponse
        {
            public string ObjectId { get; set; }
            public string name { get; set; }
            public string username { get; set; }
            public string email { get; set; }
            public string familyName { get; set; }
            public string imageUrl { get; set; }
        }

        private class ErrorResponse
        {
            public string error { get; set; }
        }

        public AuthControllerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTokenService = new Mock<ITokenService>();
            _controller = new AuthController(_mockUserRepository.Object, _mockTokenService.Object);
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public async Task UserLogin_WithValidCredentials_ReturnsOkWithToken()
        {
            var loginModel = new LoginModel
            {
                username = "testuser",
                password = "testpass"
            };

            var user = new User
            {
                ObjectId = "123",
                Name = "Test User",
                Username = loginModel.username,
                Email = "test@test.com",
                FamilyName = "Test Family",
                ImageUrl = "test.jpg"
            };

            var token = "test-token";

            _mockUserRepository.Setup(repo => repo.Authenticate(loginModel.username, loginModel.password))
                             .ReturnsAsync(user);
            _mockTokenService.Setup(service => service.GenerateJwtToken(user))
                           .Returns(token);

            var result = await _controller.UserLogin(loginModel);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var jsonString = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<LoginResponse>(jsonString, _jsonOptions);

            Assert.NotNull(response);
            Assert.Equal(token, response.token);
            Assert.Equal(user.ObjectId, response.user.ObjectId);
            Assert.Equal(user.Name, response.user.name);
            Assert.Equal(user.Email, response.user.email);
        }

        [Fact]
        public async Task UserLogin_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var loginModel = new LoginModel
            {
                username = "wrongusername",
                password = "wrongpassword"
            };

            _mockUserRepository.Setup(repo => repo.Authenticate(loginModel.username, loginModel.password))
                             .ReturnsAsync((User)null);

            var result = await _controller.UserLogin(loginModel);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var jsonString = JsonSerializer.Serialize(unauthorizedResult.Value);
            var response = JsonSerializer.Deserialize<ErrorResponse>(jsonString, _jsonOptions);

            Assert.NotNull(response);
            Assert.Equal("Invalid username or password", response.error);
        }

        [Fact]
        public async Task Register_WithNewUser_ReturnsOkWithToken()
        {
            var registerModel = new RegisterModel
            {
                name = "Test User",
                familyName = "Test Family",
                imageUrl = "test.jpg",
                username = "testuser",
                email = "test@test.com",
                password = "testpass"
            };

            _mockUserRepository.Setup(repo => repo.FindUserByEmail(registerModel.email))
                             .ReturnsAsync((User)null);
            _mockUserRepository.Setup(repo => repo.FindUserByUsername(registerModel.username))
                             .ReturnsAsync((User)null);

            var token = "test-token";
            _mockTokenService.Setup(service => service.GenerateJwtToken(It.IsAny<User>()))
                           .Returns(token);

            var result = await _controller.UserRegister(registerModel);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var jsonString = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<LoginResponse>(jsonString, _jsonOptions);

            Assert.NotNull(response);
            Assert.Equal(token, response.token);
            Assert.NotNull(response.user.username);
            Assert.Equal(registerModel.name, response.user.name);
            Assert.Equal(registerModel.email, response.user.email);
        }
    }
}