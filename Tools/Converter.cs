using System;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Tools
{
    public sealed class Converter
    {
        public static readonly string[] Months = new[] { "january", "february", "march", "april", "may", "june", "july", "august", "september", "october", "november", "december" };
        public static readonly string[] Days = new[] { "monday", "tuesday", "wednesday", "thursday", "friday" };
        public static readonly string[] Lessons = new[] { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eigth" };
        public static int MonthToNumber(string month)
        {
            return month switch
            {
                "Januar" => 1,
                "Februar" => 2,
                "März" => 3,
                "April" => 4,
                "Mai" => 5,
                "Juni" => 6,
                "Juli" => 7,
                "August" => 8,
                "September" => 9,
                "Oktober" => 10,
                "November" => 11,
                "Dezember" => 12,
                _ => 8
            };
        }
        public static string NumberToDBMonth(string number)
        {
            return number switch
            {
                "01" => "january",
                "02" => "february",
                "03" => "march",
                "04" => "april",
                "05" => "may",
                "06" => "june",
                "07" => "july",
                "08" => "august",
                "09" => "september",
                "10" => "october",
                "11" => "november",
                "12" => "december",
                _ => "august"
            };
        }
        public static TimeType? TimeNameToType(string timeName)
        {
            if (Array.IndexOf(Months, timeName) != -1) return TimeType.month;
            if (Array.IndexOf(Days, timeName) != -1) return TimeType.month;
            if (Array.IndexOf(Lessons, timeName) != -1) return TimeType.month;
            return null;
        }
    }
}
