using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using Nethereum.Hex.HexConvertors.Extensions;
class Program
{
    static void Main()
    {
        // === ورودی‌های نمونه ===
        string pubkeyHex = "0x" + RandomHex(48);
        string wcHex = "0x01" + new string('0', 22) + RandomHex(20); // نمونه ساخت WC
        ulong amountGwei = 7000000000000; // gwei
        string sigHex = "0x" + RandomHex(96);
        byte[] pubkey = pubkeyHex.HexToByteArray();
        byte[] wc = wcHex.HexToByteArray();
        byte[] sig = sigHex.HexToByteArray();
        // محاسبه deposit_data_root
        byte[] rootComputed = ComputeDepositDataRoot(pubkey, wc, amountGwei, sig);
        string rootHex = "0x" + rootComputed.ToHex();
        // ساخت JSON خروجی
        var depositData = new
        {
            pubkey = pubkeyHex,
            withdrawal_credentials = wcHex,
            amount = amountGwei,
            signature = sigHex,
            deposit_data_root = rootHex
        };
        File.WriteAllText("deposit_data.json",
        JsonSerializer.Serialize(depositData, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("deposit_data.json ساخته شد.");
        Console.WriteLine($"deposit_data_root = {rootHex}");
        // تست و تایید
        bool match = VerifyDepositDataRoot(pubkey, wc, amountGwei, sig, rootComputed);
        Console.WriteLine($"آیا deposit_data_root صحیح است؟ {match}");
    }
    /// تابع محاسبه root مطابق قرارداد Solidity
    static byte[] ComputeDepositDataRoot(byte[] pubkey, byte[] wc, ulong amountGwei, byte[] signature)
    {
        using var sha256 = SHA256.Create();
        // مرحله 1: pubkey_root = sha256(pubkey || zero16)
        byte[] zero16 = new byte[16];
        byte[] pubkeyConcat = Concat(pubkey, zero16);
        byte[] pubkeyRoot = sha256.ComputeHash(pubkeyConcat);
        // مرحله 2: signature_root
        byte[] sigPart1 = new byte[64];
        byte[] sigPart2 = new byte[32];
        Buffer.BlockCopy(signature, 0, sigPart1, 0, 64);
        Buffer.BlockCopy(signature, 64, sigPart2, 0, 32);
        byte[] zero32 = new byte[32];
        byte[] sigPart2Padded = Concat(sigPart2, zero32);
        byte[] sigHash1 = sha256.ComputeHash(sigPart1);
        byte[] sigHash2 = sha256.ComputeHash(sigPart2Padded);
        byte[] signatureRoot = sha256.ComputeHash(Concat(sigHash1, sigHash2));
        // مرحله 3: Amount Little Endian + zero24
        byte[] amountLE = BitConverter.GetBytes(amountGwei);
        if (!BitConverter.IsLittleEndian) Array.Reverse(amountLE); // اطمینان از LE بودن
        byte[] zero24 = new byte[24];
        byte[] amountPadded = Concat(amountLE, zero24);
        // مرحله 4: left = sha256(pubkey_root || wc)
        byte[] left = sha256.ComputeHash(Concat(pubkeyRoot, wc));
        // مرحله 5: right = sha256(amount_padded || signature_root)
        byte[] right = sha256.ComputeHash(Concat(amountPadded, signatureRoot));
        // مرحله 6: node = sha256(left || right)
        byte[] root = sha256.ComputeHash(Concat(left, right));
        return root;
    }
    /// تابع بررسی: محاسبه مجدد root و تطبیق با مقدار داده شده
    static bool VerifyDepositDataRoot(byte[] pubkey, byte[] wc, ulong amountGwei, byte[] signature, byte[] suppliedRoot)
    {
        byte[] computed = ComputeDepositDataRoot(pubkey, wc, amountGwei, signature);
        string compHex = "0x" + computed.ToHex();
        string suppHex = "0x" + suppliedRoot.ToHex();
        Console.WriteLine($"Root محاسبه شده: {compHex}");
        Console.WriteLine($"Root داده‌شده : {suppHex}");
        return compHex.Equals(suppHex, StringComparison.OrdinalIgnoreCase);
    }
    /// تابع کمکی اتصال بایت‌ها
    static byte[] Concat(byte[] a, byte[] b)
    {
        byte[] output = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, output, 0, a.Length);
        Buffer.BlockCopy(b, 0, output, a.Length, b.Length);
        return output;
    }
    /// تولید hex رندم با طول مشخص (بایت)
    static string RandomHex(int bytesLength)
    {
        byte[] buf = new byte[bytesLength];
        RandomNumberGenerator.Fill(buf);
        return BitConverter.ToString(buf).Replace("-", "").ToLowerInvariant();
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