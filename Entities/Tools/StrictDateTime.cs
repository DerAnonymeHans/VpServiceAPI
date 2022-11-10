using System;

namespace VpServiceAPI.Entities.Tools
{
    public sealed class StrictDateTime
    {
        public readonly string Format;
        private readonly DateTime? Fallback;
        public StrictDateTime(string format, DateTime? fallBack=null)
        {
            Format = format;
            Fallback = fallBack;
        }

        public DateTime DateTime { get; private set; }
        public override string ToString()
        {
            return DateTime.ToString(Format);
        }

        public void Set(string dateTime)
        {
            try
            {
                DateTime = DateTime.ParseExact(dateTime, Format, null);
            }
            catch(Exception)
            {
                if (Fallback is null) throw;
                DateTime = (DateTime)Fallback;
            }
        }
        public void Set(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public static bool operator ==(StrictDateTime a, StrictDateTime b) => a.DateTime == b.DateTime;
        public static bool operator !=(StrictDateTime a, StrictDateTime b) => a.DateTime != b.DateTime;
        public static bool operator >(StrictDateTime a, StrictDateTime b) => a.DateTime > b.DateTime;
        public static bool operator <(StrictDateTime a, StrictDateTime b) => a.DateTime < b.DateTime;
        public static bool operator >=(StrictDateTime a, StrictDateTime b) => a.DateTime >= b.DateTime;
        public static bool operator <=(StrictDateTime a, StrictDateTime b) => a.DateTime <= b.DateTime;

        public static bool operator ==(StrictDateTime a, DateTime b) => a.DateTime == b;
        public static bool operator !=(StrictDateTime a, DateTime b) => a.DateTime != b;
        public static bool operator >(StrictDateTime a, DateTime b) => a.DateTime > b;
        public static bool operator <(StrictDateTime a, DateTime b) => a.DateTime < b;
        public static bool operator >=(StrictDateTime a, DateTime b) => a.DateTime >= b;
        public static bool operator <=(StrictDateTime a, DateTime b) => a.DateTime <= b;

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
