using System;

namespace VpServiceAPI.Entities
{
    public class Timespan
    {
        public (int day, int month) From { get; init; }
        public (int day, int month) To { get; init; }
        public bool IncludeAll { get; init; }
        public Timespan((int day, int month)from, (int day, int month)to)
        {
            From = from;
            To = to;
        }
        public Timespan(string from, string to)
        {
            if(from.Length == 0 || to.Length == 0 || from == "0.0." || to == "0.0.")
            {
                IncludeAll = true;
                return;
            }

            var fromSplitted = from.Split('.');
            var toSplitted = to.Split('.');            
            From = (int.Parse(fromSplitted[0]), int.Parse(fromSplitted[1]));
            To = (int.Parse(toSplitted[0]), int.Parse(toSplitted[1]));
        }
        public Timespan(bool includeAll)
        {
            IncludeAll = includeAll;
        }

        public bool Includes(DateTime datetime)
        {
            if (IncludeAll) return true;
            if (datetime.Month > To.month) return false;
            if (datetime.Month < From.month) return false;
            if(datetime.Month == To.month)
            {
                if(datetime.Day > To.day) return false;
            }
            if(datetime.Month == From.month)
            {
                if (datetime.Day < From.day) return false;
            }
            return true;
        }

        public string FromToString()
        {
            return $"{From.day}.{From.month}.";
        }
        public string ToToString()
        {
            return $"{To.day}.{To.month}.";
        }
    }
}
