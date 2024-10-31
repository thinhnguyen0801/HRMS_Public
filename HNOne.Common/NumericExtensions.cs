using System.Globalization;

namespace HNOne.Common
{
    public static class NumericExtensions
    {

        public static string FormatCurrency(this object? value, string locate = "vi-VN")
        {
            if (value == null) return "0";
            CultureInfo culture = new CultureInfo(locate);
            if (value is double) return string.Format(culture, "{0:N0}", (double)value);
            if (value is decimal) return string.Format(culture, "{0:N0}", (decimal)value);
            if (value is int) return string.Format(culture, "{0:N0}", (int)value);
            throw new ArgumentException("Unsupported type");
        }
    }
    public static class DateTimeExtensions
    {
        public static string FormatDateTimeSql(this DateTime? value)
        {
            value ??= DateTime.Now;
            return string.Format("{0:yyy-MM-dd}", value);
        }
    }
}
