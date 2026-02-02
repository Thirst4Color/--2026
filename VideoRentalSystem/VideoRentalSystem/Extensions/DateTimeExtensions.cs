using System;

namespace VideoRentalSystem.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToShortDateSafe(this DateTime? dateTime)
        {
            return dateTime?.ToString("dd.MM.yyyy") ?? "неизвестно";
        }

        public static string ToShortDateSafe(this DateTime? dateTime, string format)
        {
            return dateTime?.ToString(format) ?? "неизвестно";
        }
    }
}