using System.Net;
using System.Net.Http.Json;
using api.DTOs;
using api.DTOs.Users;
using api.Models;
using FluentAssertions;
using Api.IntegrationTests.Infrastructure;
using Api.IntegrationTests.Helpers;
using Xunit;
using Microsoft.AspNetCore.Http;
using Api.IntegrationTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using api.Interfaces;

namespace Api.IntegrationTests.Controllers
{
    public class UserControllerTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly MockFileStorageService _fileStorageMock;
        private readonly UserApi _userApi;

        public UserControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _userApi = new UserApi(_factory, _client);
            _fileStorageMock = factory.FileStorageMock;
        }

        public async Task InitializeAsync()
        {
            // Clean database before each test
            await DatabaseCleaner.CleanDatabase(_factory.Services);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
        #region Registration Tests

        [Fact]
        public async Task RegisterUser_WithAvatar_ReturnsCreatedUser()
        {
            // Act
            var user = await _userApi.Register(
                telegramId: 1010101010,
                displayName: "TestUser",
                age: 20);

            // Assert
            user.TelegramId.Should().Be(1010101010);
            user.DisplayName.Should().Be("TestUser");
            user.Age.Should().Be(20);
            user.Avatar.Should().NotBeNull();
        }

        [Fact]
        public async Task RegisterUser_WithFullProfile_ReturnsUserWithAllFields()
        {
            // Act
            var user = await _userApi.Register(
                telegramId: 1010101010,
                displayName: "FullProfileUser",
                age: 25,
                bio: "This is my bio",
                interests: new List<string> { "coding", "reading", "gaming" },
                languages: new List<string> { "English", "Spanish" },
                lookingFor: new List<string> { "friends", "networking" });

            // Assert
            user.DisplayName.Should().Be("FullProfileUser");
            user.Bio.Should().Be("This is my bio");
            user.Interests.Should().BeEquivalentTo(new[] { "coding", "reading", "gaming" });
            user.Languages.Should().BeEquivalentTo(new[] { "English", "Spanish" });
            user.LookingFor.Should().BeEquivalentTo(new[] { "friends", "networking" });
        }

        [Fact]
        public async Task RegisterUser_WithProfilePhotos_UploadsAllPhotos()
        {
            // Act
            var user = await _userApi.RegisterWithPhotos(
                telegramId: 1010101010,
                displayName: "FullProfileUser",
                age: 25,
                bio: "This is my bio",
                photoCount: 2,
                interests: new List<string> { "coding", "reading", "gaming" },
                languages: new List<string> { "English", "Spanish" },
                lookingFor: new List<string> { "friends", "networking" });

            // Assert
            user.ProfilePhotos.Should().HaveCount(2);
            user.ProfilePhotos.Should().Contain(p => p.SlotIndex == 0);
            user.ProfilePhotos.Should().Contain(p => p.SlotIndex == 1);
        }

        [Fact]
        public async Task RegisterUser_WhenDuplicateTelegramId_UpdatesExistingUser()
        {
            // Arrange: Register first user
            await _userApi.Register(
                telegramId: 1010101010,
                displayName: "OriginalName",
                age: 20,
                interests: new List<string> { "reading" });

            // Act: Register again with same TelegramId but different data
            var updatedUser = await _userApi.Register(
                telegramId: 1010101010,
                displayName: "UpdatedName",
                age: 25,
                bio: "New bio",
                interests: new List<string> { "reading", "coding" },
                languages: new List<string> { "English" },
                lookingFor: new List<string> { "friends" });

            // Assert
            updatedUser.DisplayName.Should().Be("UpdatedName");
            updatedUser.Age.Should().Be(25);
            updatedUser.Bio.Should().Be("New bio");
            updatedUser.Interests.Should().BeEquivalentTo(new[] { "reading", "coding" });
        }

        [Fact]
        public async Task RegisterUser_WithoutAvatar_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RegisterUserDto
            {
                TelegramId = 1010101010,
                DisplayName = "NoAvatarUser",
                Age = 20,
                Avatar = null!,
                Interests = new(),
                Languages = new(),
                LookingFor = new(),
                ProfilePhotoSlotIndices = new()
            };

            // Act & Assert
            var act = async () => await _client.PostAsync("/user/register", dto.ToMultipart());
            var response = await act();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RegisterUser_InvalidAge_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RegisterUserDto
            {
                TelegramId = 1010101010,
                DisplayName = "InvalidAgeUser",
                Age = 12,
                Avatar = TestDataBuilder.CreateDummyAvatar(),
                Interests = new(),
                Languages = new(),
                LookingFor = new(),
                ProfilePhotoSlotIndices = new()
            };

            // Act & Assert
            var response = await _client.PostAsync("/user/register", dto.ToMultipart());
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RegisterUser_EmptyDisplayName_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RegisterUserDto
            {
                TelegramId = 1010101010,
                DisplayName = "",
                Age = 20,
                Avatar = TestDataBuilder.CreateDummyAvatar(),
                Interests = new(),
                Languages = new(),
                LookingFor = new(),
                ProfilePhotoSlotIndices = new()
            };

            // Act & Assert
            var response = await _client.PostAsync("/user/register", dto.ToMultipart());
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Update User Tests

        [Fact]
        public async Task UpdateUser_UpdatesBasicFields_ReturnsUpdatedUser()
        {
            // Arrange
            await _userApi.Register(
                telegramId: 1010101010,
                displayName: "OriginalName",
                age: 20,
                bio: "Original bio",
                interests: new List<string> { "reading" },
                languages: new List<string> { "English" },
                lookingFor: new List<string> { "friends" });

            var updateDto = new UpdateUserDto
            {
                DisplayName = "UpdatedName",
                Age = 25,
                Bio = "Updated bio",
                Interests = new List<string> { "reading", "coding" },
                Languages = new List<string> { "English", "French" },
                LookingFor = new List<string> { "friends", "dating" }
            };

            // Act
            var user = await _userApi.UpdateUser(1010101010, updateDto);

            // Assert
            user.DisplayName.Should().Be("UpdatedName");
            user.Age.Should().Be(25);
            user.Bio.Should().Be("Updated bio");
            user.Interests.Should().BeEquivalentTo(new[] { "reading", "coding" });
            user.Languages.Should().BeEquivalentTo(new[] { "English", "French" });
            user.LookingFor.Should().BeEquivalentTo(new[] { "friends", "dating" });
        }

        [Fact]
        public async Task UpdateUser_UpdatesAvatar_ReplacesOldAvatar()
        {
            // Arrange
            var originalUser = await _userApi.Register(
                telegramId: 1010101010,
                displayName: "AvatarUser",
                age: 20);

            var updateDto = new UpdateUserDto
            {
                Avatar = TestDataBuilder.CreateDummyAvatar("avatar2.png") // Different filename
            };

            // Act
            var updatedUser = await _userApi.UpdateUser(1010101010, updateDto);

            // Assert
            updatedUser.Avatar.Should().NotBeNull();
            updatedUser.Avatar.Url.Should().NotBe(originalUser.Avatar.Url);
            updatedUser.Avatar.Url.Should().Contain("avatar2.png");

        }

        [Fact]
        public async Task UpdateUser_AddsNewProfilePhotos_KeepsExistingOnes()
        {
            // Arrange: Create user with one profile photo
            var registeredUser = await _userApi.RegisterWithPhotos(
                telegramId: 1010101010,
                displayName: "PhotoUpdateUser",
                age: 20,
                photoCount: 1);

            var updateDto = new UpdateUserDto
            {
                ProfilePhotos = new[]
                {
            TestDataBuilder.CreateDummyPhoto("photo2.jpg")
        },
                ProfilePhotoSlotIndices = new List<int> { 1 },
                ExistingPhotoMessageIds = new List<string> { registeredUser.ProfilePhotos.First().MessageId }
            };

            // Act
            var updatedUser = await _userApi.UpdateUser(1010101010, updateDto);

            // Assert
            updatedUser.ProfilePhotos.Should().HaveCount(2);
        }

        [Fact]
        public async Task UpdateUser_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                DisplayName = "UpdatedName"
            };

            // Act & Assert
            var act = async () => await _userApi.UpdateUser(9999999999999, updateDto);
            await act.Should().ThrowAsync<Exception>()
                .Where(e => e.Message.Contains("403") || e.Message.Contains("Forbidden"));
        }

        #endregion

        #region Get User Tests

        [Fact]
        public async Task GetUserByTelegramId_ExistingUser_ReturnsUserDto()
        {
            // Arrange
            await _userApi.Register(
                telegramId: 1010101010,
                displayName: "GetUser",
                age: 20,
                bio: "Test bio",
                interests: new List<string> { "coding" },
                languages: new List<string> { "English" },
                lookingFor: new List<string> { "friends" });

            // Act
            var userDto = await _userApi.GetUser(1010101010);

            // Assert
            userDto.TelegramId.Should().Be(1010101010);
            userDto.DisplayName.Should().Be("GetUser");
            userDto.Age.Should().Be(20);
            userDto.Bio.Should().Be("Test bio");
            userDto.Interests.Should().BeEquivalentTo(new[] { "coding" });
            userDto.Languages.Should().BeEquivalentTo(new[] { "English" });
            userDto.LookingFor.Should().BeEquivalentTo(new[] { "friends" });
            userDto.Avatar.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUserByTelegramId_NonExistentUser_ReturnsNotFound()
        {
            // Act
            var userDto = await _userApi.TryGetUser(9876543210999);

            // Assert
            userDto.Should().BeNull();
        }

        #endregion

        #region Location Tests

        [Fact]
        public async Task UpdateUserLocation_UpdatesLocation_ReturnsUpdatedUserDto()
        {
            // Arrange
            await _userApi.Register(
                telegramId: 1010101010,
                displayName: "LocationTestUser",
                age: 20);

            // Act
            await _userApi.UpdateLocation(
                telegramId: 1010101010,
                lat: 51.5074,
                lng: -0.1278);

            var userDto = await _userApi.GetUser(1010101010);

            // Assert
            userDto.Location.Should().NotBeNull();
            userDto.Location!.Latitude.Should().BeApproximately(51.5074, 0.0001);
            userDto.Location.Longitude.Should().BeApproximately(-0.1278, 0.0001);
        }

        [Fact]
        public async Task UpdateUserLocation_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var locationDto = new UpdateUserLocationDto
            {
                Latitude = 51.5074,
                Longitude = -0.1278
            };

            // Act & Assert
            var response = await _client.PutAsync("/user/1010101010/location",
                JsonContent.Create(locationDto));

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Nearby Users Tests

        [Fact]
        public async Task FindNearbyUsers_ReturnsUsersWithinRadius()
        {
            // Arrange
            var currentUser = await _userApi.Register(
                telegramId: 1010101010,
                displayName: "CurrentUser");

            await _userApi.UpdateLocation(
                currentUser.TelegramId,
                lat: 51.5074,
                lng: -0.1278);

            await _userApi.CreateUserDirectly(
                telegramId: 6060606060,
                displayName: "NearbyUser",
                lat: 51.5074,
                lng: -0.1278);

            // Act
            var nearbyUsers = await _userApi.FindNearby(
                lat: 51.5074,
                lng: -0.1278,
                radiusMeters: 10_000);

            // Assert
            var nearbyUser = nearbyUsers.Should().ContainSingle(u => u.TelegramId == 6060606060).Subject;
            nearbyUser.DisplayName.Should().Be("NearbyUser");
            nearbyUser.AvatarUrl.Should().NotBeNullOrEmpty();
            nearbyUser.Location.Should().NotBeNull();
            nearbyUser.Location.Latitude.Should().BeApproximately(51.5074, 0.0001);
            nearbyUser.Location.Longitude.Should().BeApproximately(-0.1278, 0.0001);
        }

        [Fact]
        public async Task FindNearbyUsers_ExcludesCurrentUser()
        {
            // Arrange
            var currentUser = await _userApi.Register(
                telegramId: 1010101010,
                displayName: "CurrentUser");

            await _userApi.UpdateLocation(
                currentUser.TelegramId,
                lat: 40.712,
                lng: -74.0060);

            // Act
            var nearbyUsers = await _userApi.FindNearby(
                lat: 40.7128,
                lng: -74.0060,
                radiusMeters: 10_000);

            // Assert
            nearbyUsers.Should().NotContain(u => u.TelegramId == 1010101010);
        }

        [Fact]
        public async Task FindNearbyUsers_ExcludesUsersOutsideRadius()
        {
            // Arrange
            await _userApi.CreateUserDirectly(
                telegramId: 7171717171,
                displayName: "NYUser",
                lat: 40.7128,
                lng: -74.0060);

            await _userApi.CreateUserDirectly(
                telegramId: 7272727272,
                displayName: "LondonUser",
                lat: 51.5074,
                lng: -0.1278);

            // Act
            var nearbyUsers = await _userApi.FindNearby(
                lat: 40.7128,
                lng: -74.0060,
                radiusMeters: 10_000);

            // Assert
            nearbyUsers.Should().NotContain(u => u.TelegramId == 7272727272,
                "London user should be excluded (>5000km away from New York)");
        }
        #endregion
    }
}
