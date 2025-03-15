using dj_api.Controllers;
using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace DjTests
{
    public class PlaylistControllerTests
    {
        private readonly Mock<IPlaylistRepository> _mockRepository;
        private readonly PlaylistController _controller;

        public PlaylistControllerTests()
        {
            _mockRepository = new Mock<IPlaylistRepository>();
            _controller = new PlaylistController(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAllPlayList_ReturnsOkResultWithPlaylists()
        {
            var expectedPlaylists = new List<Playlist>
            {
                new Playlist
                {
                    ObjectId = "playlist123",
                    UserID = "user123",
                    MusicList = new string[] { "song1", "song2" }
                },
                new Playlist
                {
                    ObjectId = "playlist456",
                    UserID = "user456",
                    MusicList = new string[] { "song3", "song4" }
                }
            };

            _mockRepository.Setup(repo => repo.GetAllPlaylist())
                         .ReturnsAsync(expectedPlaylists);

            var result = await _controller.GetAllPlayList();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<Playlist>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal(expectedPlaylists[0].ObjectId, returnValue[0].ObjectId);
            Assert.Equal(expectedPlaylists[0].UserID, returnValue[0].UserID);
            Assert.Equal(expectedPlaylists[0].MusicList, returnValue[0].MusicList);
            Assert.Equal(expectedPlaylists[1].ObjectId, returnValue[1].ObjectId);
            Assert.Equal(expectedPlaylists[1].UserID, returnValue[1].UserID);
            Assert.Equal(expectedPlaylists[1].MusicList, returnValue[1].MusicList);
        }
    }
}