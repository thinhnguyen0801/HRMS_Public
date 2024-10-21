using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Common
{
    public interface IDateTimeHelper
    {
        DateTime GetCurrentVietnamTime();
        DateTime? ToDateLunar(DateTime? gregorianDate);
        DateTime? ToDateGregorian(DateTime? lunarCalendar);
    }

    public class DateTimeHelper : IDateTimeHelper
    {
        public DateTime GetCurrentVietnamTime()
        {
            // Get Vietnam's time zone
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
        }

        /// <summary>
        /// convert Gregorian to Lunar
        /// </summary>
        /// <param name="gregorianDate"></param>
        /// <returns></returns>
        public DateTime? ToDateLunar(DateTime? gregorianDate)
        {
            if (gregorianDate == null) return null;
            ChineseLunisolarCalendar lunarCalendar = new ChineseLunisolarCalendar();
            int year = lunarCalendar.GetYear(gregorianDate.Value);
            int month = lunarCalendar.GetMonth(gregorianDate.Value);
            int day = lunarCalendar.GetDayOfMonth(gregorianDate.Value);
            return new DateTime(year, month, day);
        }

        /// <summary>
        /// convert Lunar to Gregorian
        /// </summary>
        /// <param name="lunarCalendar"></param>
        /// <returns></returns>
        public DateTime? ToDateGregorian(DateTime? lunarDate)
        {
            if (lunarDate == null) return null;
            ChineseLunisolarCalendar lunarCalendar = new ChineseLunisolarCalendar();
            int year = lunarDate.Value.Year;
            int month = lunarDate.Value.Month;
            int day = lunarDate.Value.Day;
            return lunarCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
        }
    }
}
