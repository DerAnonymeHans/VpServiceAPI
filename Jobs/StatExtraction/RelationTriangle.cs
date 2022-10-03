using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Enums;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.StatProviding;

namespace VpServiceAPI.Jobs.StatExtraction
{
    public sealed class RelationTriangle
    {
        public StatEntity Teacher { get; } = new StatEntity { Type = EntityType.TEACHER };
        public StatEntity Subject { get; } = new StatEntity { Type = EntityType.SUBJECT };
        public StatEntity ClassName { get; } = new StatEntity();
        public Attendance Attendance { get; init; }
        public int LessonCount { get; init; } = 0;
        public bool WasRowAlreadyChanged { get; init; } = false;
        private DateTime Date { get; }
        private int[] Lessons { get; }


        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;

        public RelationTriangle(IMyLogger logger, IDataQueries dataQueries, string teacher, string className, string subject, DateTime date, int[] lessons)
        {
            Logger = logger;
            DataQueries = dataQueries;

            Teacher.Name = teacher;
            Subject.Name = subject;
            ClassName.Name = className;
            ClassName.Type = GetClassType(className);

            Date = date;
            Lessons = lessons;
        }

        public async Task Save()
        {
            if (Teacher.Name.Length == 0 || Subject.Name.Length == 0 || ClassName.Name.Length == 0)
            {
                Logger.Warn(LogArea.StatExtraction, "Not saving row because teacher, subject or classname lenth was 0", this);
                return;
            }
            await InsertMissingEntities();
            await GetIds();
            await SaveBys();            
        }
        private static EntityType GetClassType(string className)
        {
            if (className == new Regex(@"\d+").Match(className).Value) return EntityType.GRADE;
            if (new Regex(@"JG").IsMatch(className)) return EntityType.KURS;
            if (className == "kepler") return EntityType.KEPLER;
            return EntityType.CLASS;
        }
        private async Task InsertMissingEntities()
        {
            try
            {
                var entities = await DataQueries.Load<StatEntity, dynamic>("SELECT type, name FROM stat_entities WHERE ( BINARY name=@teacher OR BINARY name=@subject OR BINARY name=@className) AND year=@year", new { teacher = Teacher.Name, subject = Subject.Name, className = ClassName.Name, year = ProviderHelper.CurrentSchoolYear });

                var entityNames = (from entity in entities select entity.Name).ToList();

                if (entityNames.IndexOf(Teacher.Name) == -1)
                {
                    await DataQueries.AddStatEntitiy(Teacher.Type.ToString(), Teacher.Name);
                }
                if (entityNames.IndexOf(Subject.Name) == -1)
                {
                    await DataQueries.AddStatEntitiy(Subject.Type.ToString(), Subject.Name);
                }
                if (entityNames.IndexOf(ClassName.Name) == -1)
                {
                    await DataQueries.AddStatEntitiy(ClassName.Type.ToString(), ClassName.Name);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.StatExtraction, ex, "Tried to insert missing entities.");
            }
        }
        private async Task GetIds()
        {
            var entities = await DataQueries.Select<StatEntity, dynamic>("stat_entities", "(name=@teacher OR name=@subject OR BINARY name=@className) AND year=@year", new { teacher = Teacher.Name, subject = Subject.Name, className = ClassName.Name, year = ProviderHelper.CurrentSchoolYear });
            foreach(StatEntity entity in entities)
            {
                switch (entity.Type)
                {
                    case EntityType.TEACHER:
                        Teacher.Id = entity.Id;
                        break;
                    case EntityType.SUBJECT:
                        Subject.Id = entity.Id;
                        break;
                    default:
                        ClassName.Id = entity.Id;
                        break;
                }
            }
        }


        private async Task SaveBys()
        {
            int missed = Attendance == Attendance.Missing ? LessonCount : 0;
            int substituted = Attendance == Attendance.Missing ? 0 : LessonCount;

            StatEntity[] entities = new[] { Subject, Teacher, ClassName };

            foreach (StatEntity entity in entities)
            {
                if (WasRowAlreadyChanged && (entity.Type == EntityType.TEACHER || entity.Type == EntityType.SUBJECT)) continue;
                await SaveCount(entity, missed, substituted);
                await SaveTime(entity, Attendance.ToString());
            }

            if (ClassName.Type == EntityType.KEPLER) return;

            await SaveWho(ClassName, Teacher, missed, substituted);
            await SaveWho(ClassName, Subject, missed, substituted);
            if (WasRowAlreadyChanged) return;
            await SaveWho(Subject, Teacher, missed, substituted);
        }

        private async Task SaveCount(StatEntity entity, int missed, int substituted)
        {
            try
            {
                await DataQueries.Upsert("UPDATE stats_by_count SET missed=missed + @missed, substituted=substituted + @substituted WHERE entity_id=@entityId", "INSERT INTO stats_by_count(year, entity_id, missed, substituted) VALUES (@year, @entityId, @missed, @substituted)", new { year = ProviderHelper.CurrentSchoolYear, entityId = entity.Id, missed, substituted });
            }catch(Exception ex)
            {
                Logger.Error(LogArea.StatExtraction, ex, "Tried to upsert stats_by_count");
            }
        }
        private async Task SaveTime(StatEntity entity, string attendance)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                string month = Date.ToString("MMMM").ToLower();
                string day = Date.ToString("dddd").ToLower();
                await DataQueries.Upsert(
                    $"UPDATE stats_by_time " +
                    $"SET {month}={month}+@lessonCount, {day}={day}+@lessonCount, first=first+@first, second=second+@second, third=third+@third, fourth=fourth+@fourth, fifth=fifth+@fifth, sixth=sixth+@sixth, seventh=seventh+@seventh, eigth=eigth+@eigth " +
                    $"WHERE entity_id=@entityId AND attendance=@attendance", 

                    $"INSERT INTO stats_by_time(year, entity_id, attendance, {month}, {day}, first, second, third, fourth, fifth, sixth, seventh, eigth) " +
                    $"VALUES (@year, @entityId, @attendance, @lessonCount, @lessonCount, @first, @second, @third, @fourth, @fifth, @sixth, @seventh, @eigth)", 
                    new { year = ProviderHelper.CurrentSchoolYear, entityId = entity.Id, attendance, lessonCount = LessonCount, first = Lessons[0], second = Lessons[1], third = Lessons[2], fourth = Lessons[3], fifth = Lessons[4], sixth = Lessons[5], seventh = Lessons[6], eigth = Lessons[7] }
                    );
            }catch(Exception ex)
            {
                Logger.Error(LogArea.StatExtraction, ex, "Tried to upsert stats_by_time");
            }
        }
        private async Task SaveWho(StatEntity a, StatEntity b, int missed, int substituted)
        {
            try
            {
                await DataQueries.Upsert(
                    "UPDATE stats_by_who SET missed=missed + @missed, substituted=substituted + @substituted WHERE (entity_id_a=@idA AND entity_id_b=@idB) OR (entity_id_a=@idB AND entity_id_b=@idA)",
                    "INSERT INTO stats_by_who(year, entity_id_a, entity_id_b, missed, substituted) VALUES (@year, @idA, @idB, @missed, @substituted)",
                    new { year = ProviderHelper.CurrentSchoolYear, idA = a.Id, idB = b.Id, missed, substituted }
                    );
            }catch(Exception ex)
            {
                Logger.Error(LogArea.StatExtraction, ex, "Tried to upsert stats_by_who");
            }
        }
    }
}




