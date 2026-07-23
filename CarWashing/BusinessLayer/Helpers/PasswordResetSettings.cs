namespace BusinessLayer.Helpers;
public sealed class PasswordResetSettings { public string FrontendBaseUrl { get; set; } = "http://localhost:5173"; public int TokenLifetimeMinutes { get; set; } = 15; }
