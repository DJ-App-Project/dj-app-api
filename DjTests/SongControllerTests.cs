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
    public class SongControllerTests
    {
        private readonly Mock<ISongRepository> _mockSongRepository;
        private readonly Mock<ISongPlayRepository> _mockSongPlayRepository;
        private readonly SongController _controller;

        public SongControllerTests()
        {
            _mockSongRepository = new Mock<ISongRepository>();
            _mockSongPlayRepository = new Mock<ISongPlayRepository>();
            _controller = new SongController(
                _mockSongRepository.Object,
                _mockSongPlayRepository.Object
            );

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
        public async Task GetSongById_ExistingId_ReturnsOkResult()
        {
            var songId = "testSongId";
            var testSong = new Song
            {
                ObjectId = songId,
                Name = "Test Song",
                Artist = "Test Artist",
                Genre = "Test Genre",
                AddedAt = DateTime.UtcNow
            };

            _mockSongRepository.Setup(repo => repo.GetSongByIdAsync(songId))
                .ReturnsAsync(testSong);

            var result = await _controller.GetSongById(songId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSong = Assert.IsType<Song>(okResult.Value);

            Assert.Equal(testSong.ObjectId, returnedSong.ObjectId);
            Assert.Equal(testSong.Name, returnedSong.Name);
            Assert.Equal(testSong.Artist, returnedSong.Artist);
            Assert.Equal(testSong.Genre, returnedSong.Genre);
        }

        [Fact]
        public async Task GetSongById_NonExistingId_ReturnsNotFound()
        {
            var songId = "nonExistingSongId";
            _mockSongRepository.Setup(repo => repo.GetSongByIdAsync(songId))
                .ReturnsAsync((Song)null);

            var result = await _controller.GetSongById(songId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteSong_ExistingSong_ReturnsOkResult()
        {
            var songId = "testSongId";
            var testSong = new Song
            {
                ObjectId = songId,
                Name = "Test Song",
                Artist = "Test Artist",
                Genre = "Test Genre"
            };

            _mockSongRepository.Setup(repo => repo.GetSongByIdAsync(songId))
                .ReturnsAsync(testSong);
            _mockSongRepository.Setup(repo => repo.DeleteSongAsync(songId))
                .Returns(Task.CompletedTask);

            var result = await _controller.DeleteSong(songId);

            Assert.IsType<OkResult>(result);
            _mockSongRepository.Verify(repo => repo.DeleteSongAsync(songId), Times.Once());
        }

        [Fact]
        public async Task DeleteSong_NonExistingSong_ReturnsNotFound()
        {
            var songId = "nonExistingSongId";
            _mockSongRepository.Setup(repo => repo.GetSongByIdAsync(songId))
                .ReturnsAsync((Song)null);

            var result = await _controller.DeleteSong(songId);

            Assert.IsType<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.Equal("Song not found", notFoundResult.Value);
            _mockSongRepository.Verify(repo => repo.DeleteSongAsync(songId), Times.Never());
        }
    }
}