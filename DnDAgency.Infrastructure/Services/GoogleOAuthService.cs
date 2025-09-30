using Google.Apis.Auth;
using DnDAgency.Infrastructure.Interfaces;
using Google.Apis.Auth.OAuth2;
using System.Text.Json;

namespace DnDAgency.Infrastructure.Services;

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    public GoogleOAuthService(string clientId, string clientSecret)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    public async Task<GoogleUserInfo> ValidateGoogleTokenAsync(string idToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _clientId }
        });

        return new GoogleUserInfo
        {
            Email = payload.Email,
            Name = payload.Name,
            GoogleId = payload.Subject,
            Picture = payload.Picture
        };
    }

    public async Task<GoogleUserInfo> ExchangeCodeForUserInfoAsync(string code, string redirectUri)
    {
        using var httpClient = new HttpClient();

        var tokenRequest = new Dictionary<string, string>
    {
            { "code", code },
        { "client_id", _clientId },
        { "client_secret", _clientSecret },
        { "redirect_uri", redirectUri },
        { "grant_type", "authorization_code" }
    };

        var tokenResponse = await httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(tokenRequest)
        );

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(tokenJson);

        var idToken = tokenData.GetProperty("id_token").GetString();

        return await ValidateGoogleTokenAsync(idToken!);
    }
}