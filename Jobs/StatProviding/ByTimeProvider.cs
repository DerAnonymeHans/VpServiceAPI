using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Enums;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Tools;

namespace VpServiceAPI.Jobs.StatProviding
{
    public class ByTimeProvider : IByTimeProvider
    {
        private readonly IDataQueries DataQueries;
        private readonly IMyLogger Logger;
        public ByTimeProvider(IDataQueries dataQueries, IMyLogger logger)
        {
            DataQueries = dataQueries;
            Logger = logger;
        }
        private static string[] GetTimesArray(TimeType timeType)
        {
            return timeType switch
            {
                TimeType.month => Converter.Months,
                TimeType.day => Converter.Days,
                TimeType.lesson => Converter.Lessons
            };
        }
        private int[] ToArray(Times times, TimeType timeType)
        {
            return timeType switch
            {
                TimeType.month => new int[] { times.January, times.February, times.March, times.April, times.May, times.June, times.July, times.August, times.September, times.October, times.November, times.December },
                TimeType.day => new int[] { times.Monday, times.Tuesday, times.Wednesday, times.Thursday, times.Friday },
                TimeType.lesson => new int[] { times.First, times.Second, times.Third, times.Fourth, times.Fifth, times.Sixth, times.Seventh, times.Eigth }
            };
        }
        public async Task<TimeStatistic> GetTimesOf(TimeType timeType, string name)
        {
            string[] times = GetTimesArray(timeType);
            string sql = $"SELECT name, type, attendance, entity_id, {string.Join(", ", times)} FROM stat_entities e INNER JOIN stats_by_time t ON e.id = t.entity_id WHERE BINARY e.name=@name AND e.year=@year";

            var res = await DataQueries.Load<Times, dynamic>(sql, new { name, year = ProviderHelper.GetYear() });
            if (res.Count == 0) throw new NameNotFoundException(name);
            if(res.Count == 1)
            {
                res.Add(new Times
                {
                    Name = res[0].Name,
                    Type = res[0].Type,
                });
                if (res[0].Attendance == "Missing") res[1].Attendance = "Substituting";
                else res[1].Attendance = "Missing";
            }

            int[] missed = res[0].Attendance == "Missing" ? ToArray(res[0], timeType) : ToArray(res[1], timeType);
            int[] substituted = res[0].Attendance == "Substituting" ? ToArray(res[0], timeType) : ToArray(res[1], timeType);

            for(int i = 0; i < missed.Length; i++)
            {
                missed[i] -= substituted[i];
            }

            var stat = new TimeStatistic(name, substituted, missed) { Type = res[0].Type };
            return stat;

        }
        // sorts missed or substituted of one timename (january, third, thursday) of one entitytype
        public async Task<List<CountStatistic>> GetSortedTimeBy(TimeType timeType, string timeName, EntityType includeWho, string sortBy)
        {

            string attendance = sortBy[1] switch
            {
                'm' => "Missing",
                's' => "Substituting",
                _ => ""
            };

            string sortSql = sortBy switch
            {
                "mm" => $"missed DESC",
                "lm" => $"missed",
                "ms" => $"substituted DESC",
                "ls" => $"substituted",
                _ => throw new SortNotFoundException(sortBy)
            };

            // subtracting substituted from missed
            string selectSql = "SELECT name, type, entity_id AS myid";

            string selectMissedSql = $"((SELECT {timeName} FROM stats_by_time WHERE entity_id = myid AND attendance = 'Missing') - COALESCE((SELECT {timeName} FROM stats_by_time WHERE entity_id = myid AND attendance = 'Substituting'), 0)) AS missed";

            string selectSubstitutedSql = $"COALESCE((SELECT {timeName} FROM stats_by_time WHERE entity_id = myid AND attendance = 'Substituting'), 0) AS substituted";

            string fromSql = $"FROM stat_entities e INNER JOIN stats_by_time t ON e.id = t.entity_id WHERE e.type = @type AND attendance = @attendance AND e.year=@year ORDER BY {sortSql} LIMIT @limit";

            string sql = $"{selectSql}, {selectMissedSql}, {selectSubstitutedSql} {fromSql}";
            var res = await DataQueries.Load<CountStatistic, dynamic>(sql, new { type = includeWho.ToString(), attendance, year = ProviderHelper.GetYear(), limit = 10 });
            if (res.Count == 0) throw new NameNotFoundException(includeWho.ToString());

            return res;
        }

    }


    public class Times
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Attendance { get; set; }

        public int January { get; set; }
        public int February { get; set; }
        public int March { get; set; }
        public int April { get; set; }
        public int May { get; set; }
        public int June { get; set; }
        public int July { get; set; }
        public int August { get; set; }
        public int September { get; set; }
        public int October { get; set; }
        public int November { get; set; }
        public int December { get; set; }

        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }

        public int First { get; set; }
        public int Second { get; set; }
        public int Third { get; set; }
        public int Fourth { get; set; }
        public int Fifth { get; set; }
        public int Sixth { get; set; }
        public int Seventh { get; set; }
        public int Eigth { get; set; }

        public Times()
        {

        }
        public Times(
            string name, string type, string attendance,
            int january, int february, int march, int april, int may, int june, int july, int august, int september, int october, int november, int december,
            int monday, int tuesday, int wednesday, int thursday, int friday,
            int first, int second, int third, int fourth, int fifth, int sixth, int seventh, int eigth
            )
        {
            Name = name;
            Type = type;
            Attendance = attendance;

            January = january;
            February = february;
            March = march;
            April = april;
            May = may;
            June = june;
            July = july;
            August = august;
            September = september;
            October = october;
            November = november;
            December = december;

            Monday = monday;
            Tuesday = tuesday;
            Wednesday = wednesday;
            Thursday = thursday;
            Friday = friday;

            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
            Sixth = sixth;
            Seventh = seventh;
            Eigth = eigth;
        }
    }

    public struct Months
    {
        public int January { get; set; }
        public int February { get; set; }
        public int March { get; set; }
        public int April { get; set; }
        public int May { get; set; }
        public int June { get; set; }
        public int July { get; set; }
        public int August { get; set; }
        public int September { get; set; }
        public int October { get; set; }
        public int November { get; set; }
        public int December { get; set; }

        public Months(int january, int february, int march, int april, int may, int june, int july, int august, int september, int october, int november, int december)
        {
            January = january;
            February = february;
            March = march;
            April = april;
            May = may;
            June = june;
            July = july;
            August = august;
            September = september;
            October = october;
            November = november;
            December = december;
        }
    }
    public struct Days
    {
        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }

        public Days(int monday, int tuesday, int wednesday, int thursday, int friday)
        {
            Monday = monday;
            Tuesday = tuesday;
            Wednesday = wednesday;
            Thursday = thursday;
            Friday = friday;
        }
    }
    public struct Lessons
    {
        public int First { get; set; }
        public int Second { get; set; }
        public int Third { get; set; }
        public int Fourth { get; set; }
        public int Fifth { get; set; }
        public int Sixth { get; set; }
        public int Seventh { get; set; }
        public int Eigth { get; set; }
        public Lessons(int first, int second, int third, int fourth, int fifth, int sixth, int seventh, int eigth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
            Sixth = sixth;
            Seventh = seventh;
            Eigth = eigth;
        }
    }
}
