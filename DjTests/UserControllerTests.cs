using dj_api.Controllers;
using dj_api.Models;
using dj_api.Repositories;
using dj_api.ApiModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace DjTests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _controller = new UserController(_mockUserRepository.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "testUserId"),
                new Claim(ClaimTypes.Name, "Test User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetUserById_ExistingId_ReturnsOkResult()
        {
            var userId = "testUserId";
            var testUser = new User
            {
                ObjectId = userId,
                Name = "Test User",
                FamilyName = "Test Family",
                ImageUrl = "http://test.com/image.jpg",
                Username = "testuser",
                Email = "test@test.com",
                Password = "password123",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.MinValue
            };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(testUser);

            var result = await _controller.GetUserById(userId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<User>(okResult.Value);

            Assert.Equal(testUser.ObjectId, returnedUser.ObjectId);
            Assert.Equal(testUser.Name, returnedUser.Name);
            Assert.Equal(testUser.Email, returnedUser.Email);
            Assert.Equal(testUser.Username, returnedUser.Username);
        }

        [Fact]
        public async Task GetUserById_NonExistingId_ReturnsNotFound()
        {
            var userId = "nonExistingId";
            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync((User)null);

            var result = await _controller.GetUserById(userId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found", notFoundResult.Value);
        }

        [Fact]
        public async Task CreateUser_DuplicateEmail_ReturnsConflict()
        {
            var userData = new UserModel
            {
                name = "Test User",
                familyName = "Test Family",
                imageUrl = "http://test.com/image.jpg",
                username = "testuser",
                email = "existing@test.com",
                password = "password123"
            };

            var existingUser = new User
            {
                ObjectId = "existingUserId",
                Email = userData.email
            };

            _mockUserRepository.Setup(repo => repo.FindUserByEmail(userData.email))
                .ReturnsAsync(existingUser);

            var result = await _controller.CreateUser(userData);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Email is already registered.", conflictResult.Value);
        }
    }
}