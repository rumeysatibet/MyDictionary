using System.Text.Json;
using Microsoft.JSInterop;

namespace MyDictionary.Web.Services;

public class AuthenticationStateService
{
    private UserInfo? _currentUser;
    private readonly ILogger<AuthenticationStateService> _logger;
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private bool _isInitialized = false;

    public event Action? OnAuthenticationChanged;

    public AuthenticationStateService(ILogger<AuthenticationStateService> logger, IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _logger = logger;
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public UserInfo? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        
        _logger.LogInformation("🔄 AuthenticationStateService initialize ediliyor...");
        _isInitialized = true;
        
        // localStorage okuma işlemi component'ların OnAfterRenderAsync'inde yapılacak
        // Burada sadece initialize flag'ini set ediyoruz
    }

    public async Task LoadFromStorageAsync()
    {
        try
        {
            _logger.LogInformation("💾 localStorage'dan kullanıcı bilgileri okunuyor...");
            
            // JSInterop hazır olana kadar bekle
            await Task.Delay(100);
            
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "token");
            
            if (!string.IsNullOrEmpty(token))
            {
                // Token'ı sunucuda validate et
                var isValid = await ValidateTokenWithServerAsync(token);
                if (isValid)
                {
                    var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "currentUser");
                    if (!string.IsNullOrEmpty(userJson))
                    {
                        _currentUser = JsonSerializer.Deserialize<UserInfo>(userJson);
                        _logger.LogInformation($"🔐 Token geçerli, kullanıcı yüklendi: {_currentUser?.Username}");
                        OnAuthenticationChanged?.Invoke();
                    }
                }
                else
                {
                    _logger.LogInformation("❌ Token geçersiz, localStorage temizleniyor");
                    await ClearStorageAsync();
                }
            }
            else
            {
                _logger.LogInformation("🔍 localStorage'da token bulunamadı");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            _logger.LogWarning($"⚠️ localStorage okuma ertelendi (prerendering): {ex.Message}");
            // Prerendering sırasında localStorage'dan okuyamayız
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ localStorage okuma hatası: {ex.Message}");
        }
    }

    public async Task SetUserAsync(UserInfo user, string token)
    {
        _currentUser = user;
        _logger.LogInformation($"🔐 Kullanıcı giriş yaptı: {user.Username}");
        _logger.LogInformation($"🔐 IsAuthenticated: {IsAuthenticated}");
        _logger.LogInformation($"🔐 CurrentUser: {CurrentUser?.Username ?? "null"}");
        
        // localStorage'a kaydet - sadece interaktif rendering sırasında
        await SaveToStorageAsync(user, token);
        
        _logger.LogInformation("🔔 OnAuthenticationChanged event tetikleniyor");
        OnAuthenticationChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
        var username = _currentUser?.Username ?? "Unknown";
        _currentUser = null;
        _logger.LogInformation($"🚪 Kullanıcı çıkış yaptı: {username}");
        
        // localStorage'dan temizle
        await ClearStorageAsync();
        
        OnAuthenticationChanged?.Invoke();
    }

    private async Task<bool> ValidateTokenWithServerAsync(string token)
    {
        try
        {
            var request = new { Token = token };
            var response = await _httpClient.PostAsJsonAsync("api/auth/validate-token", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Token validation hatası: {ex.Message}");
            return false;
        }
    }

    private async Task SaveToStorageAsync(UserInfo user, string token)
    {
        try
        {
            // JSInterop hazır olana kadar bekle
            await Task.Delay(100);
            
            var userJson = JsonSerializer.Serialize(user);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "currentUser", userJson);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "token", token);
            _logger.LogInformation("💾 Kullanıcı bilgileri localStorage'a kaydedildi");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            _logger.LogWarning($"⚠️ localStorage yazma ertelendi (prerendering): {ex.Message}");
            // Prerendering sırasında localStorage'a yazamayız, sadece memory'de tut
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ localStorage yazma hatası: {ex.Message}");
        }
    }

    private async Task ClearStorageAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "currentUser");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "token");
            _logger.LogInformation("🗑️ localStorage temizlendi");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            _logger.LogWarning($"⚠️ localStorage temizleme ertelendi (prerendering): {ex.Message}");
            // Prerendering sırasında localStorage'ı temizleyemeyiz
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ localStorage temizleme hatası: {ex.Message}");
        }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public int Gender { get; set; }
        public string Role { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}