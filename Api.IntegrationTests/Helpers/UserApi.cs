using System.Net.Http.Json;
using api.Data;
using api.DTOs;
using api.DTOs.Users;
using api.Models;
using Api.IntegrationTests.Helpers;
using Api.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;

public class UserApi
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UserApi(CustomWebApplicationFactory factory, HttpClient client)
    {
        _factory = factory;
        _client = client;
    }

    public async Task<User> Register(
        long telegramId,
        string displayName = "TestUser",
        int age = 20,
        string? bio = null,
        List<string>? interests = null,
        List<string>? languages = null,
        List<string>? lookingFor = null)
    {
        var dto = new RegisterUserDto
        {
            TelegramId = telegramId,
            DisplayName = displayName,
            Age = age,
            Bio = bio,
            Avatar = TestDataBuilder.CreateDummyAvatar(),
            Interests = interests ?? new(),
            Languages = languages ?? new(),
            LookingFor = lookingFor ?? new(),
            ProfilePhotoSlotIndices = new()
        };

        var response = await _client.PostAsync("/user/register", dto.ToMultipart());
        return await EnsureSuccess<User>(response, nameof(Register));
    }

    public async Task<UserDto> GetUser(long telegramId)
    {
        var response = await _client.GetAsync($"/user/{telegramId}");
        return await EnsureSuccess<UserDto>(response, nameof(GetUser));
    }

    public async Task<UserDto?> TryGetUser(long telegramId)
    {
        var response = await _client.GetAsync($"/user/{telegramId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        return await EnsureSuccess<UserDto>(response, nameof(TryGetUser));
    }

    public async Task<User> UpdateUser(long telegramId, UpdateUserDto dto)
    {
        var response = await _client.PutAsync($"/user/{telegramId}", dto.ToMultipart());
        return await EnsureSuccess<User>(response, nameof(UpdateUser));
    }

    public async Task UpdateLocation(long telegramId, double lat, double lng)
    {
        var dto = new UpdateUserLocationDto { Latitude = lat, Longitude = lng };
        var response = await _client.PutAsync($"/user/{telegramId}/location", JsonContent.Create(dto));
        await EnsureSuccess(response, nameof(UpdateLocation));
    }

    public async Task<List<NearbyUserDto>> FindNearby(double lat, double lng, int radiusMeters)
    {
        var response = await _client.GetAsync($"/user/nearby?lat={lat}&lng={lng}&radiusMeters={radiusMeters}");
        return await EnsureSuccess<List<NearbyUserDto>>(response, nameof(FindNearby));
    }

    public async Task<User> CreateUserDirectly(long telegramId, string displayName, double lat, double lng)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            TelegramId = telegramId,
            DisplayName = displayName,
            Age = 20,
            Avatar = new Avatar
            {
                MessageId = $"test-avatar-{telegramId}",
                Url = "https://example.com/default-avatar.png"
            },
            Location = new Point(lng, lat) { SRID = 4326 },
            Interests = new(),
            Languages = new(),
            LookingFor = new(),
            ProfilePhotos = new(),
            CreatedAt = DateTime.UtcNow,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
    // Add to UserApi.cs
    public async Task<User> RegisterWithPhotos(
        long telegramId,
        string displayName = "TestUser",
        int age = 20,
        string? bio = null,
        int photoCount = 1,
        List<string>? interests = null,
        List<string>? languages = null,
        List<string>? lookingFor = null)
    {
        var photos = Enumerable.Range(0, photoCount)
            .Select(i => TestDataBuilder.CreateDummyPhoto($"photo{i}.jpg"))
            .ToArray();

        var dto = new RegisterUserDto
        {
            TelegramId = telegramId,
            DisplayName = displayName,
            Age = age,
            Bio = bio,
            Avatar = TestDataBuilder.CreateDummyAvatar(),
            ProfilePhotos = photos,
            ProfilePhotoSlotIndices = Enumerable.Range(0, photoCount).ToList(),
            Interests = interests ?? new(),
            Languages = languages ?? new(),
            LookingFor = lookingFor ?? new()
        };

        var response = await _client.PostAsync("/user/register", dto.ToMultipart());
        return await EnsureSuccess<User>(response, nameof(RegisterWithPhotos));
    }
    private static async Task<T> EnsureSuccess<T>(HttpResponseMessage response, string operation)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"{operation} failed ({response.StatusCode}): {error}");
        }
        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"{operation} returned null");
    }

    private static async Task EnsureSuccess(HttpResponseMessage response, string operation)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"{operation} failed ({response.StatusCode}): {error}");
        }
    }
}