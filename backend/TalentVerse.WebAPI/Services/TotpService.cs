using System.Security.Cryptography;
using System.Text;

namespace TalentVerse.WebAPI.Services
{
    public class TotpService
    {
        private const int TimeStep = 30;
        private const int CodeDigits = 6;

        public static string GenerateCode(string secret, long timeStepNumber)
        {
            var key = Base32Decode(secret);
            var counter = BitConverter.GetBytes(timeStepNumber);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counter);

            var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(counter);

            var offset = hash[hash.Length - 1] & 0x0F;
            var binary = ((hash[offset] & 0x7F) << 24)
                       | ((hash[offset + 1] & 0xFF) << 16)
                       | ((hash[offset + 2] & 0xFF) << 8)
                       | (hash[offset + 3] & 0xFF);

            var otp = binary % (int)Math.Pow(10, CodeDigits);
            return otp.ToString().PadLeft(CodeDigits, '0');
        }

        public static bool ValidateCode(string secret, string code, int timeToleranceSteps = 1)
        {
            var currentTimeStep = GetCurrentTimeStepNumber();

            // Check current time and +/- tolerance windows
            for (int i = -timeToleranceSteps; i <= timeToleranceSteps; i++)
            {
                var testCode = GenerateCode(secret, currentTimeStep + i);
                if (testCode == code)
                    return true;
            }

            return false;
        }

        private static long GetCurrentTimeStepNumber()
        {
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return unixTimestamp / TimeStep;
        }

        private static byte[] Base32Decode(string base32)
        {
            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            base32 = base32.TrimEnd('=').ToUpper();

            var bits = new System.Collections.BitArray(base32.Length * 5);
            for (int i = 0; i < base32.Length; i++)
            {
                var value = base32Chars.IndexOf(base32[i]);
                if (value < 0)
                    throw new ArgumentException("Invalid Base32 character");

                for (int j = 0; j < 5; j++)
                {
                    bits[i * 5 + j] = (value & (1 << (4 - j))) != 0;
                }
            }

            var bytes = new byte[bits.Length / 8];
            for (int i = 0; i < bytes.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (bits[i * 8 + j])
                        bytes[i] |= (byte)(1 << (7 - j));
                }
            }

            return bytes;
        }
    }
}
