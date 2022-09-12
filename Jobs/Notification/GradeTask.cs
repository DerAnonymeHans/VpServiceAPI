using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Enums;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Notification
{
    public sealed class GradeTask
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private PlanModel PlanModel { get; set; } = new();
        public GradeTask(IMyLogger logger, IDataQueries dataQueries)
        {
            Logger = logger;
            DataQueries = dataQueries;
        }

        public async Task<IGradeNotificationBody> Begin(PlanModel planModel, string grade)
        {
            PlanModel = planModel;
            string[] cachedRows = await GetCachedRows(grade);
            List<NotificationRow> rows = GetNotificationRows(cachedRows, grade, out bool isAffected);
            if (isAffected && Environment.GetEnvironmentVariable("VP_SOURCE") != "STATIC") await SetCachedRows(grade, rows);

            List<NotificationRow> rows2 = new();
            if(PlanModel.Table2 is not null)
            {
                rows2 = GetNotificationRows(new string[0], grade, out bool _, true);
            }            

            return new GradeNotificationBody
            {
                Grade = grade,
                IsNotify = await IsSendMail(grade, isAffected),
                GradeExtra = await GetGradeExtra(grade),
                Rows = rows,
                Rows2 = rows2,
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
                return (await DataQueries.GetRoutineData("EXTRA", "special_extra"))[0];
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
                gradeMode = (await DataQueries.GetRoutineData("GRADE_MODE", grade))[0];
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
        private List<NotificationRow> GetNotificationRows(string[] cachedRows, string grade, out bool hasChange, bool isSecondPlan=false)
        {
            List<NotificationRow> rows = new();
            hasChange = false;
            var table = isSecondPlan ? PlanModel.Table2 : PlanModel.Table;
            if (table is null) return rows;
            grade = grade == "11" || grade == "12" ? $"JG{grade}" : grade; // because of Kurs Bezeichnung - Kurs JG11/de123 would also count for grade 12
            foreach (var row in table)
            {
                if (!row.Klasse.Contains(grade)) continue;

                if (Array.IndexOf(cachedRows, string.Join(';', row.GetArray())) == -1)
                {
                    hasChange = true;
                    rows.Add(new NotificationRow
                    {
                        HasChange = !isSecondPlan, // only if first plan
                        Row = row
                    });
                    continue;
                }
                rows.Add(new NotificationRow
                {
                    HasChange = false,
                    Row = row
                });
            }
            return rows;
        }                
        private async Task<string[]> GetCachedRows(string grade)
        {
            // LAST_PLAN_CACHE provides the last rows of the grade separated like aaaa|bbbb|cccc
            try
            {
                var cache = (await DataQueries.GetRoutineData("LAST_PLAN_CACHE", grade))[0];
                if (cache is null) return Array.Empty<string>();
                return cache.Split('|');
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to get cached plan data for grade " + grade);
                return Array.Empty<string>();
            }
        }
        private async Task SetCachedRows(string grade, List<NotificationRow> rows)
        {
            try
            {
                string cache = string.Join('|', from notifRow in rows select string.Join(";", notifRow.Row.GetArray()));

                await DataQueries.SetRoutineData("LAST_PLAN_CACHE", grade, cache);
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to set LAST_PLAN_CACHE for grade " + grade, rows);
            }
        }
    }
}
