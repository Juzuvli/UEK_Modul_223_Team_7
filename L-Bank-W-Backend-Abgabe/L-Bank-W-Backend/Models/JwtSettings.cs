namespace L_Bank_W_Backend.Models;

public class JwtSettings
{
    public string? IssuerSigninKey { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
}