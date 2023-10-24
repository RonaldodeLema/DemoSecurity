using System.Text;

namespace Infrastructure;

public class Cipher
{
    public string? Name { get; set; }
    public byte[]? Encrypt { get; set; }
    public string? Decrypt { get; set; }
    public byte[] Key { get; set; }
    public byte[] IV { get; set; }
    public long TimeEncrypt { get; set; }
    public int SpaceEncrypt { get; set; }
    public long TimeDecrypt { get; set; }

    
    public override string ToString()
    {
        return "{Name: " + Name + ", Key: " + Encoding.UTF8.GetString(Key) + ", Time Encrypt(ns): " + TimeEncrypt +
               ", Space Encrypt(char): " + SpaceEncrypt + ", Time Decrypt(ns): " + TimeDecrypt+"}";
    }
}