using dj_api.Controllers;
using dj_api.Models;
using dj_api.Repositories;
using dj_api.ApiModels.Event.Post;
using dj_api.ApiModels.Event.Get;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace DjTests
{
    public class EventControllerTests
    {
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ISongRepository> _mockSongRepository;
        private readonly EventController _controller;
        private readonly JsonSerializerOptions _jsonOptions;

        public EventControllerTests()
        {
            _mockEventRepository = new Mock<IEventRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockSongRepository = new Mock<ISongRepository>();
            _controller = new EventController(
                _mockEventRepository.Object,
                _mockUserRepository.Object,
                _mockSongRepository.Object
            );
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };


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
        public async Task CreateEvent_ValidData_ReturnsOkResult()
        {
            var eventData = new CreateEventPost
            {
                Name = "Test Event",
                Description = "Test Description",
                Date = DateTime.UtcNow.AddDays(1),
                Location = "Test Location",
                Active = false
            };

            var createdEvent = new Event
            {
                ObjectId = "testEventId",
                Name = eventData.Name,
                Description = eventData.Description,
                Date = eventData.Date,
                Location = eventData.Location,
                Active = eventData.Active,
                DJId = "testUserId",
                MusicConfig = new Event.MusicConfigClass()
            };

            _mockEventRepository.Setup(repo => repo.CreateEventAsync(It.IsAny<Event>()))
                .Callback<Event>(e =>
                {
                    e.ObjectId = createdEvent.ObjectId;
                })
                .Returns(Task.CompletedTask);

            var result = await _controller.CreateEvent(eventData);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CreateEventResponse>(okResult.Value);

            Assert.NotNull(response);
            Assert.Equal("Event created successfully.", response.Message);
            Assert.NotNull(response.EventId);
            Assert.Equal(createdEvent.ObjectId, response.EventId);

            _mockEventRepository.Verify(repo => repo.CreateEventAsync(It.IsAny<Event>()), Times.Once());
        }

        [Fact]
        public async Task GetEventById_ExistingEvent_ReturnsOkResult()
        {
            var eventId = "testEventId";
            var testEvent = new Event
            {
                ObjectId = eventId,
                Name = "Test Event",
                Description = "Test Description",
                Date = DateTime.UtcNow,
                Location = "Test Location",
                Active = true,
                DJId = "testUserId",
                MusicConfig = new Event.MusicConfigClass
                {
                    EnableUserRecommendation = true,
                    MusicPlaylist = new List<MusicData>()
                }
            };

            _mockEventRepository.Setup(repo => repo.GetEventByIdAsync(eventId))
                .ReturnsAsync(testEvent);

            var result = await _controller.GetEventById(eventId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEvent = Assert.IsType<Event>(okResult.Value);

            Assert.Equal(testEvent.ObjectId, returnedEvent.ObjectId);
            Assert.Equal(testEvent.Name, returnedEvent.Name);
            Assert.Equal(testEvent.Description, returnedEvent.Description);
            Assert.Equal(testEvent.Location, returnedEvent.Location);
        }

        [Fact]
        public async Task GetEventById_NonExistingEvent_ReturnsNotFound()
        {
            var eventId = "nonExistingEventId";
            _mockEventRepository.Setup(repo => repo.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event)null);

            var result = await _controller.GetEventById(eventId);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}