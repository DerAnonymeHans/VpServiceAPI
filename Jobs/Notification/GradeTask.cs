using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Notification
{
    public class GradeTask
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
            List<NotificationRow> rows = GetNotificationRows(cachedRows, grade, out bool hasChange);
            if (hasChange && Environment.GetEnvironmentVariable("VP_SOURCE") != "STATIC") await SetCachedRows(grade, rows);

            List<NotificationRow> rows2 = new();
            if(PlanModel.Table2 is not null)
            {
                rows2 = GetNotificationRows(new string[0], grade, out bool dummy, true);
            }

            if (Environment.GetEnvironmentVariable("MODE") == "Testing") hasChange = true;

            return new GradeNotificationBody
            {
                Grade = grade,
                IsAffected = hasChange,
                Rows = rows,
                Rows2 = rows2,
            };
        }
        private List<NotificationRow> GetNotificationRows(string[] cachedRows, string grade, out bool hasChange, bool isSecondPlan=false)
        {
            List<NotificationRow> rows = new();
            hasChange = false;
            var table = isSecondPlan ? PlanModel.Table2 : PlanModel.Table;
            if (table is null) return rows;
            foreach (var row in table)
            {
                if (row.Klasse.IndexOf(grade) == -1) continue;

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
