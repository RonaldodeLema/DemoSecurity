using System.Security.Cryptography;
using System.Text;

namespace Infrastructure;

public class UserDto
{
    public string Username;
    public string Password;
    public DateTime DateLogin;

    public UserDto(string username, string password)
    {
        Username = username;
        Password = password;
        DateLogin = DateTime.Now;
    }
    public void HashPasswordSha256()
    {
        using var hasher = SHA256.Create();
        byte[] hashedBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(Password));
        Password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }
}