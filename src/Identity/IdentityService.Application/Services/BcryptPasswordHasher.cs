namespace IdentityService.Application.Services;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
    
    // BCrypt а не SHA256, у BCrypt adaptive hash, имеет work factor(cost), медленнее для brute-force защиты;
    // SHA256 - быстрый, для паролей не оч хороший вариант
}