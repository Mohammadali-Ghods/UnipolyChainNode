using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Nethereum.Hex.HexConvertors.Extensions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
class Program
{
    static void Main()
    {
        // === ورودی‌ها ===
        string eoaAddress = "0xf182fC897F4Dc167173dda594548C9439330fE55"; // آدرس کیف پول MetaMask برای برداشت
        ulong amountGwei = 7000000000000; // gwei
        string password = "Aa@43320098"; // رمز keystore
                                         // --- Public/Private Key BLS رندوم (دموی ساختار) ---
        byte[] privKey = RandomBytes(32);
        byte[] pubKey = RandomBytes(48);
        string pubkeyHex = "0x" + pubKey.ToHex(true);
        // ساخت withdrawal_credentials از EOA:
        byte[] wcBytes = BuildWithdrawalCredentials(eoaAddress);
        string wcHex = "0x" + wcBytes.ToHex(true);
        // Signature رندوم (دمو)
        byte[] sig = RandomBytes(96);
        string sigHex = "0x" + sig.ToHex(true);
        // محاسبه deposit_data_root
        byte[] rootComputed = ComputeDepositDataRoot(pubKey, wcBytes, amountGwei, sig);
        string rootHex = "0x" + rootComputed.ToHex(true);
        // --- ذخیره deposit_data.json ---
        var depositData = new
        {
            pubkey = pubkeyHex,
            withdrawal_credentials = wcHex,
            amount = amountGwei,
            signature = sigHex,
            deposit_data_root = rootHex
        };
        File.WriteAllText("validator/deposit_data.json",
        JsonSerializer.Serialize(depositData, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("deposit_data.json ساخته شد.");
        // --- ساخت keystore.json ---
        var keystore = CreateEip2335Keystore(privKey, password, pubKey);
        File.WriteAllText("validator/keystore.json",
        JsonSerializer.Serialize(keystore, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("keystore.json ساخته شد.");
        // --- ساخت فایل پسورد ---
        File.WriteAllText("validator/password.txt", password);
        Console.WriteLine("password.txt ساخته شد.");
    }
    static byte[] BuildWithdrawalCredentials(string eoa)
    {
        byte[] wc = new byte[32];
        wc[0] = 0x01; // prefix execution address
        byte[] addrBytes = eoa.HexToByteArray();
        Buffer.BlockCopy(addrBytes, 0, wc, 12, 20);
        return wc;
    }
    static object CreateEip2335Keystore(byte[] secretKey, string password, byte[] pubkey)
    {
        // ساخت salt و iv
        byte[] salt = RandomBytes(32);
        byte[] iv = RandomBytes(16);
        // پارامترهای scrypt
        int N = 262144, r = 8, p = 1, dklen = 32;
        byte[] derivedKey = Org.BouncyCastle.Crypto.Generators.SCrypt.Generate(
        Encoding.UTF8.GetBytes(password),
        salt, N, r, p, dklen
        );
        // رمزنگاری با AES-128-CTR
        byte[] encKey = new byte[16];
        Array.Copy(derivedKey, 0, encKey, 0, 16);
        byte[] cipherText = AesCtrEncrypt(secretKey, encKey, iv);
        // Checksum SHA-256
        byte[] checkHash;
        using (var sha256 = SHA256.Create())
        {
            byte[] dkRight = new byte[16];
            Array.Copy(derivedKey, 16, dkRight, 0, 16);
            byte[] concat = new byte[16 + cipherText.Length];
            Buffer.BlockCopy(dkRight, 0, concat, 0, 16);
            Buffer.BlockCopy(cipherText, 0, concat, 16, cipherText.Length);
            checkHash = sha256.ComputeHash(concat);
        }
        return new
        {
            crypto = new
            {
                kdf = new
                {
                    function = "scrypt",
                    @params = new { dklen, n = N, r, p, salt = salt.ToHex(true) },
                    message = ""
                },
                checksum = new
                {
                    function = "sha256",
                    @params = new { },
                    message = checkHash.ToHex(true)
                },
                cipher = new
                {
                    function = "aes-128-ctr",
                    @params = new { iv = iv.ToHex(true) },
                    message = cipherText.ToHex(true)
                }
            },
            description = "",
            pubkey = pubkey.ToHex(true),
            path = "m/12381/3600/0/0/0",
            uuid = Guid.NewGuid().ToString(),
            version = 4
        };
    }
    static byte[] AesCtrEncrypt(byte[] data, byte[] key, byte[] iv)
    {
        var cipher = new BufferedBlockCipher(new SicBlockCipher(new AesEngine()));
        cipher.Init(true, new ParametersWithIV(new KeyParameter(key), iv));
        return cipher.DoFinal(data);
    }
    static byte[] ComputeDepositDataRoot(byte[] pubkey, byte[] wc, ulong amountGwei, byte[] signature)
    {
        using var sha256 = SHA256.Create();
        byte[] zero16 = new byte[16];
        byte[] pubkeyRoot = sha256.ComputeHash(Concat(pubkey, zero16));
        byte[] sigPart1 = new byte[64];
        byte[] sigPart2 = new byte[32];
        Buffer.BlockCopy(signature, 0, sigPart1, 0, 64);
        Buffer.BlockCopy(signature, 64, sigPart2, 0, 32);
        byte[] zero32 = new byte[32];
        byte[] sigRoot = sha256.ComputeHash(Concat(
        sha256.ComputeHash(sigPart1),
        sha256.ComputeHash(Concat(sigPart2, zero32))
        ));
        byte[] amountLE = BitConverter.GetBytes(amountGwei);
        if (!BitConverter.IsLittleEndian) Array.Reverse(amountLE);
        byte[] amountPadded = Concat(amountLE, new byte[24]);
        byte[] left = sha256.ComputeHash(Concat(pubkeyRoot, wc));
        byte[] right = sha256.ComputeHash(Concat(amountPadded, sigRoot));
        return sha256.ComputeHash(Concat(left, right));
    }
    static byte[] Concat(byte[] a, byte[] b)
    {
        byte[] output = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, output, 0, a.Length);
        Buffer.BlockCopy(b, 0, output, a.Length, b.Length);
        return output;
    }
    static byte[] RandomBytes(int len)
    {
        byte[] buf = new byte[len];
        RandomNumberGenerator.Fill(buf);
        return buf;
    }
}
static class HexExtensions
{
    public static string ToHex(this byte[] bytes, bool lower = false)
    {
        var hex = BitConverter.ToString(bytes).Replace("-", "");
        return lower ? hex.ToLowerInvariant() : hex.ToUpperInvariant();
    }
}