using dj_api.Controllers;
using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace DjTests
{
    public class AnalyticsControllerTests
    {
        private readonly Mock<ISongPlayRepository> _mockRepository;
        private readonly AnalyticsController _controller;

        public AnalyticsControllerTests()
        {
            _mockRepository = new Mock<ISongPlayRepository>();
            _controller = new AnalyticsController(_mockRepository.Object);
        }

        [Fact]
        public async Task RecordSongPlay_ValidSongId_ReturnsOkResult()
        {
            string testSongId = "test123";
            _mockRepository.Setup(repo => repo.RecordPlayAsync(testSongId))
                         .Returns(Task.CompletedTask);

            var result = await _controller.RecordSongPlay(testSongId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseObj = okResult.Value;
            var properties = responseObj.GetType().GetProperties();
            var messageProperty = properties.FirstOrDefault(p => p.Name == "Message");
            Assert.NotNull(messageProperty);
            var message = messageProperty.GetValue(responseObj) as string;
            Assert.Equal("Song play recorded successfully.", message);
            _mockRepository.Verify(repo => repo.RecordPlayAsync(testSongId), Times.Once());
        }

        [Fact]
        public async Task GetMostPlayedSong_ReturnsOkWithSongDetails()
        {
            var mostPlayedSong = new Song
            {
                ObjectId = "song123",
                Name = "Test Song",
                Artist = "Test Artist",
                Genre = "Rock",
                AddedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(repo => repo.GetMostPLayedSongAsync())
                         .ReturnsAsync(mostPlayedSong);

            var result = await _controller.GetMostPlayedSong();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseObj = okResult.Value;
            var properties = responseObj.GetType().GetProperties();

            var objectIdProp = properties.FirstOrDefault(p => p.Name == "ObjectId");
            var nameProp = properties.FirstOrDefault(p => p.Name == "Name");
            var artistProp = properties.FirstOrDefault(p => p.Name == "Artist");
            var genreProp = properties.FirstOrDefault(p => p.Name == "Genre");

            Assert.NotNull(objectIdProp);
            Assert.NotNull(nameProp);
            Assert.NotNull(artistProp);
            Assert.NotNull(genreProp);

            Assert.Equal(mostPlayedSong.ObjectId, objectIdProp.GetValue(responseObj) as string);
            Assert.Equal(mostPlayedSong.Name, nameProp.GetValue(responseObj) as string);
            Assert.Equal(mostPlayedSong.Artist, artistProp.GetValue(responseObj) as string);
            Assert.Equal(mostPlayedSong.Genre, genreProp.GetValue(responseObj) as string);
        }
    }
}