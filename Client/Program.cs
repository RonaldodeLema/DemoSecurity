using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Infrastructure;
using Newtonsoft.Json;

namespace Client;

internal static class Client
{
    private static void Main()
    {
        Console.WriteLine("---------------------Ctrl + C to out of programing---------------------");
        while (true)
        {
            DemoCipher();
            Console.WriteLine("---------------------Ctrl + C to out of programing---------------------");
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static void DemoCipher()
    {
        Console.WriteLine("-------------Demo cipher with context login---------------");

        var client = new TcpClient("localhost", 1234);
        var ns = client.GetStream();
        Console.WriteLine("Enter username:");
        var username = Console.ReadLine();
        Console.WriteLine("Enter password:");
        var password = Console.ReadLine();
        if (username == null || password == null) return;
        var userLogin = new UserDto(username, password);

        // Encrypt password with sha256
        userLogin.HashPasswordSha256();


        var data = JsonConvert.SerializeObject(userLogin);
        var publicKey =
            "-----BEGIN PUBLIC KEY----- MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAhiurQ0+cnsZ+3ihRGzTk uA/gX112Eo6VJkzpBP74XOps1i14qqax2dBvhK/p+RX69FJ5Jb6lACTPTwAW3Di8 8YITnUNfX2YzS9OvweG/hL3k6A3siT5AGKlpn6pgUVCzxXSRW70ws1DTVyrt0cro KZIPISkc8bMdBRxHoVy79IvNZ0ygVHulm0UuZzbHDoad9Bicf/DRXM93xAaar7IB aa3Jf1PeZU2Mfem2/2soWLDO0dx+wonErHNl/tQdOx2Xso7FGHB2BjS6vCZNr77i nA1spyJH1O/X40gJQZY5ZAYt/UETGnlqKdtAi5RdQPO0VME0qHxysDW6RhA6OI2d +uCwqniNYCmtRyCNGLzc210dCD5gDzC8nERAhpv2kTn/zYHdeytsEF/eVB5cfgee yFWVuhQKAIxjz8mJs2py4/A6WJxiv49+5cFVh+GYcMkRif5YyUzbTqirF8+kaL31 VGm6jDzEmSE+SmN9KCb5wDVUtLFxXqm8l3UMyCTxIcNhAgMEAP8= -----END PUBLIC KEY-----";

        // Import the RSA key information. This only needs to include the public key information.
        var rsa = new RSACryptoServiceProvider();
        rsa.ImportFromPem(publicKey.ToCharArray());
        var dataToSend = Encoding.ASCII.GetBytes(data);

        // Encrypt Data with RSA
        var encryptedData = RsaEncrypt(dataToSend, rsa.ExportParameters(false), false);
        var base64EncryptedData = Convert.ToBase64String(encryptedData);
        var base64Bytes = Encoding.ASCII.GetBytes(base64EncryptedData);
        // Send
        ns.Write(base64Bytes, 0, base64Bytes.Length);

        // Response
        var buffer = new byte[client.ReceiveBufferSize];
        var bytesRead = ns.Read(buffer, 0, client.ReceiveBufferSize);
        var responseReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        Console.WriteLine(responseReceived);
        DataResponse? dataResponse = new DataResponse();
        using (var sr = new StringReader(responseReceived))
        using (JsonReader reader = new JsonTextReader(sr))
        {
            var serializer = new JsonSerializer();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    dataResponse = serializer.Deserialize<DataResponse>(reader);
                }
            }
        }

        if (dataResponse!.Data == null)
        {
            return;
        }

        var listCiphers = dataResponse.Data;
        if (listCiphers == null) return;
        // decrypt
        listCiphers = listCiphers.Select(CipherDecrypt).ToList();
        var encryptPath = "/home/vanthao/RiderProjects/DemoSecurity/Client/FileEncrypt";
        var decryptPath = "/home/vanthao/RiderProjects/DemoSecurity/Client/FileDecrypt";

        foreach (var cipher in listCiphers)
        {
            Console.WriteLine(cipher.ToString());
            File.WriteAllText(encryptPath + "/" + cipher.Name + "_encrypt.txt",
                Encoding.ASCII.GetString(cipher.Encrypt));
            File.WriteAllText(decryptPath + "/" + cipher.Name + "_decrypt.txt", cipher.Decrypt);
        }
    }

    private static byte[] RsaEncrypt(byte[] dataToEncrypt, RSAParameters rsaKeyInfo, bool doOaepPadding)
    {
        try
        {
            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaKeyInfo);
            var encryptedData = rsa.Encrypt(dataToEncrypt, doOaepPadding);
            return encryptedData;
        }
        catch (CryptographicException e)
        {
            Console.WriteLine(e.ToString());
            return null!;
        }
    }

    private static Cipher CipherDecrypt(Cipher cipher)
    {
        switch (cipher.Name)
        {
            case "AES":
            {
                using Aes aesAlg = Aes.Create();
                aesAlg.Key = cipher.Key;
                aesAlg.IV = cipher.IV;
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                var stopwatch = Stopwatch.StartNew();
                using var msDecrypt = new MemoryStream(cipher.Encrypt);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                cipher.Decrypt = srDecrypt.ReadToEnd();
                stopwatch.Stop();
                cipher.TimeDecrypt = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);
                break;
            }
            case "DES":
            {
                using DES desAlg = DES.Create();
                desAlg.Key = cipher.Key;
                desAlg.IV = cipher.IV;
                var decryptor = desAlg.CreateDecryptor(desAlg.Key, desAlg.IV);
                var stopwatch = Stopwatch.StartNew();
                using var msDecrypt = new MemoryStream(cipher.Encrypt);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                cipher.Decrypt = srDecrypt.ReadToEnd();
                stopwatch.Stop();
                cipher.TimeDecrypt = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);
                break;
            }
            case "3DES":
            {
                using var tripleDes = TripleDES.Create();
                tripleDes.Key = cipher.Key;
                tripleDes.IV = cipher.IV;
                var decryptor = tripleDes.CreateDecryptor(tripleDes.Key, tripleDes.IV);
                var stopwatch = Stopwatch.StartNew();
                using var msDecrypt = new MemoryStream(cipher.Encrypt);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                cipher.Decrypt = srDecrypt.ReadToEnd();
                stopwatch.Stop();
                cipher.TimeDecrypt = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);
                break;
            }
        }

        return cipher;
    }
}