using System.Text.Json;
using Microsoft.JSInterop;

namespace MyDictionary.Web.Services;

public class AuthenticationStateService
{
    private UserInfo? _currentUser;
    private readonly ILogger<AuthenticationStateService> _logger;
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized = false;

    public event Action? OnAuthenticationChanged;

    public AuthenticationStateService(ILogger<AuthenticationStateService> logger, IJSRuntime jsRuntime)
    {
        _logger = logger;
        _jsRuntime = jsRuntime;
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
            var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "currentUser");
            
            if (!string.IsNullOrEmpty(userJson))
            {
                _currentUser = JsonSerializer.Deserialize<UserInfo>(userJson);
                _logger.LogInformation($"🔐 localStorage'dan kullanıcı yüklendi: {_currentUser?.Username}");
                OnAuthenticationChanged?.Invoke();
            }
            else
            {
                _logger.LogInformation("🔍 localStorage'da kullanıcı bilgisi bulunamadı");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ localStorage okuma hatası: {ex.Message}");
        }
    }

    public async void SetUser(UserInfo user, string token)
    {
        _currentUser = user;
        _logger.LogInformation($"🔐 Kullanıcı giriş yaptı: {user.Username}");
        _logger.LogInformation($"🔐 IsAuthenticated: {IsAuthenticated}");
        _logger.LogInformation($"🔐 CurrentUser: {CurrentUser?.Username ?? "null"}");
        
        // localStorage'a kaydet
        try
        {
            var userJson = JsonSerializer.Serialize(user);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "currentUser", userJson);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "token", token);
            _logger.LogInformation("💾 Kullanıcı bilgileri localStorage'a kaydedildi");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ localStorage yazma hatası: {ex.Message}");
        }
        
        _logger.LogInformation("🔔 OnAuthenticationChanged event tetikleniyor");
        OnAuthenticationChanged?.Invoke();
    }

    public async void Logout()
    {
        var username = _currentUser?.Username ?? "Unknown";
        _currentUser = null;
        _logger.LogInformation($"🚪 Kullanıcı çıkış yaptı: {username}");
        
        // localStorage'dan temizle
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "currentUser");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "token");
            _logger.LogInformation("🗑️ localStorage temizlendi");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ localStorage temizleme hatası: {ex.Message}");
        }
        
        OnAuthenticationChanged?.Invoke();
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