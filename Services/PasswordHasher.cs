using System.Security.Cryptography;
using System.Text;

namespace ExamForge.Services;

public static class PasswordHasher
{
    public static string Crear(string texto)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
    }
}