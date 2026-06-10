using Bite.Services.Interface;
using System.Security.Cryptography;

namespace Bite.Services.Implementation;

public class OrderCodeService : IOrderCodeService
{
    // Confusion-free alphabet: excludes 0/O and 1/I/L
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 8;

    public string GenerateOrderCode()
        => RandomNumberGenerator.GetString(Alphabet, CodeLength);
}
