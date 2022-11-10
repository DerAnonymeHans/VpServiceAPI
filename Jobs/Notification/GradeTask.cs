using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Notification;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Enums;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Notification
{
    public sealed class GradeTask
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private PlanCollection? PlanCollection { get; set; }
        public GradeTask(IMyLogger logger, IDataQueries dataQueries)
        {
            Logger = logger;
            DataQueries = dataQueries;
        }

        public async Task<IGradeNotificationBody> Begin(PlanCollection planCollection, string grade)
        {
            PlanCollection = planCollection;

            PlanRow[] cachedRows = await GetCachedRows(grade);

            var listOfTables = new List<List<NotificationRow>>();
            int planIdx = 0;
            foreach(var plan in planCollection.Plans)
            {
                listOfTables.Add(ConvertToNotificationRows(grade, plan.Rows, planIdx == 0 ? cachedRows.ToList() : new()));
                planIdx++;
            }
            bool isAffected = listOfTables.First().Any(row => row.HasChange);
            if (isAffected && Environment.GetEnvironmentVariable("VP_SOURCE") != "STATIC") await SetCachedRows(grade, listOfTables.First());

            return new GradeNotificationBody
            {
                Grade = grade,
                IsNotify = await IsSendMail(grade, isAffected),
                GradeExtra = await GetGradeExtra(grade),
                ListOfTables = listOfTables
            };
        }
        private async Task<bool> IsSendMail(string grade, bool isAffected)
        {
            bool isSendMail = isAffected;
            if (Environment.GetEnvironmentVariable("MODE") == "Testing") isSendMail = true;
            return await GetGradeMode(grade) switch
            {
                GradeMode.FORCE => true,
                GradeMode.SPECIAL_EXTRA_FORCE => true,
                GradeMode.STOP => false,
                _ => isSendMail
            };
        }
        private async Task<string?> GetGradeExtra(string grade)
        {
            try
            {
                var gradeMode = await GetGradeMode(grade);
                if (gradeMode != GradeMode.SPECIAL_EXTRA && gradeMode != GradeMode.SPECIAL_EXTRA_FORCE) return null;
                return (await DataQueries.GetRoutineData(RoutineDataSubject.EXTRA, "special_extra"))[0];
            }catch(Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, $"Tried to get grade extra for {grade}");
                return null;
            }
        }
        private async Task<GradeMode> GetGradeMode(string grade)
        {
            string gradeMode = "";
            try
            {
                gradeMode = (await DataQueries.GetRoutineData(RoutineDataSubject.GRADE_MODE, grade))[0];
            }catch(Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, $"Tried to get grade mode for {grade}");
            }
            return gradeMode switch
            {
                "STOP" => GradeMode.STOP,
                "FORCE" => GradeMode.FORCE,
                "SPECIAL_EXTRA" => GradeMode.SPECIAL_EXTRA,
                "SPECIAL_EXTRA_FORCE" => GradeMode.SPECIAL_EXTRA_FORCE,
                _ => GradeMode.NORMAL
            };
        }
        private List<NotificationRow> ConvertToNotificationRows(string grade, List<PlanRow> rows, List<PlanRow> cachedRows)
        {
            List<NotificationRow> notifRows = new();

            grade = grade == "11" || grade == "12" ? $"JG{grade}" : grade; // because of Kurs Bezeichnung - Kurs JG11/de123 would also count for grade 12
            foreach (var row in rows)
            {
                if (!row.Klasse.Contains(grade)) continue;

                int cachedRowIdx = cachedRows.IndexOf(row);
                // if row is new
                if (cachedRowIdx == -1)
                {
                    notifRows.Add(new NotificationRow
                    {
                        HasChange = true,
                        Row = row
                    });
                    continue;
                }

                // delete row from cached rows to later check for row deletions
                cachedRows.RemoveAt(cachedRowIdx);

                notifRows.Add(new NotificationRow
                {
                    HasChange = false,
                    Row = row
                });
            }
            foreach(var row in cachedRows)
            {
                notifRows.Add(new NotificationRow
                {
                    HasChange = true,
                    IsDeleted = true,
                    Row = row with
                    {
                        Klasse = "GELÖSCHT:\n" + row.Klasse,
                    }
                });
            }
            return notifRows;
        }                
        private async Task<PlanRow[]> GetCachedRows(string grade)
        {
            // LAST_PLAN_CACHE provides the last rows of the grade separated like aaaa|bbbb|cccc
            try
            {
                var json = (await DataQueries.GetRoutineData(RoutineDataSubject.PLAN_CACHE, grade))[0];
                if (json is null) return Array.Empty<PlanRow>();
                return JsonSerializer.Deserialize<PlanRow[]>(json) ?? Array.Empty<PlanRow>();
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to get cached plan data for grade " + grade);
                return Array.Empty<PlanRow>();
            }
        }
        private async Task SetCachedRows(string grade, List<NotificationRow> rows)
        {
            try
            {
                //string cache = string.Join('|', from notifRow in rows select string.Join(";", notifRow.Row.GetArray()));
                string json = JsonSerializer.Serialize(rows.FindAll(row => !row.IsDeleted).Select(row => row.Row));

                await DataQueries.SetRoutineData(RoutineDataSubject.PLAN_CACHE, grade, json);
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to set LAST_PLAN_CACHE for grade " + grade, rows);
            }
        }
    }
}
