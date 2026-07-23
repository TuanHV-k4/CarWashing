using System.Text.RegularExpressions;

namespace BusinessLayer.Helpers
{
    public static class NormalizationHelper
    {
        public static string? NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            var digits = Regex.Replace(phone.Trim(), @"[^\d+]", "");

            if (digits.StartsWith("+84"))
                return "+84" + digits[3..];

            if (digits.StartsWith("84") && digits.Length >= 10)
                return "+" + digits;

            if (digits.StartsWith('0') && digits.Length >= 10)
                return "+84" + digits[1..];

            return digits;
        }

        public static string NormalizeLicensePlate(string plate)
        {
            return Regex.Replace(plate.Trim().ToUpperInvariant(), @"[\s\-\.]", "");
        }
    }
}
