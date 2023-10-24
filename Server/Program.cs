using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Infrastructure;
using Newtonsoft.Json;

namespace Server;

internal static class Server
{
    private static void Main()
    {
        var users = new List<UserDto>()
        {
            new("vanthao", "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92"),
            new("userlord", "8bb0cf6eb9b17d0f7d22b456f121257dc1254e1f01665370476383ea776df414")

        };
        var server = new TcpListener(IPAddress.Any, 1234);
        server.Start();
        Console.WriteLine("Server started...");
        while (true)
        {
            var client = server.AcceptTcpClient();
            var ns = client.GetStream();
            var buffer = new byte[client.ReceiveBufferSize];
            var bytesRead = ns.Read(buffer, 0, client.ReceiveBufferSize);
            
            var dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            var privateKey =
            "-----BEGIN PRIVATE KEY----- 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 6reLjqsNUEOKRqvNq94Tntg= -----END PRIVATE KEY-----";
            // Import the RSA key information. This only needs to include the private key information.
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportFromPem(privateKey.ToCharArray());
            var encryptedData = Convert.FromBase64String(dataReceived);
            var decryptedData = RsaDecrypt(encryptedData, rsa.ExportParameters(true), false);
            Console.WriteLine("Encrypted plaintext from client: " + dataReceived);
            Console.WriteLine("Decrypted plaintext: " + Encoding.UTF8.GetString(decryptedData));
            Console.WriteLine();
            
            //convert data
            var userLogin = JsonConvert.DeserializeObject<UserDto>(Encoding.UTF8.GetString(decryptedData));
            // Check login valid
            var check = users.FirstOrDefault(u => u.Username == userLogin!.Username && u.Password==userLogin.Password);
            // response
            string response;
            if (check==null)
            {
                var dataResponse = new DataResponse()
                {
                    Code = 401,
                    Message = "Server Response: User login invalid",
                    Data = null
                };
                response =JsonConvert.SerializeObject(dataResponse);
            }
            else
            {
                var dataResponse = new DataResponse()
                {
                    Code = 200,
                    Message = "Server Response: User login valid",
                    Data = new List<Cipher>()
                    {
                        CipherEncrypt("AES","/home/vanthao/RiderProjects/DemoSecurity/Server/SampleEEG.txt"),
                        CipherEncrypt("DES","/home/vanthao/RiderProjects/DemoSecurity/Server/SampleEEG.txt"),
                        CipherEncrypt("3DES","/home/vanthao/RiderProjects/DemoSecurity/Server/SampleEEG.txt")
                    }
                };
                response =JsonConvert.SerializeObject(dataResponse);
            }
            
            
            // Convert the response to a byte array
            var responseBytes = Encoding.ASCII.GetBytes(response);

            // Send the response back to the client
            // Console.WriteLine(response);
            ns.Write(responseBytes, 0, responseBytes.Length);
       
        }
        // ReSharper disable once FunctionNeverReturns
    }
    
    private static byte[] RsaDecrypt(byte[] dataToDecrypt, RSAParameters rsaKeyInfo, bool doOaepPadding)
    {
        try
        {
            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaKeyInfo);
            var decryptedData = rsa.Decrypt(dataToDecrypt, doOaepPadding);
            return decryptedData;
        }
        catch (CryptographicException e)
        {
            Console.WriteLine(e.ToString());
            return null!;
        }
    }

    private static Cipher CipherEncrypt(string nameCipher,string dataPath)
    {
        var filePath = dataPath;
        var text = File.ReadAllText(filePath);
        switch (nameCipher)
        {
            case "AES":
            {
                var aes = Aes.Create();
                var encryptor = aes.CreateEncryptor();
                var stopwatch = Stopwatch.StartNew();
                var encryptedText = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(text), 0, text.Length);
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency));
                var cipher = new Cipher()
                {
                    Name = nameCipher,
                    Key = aes.Key,
                    IV =aes.IV,
                    Encrypt = encryptedText,
                    SpaceEncrypt = encryptedText.Length,
                    TimeEncrypt = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency)
                };
                return cipher;
            }
            case "DES":
            {
                var des = DES.Create();
                // Encrypt the text
                var encryptor = des.CreateEncryptor();
                var stopwatch = Stopwatch.StartNew();
                var encryptedText = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(text), 0, text.Length);
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency));
                var cipher = new Cipher()
                {
                    Name = nameCipher,
                    Key = des.Key,
                    Encrypt = encryptedText,                   
                    IV =des.IV,                    
                    SpaceEncrypt = encryptedText.Length,
                    TimeEncrypt = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency)
                };
                return cipher;
            }
            case "3DES":
            {
                var tripledes = TripleDES.Create();
                // Encrypt the text
                var encryptor = tripledes.CreateEncryptor();
                var stopwatch = Stopwatch.StartNew();
                var encryptedText = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(text), 0, text.Length);
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency));
                var cipher = new Cipher()
                {
                    Name = nameCipher,
                    Key = tripledes.Key,
                    Encrypt = encryptedText,
                    IV =tripledes.IV,
                    SpaceEncrypt = encryptedText.Length,
                    TimeEncrypt = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency)
                };
                return cipher;
            }
            default:
                return new Cipher();
        }
    }
}