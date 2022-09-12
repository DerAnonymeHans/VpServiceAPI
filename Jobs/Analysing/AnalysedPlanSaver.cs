using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.StatProviding;

namespace VpServiceAPI.Jobs.Analysing
{
    public sealed class AnalysedPlanSaver : IAnalysedPlanSaver
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        public AnalysedPlanSaver(IMyLogger logger, IDataQueries dataQueries)
        {
            Logger = logger;
            DataQueries = dataQueries;
        }
        public async Task DeleteOldRows(string date)
        {
            try
            {
                await DataQueries.Delete("DELETE FROM `vp_data` WHERE `date`=@date", new { date = date });
            }catch(Exception ex)
            {
                Logger.Error(LogArea.PlanAnalysing, ex, "Tried to delete old rows.", date);
            }
        }

        public async Task SaveRow(AnalysedRow row)
        {
            try
            {
                await DataQueries.Save("INSERT INTO `vp_data` (`year`, `type`, `missing_teacher`, `substitute_teacher`, `missing_subject`, `substitute_subject`, `lesson`, `class_name`, `date`, `room`, `extra`) VALUES (@year, @type, @missing_teacher, @substitute_teacher, @missing_subject, @substitute_subject, @lesson, @class_name, @date, @room, @extra)", row.ToParameters() );
            }
            catch(Exception ex)
            {
                Logger.Error(LogArea.PlanAnalysing, ex, "Tried to save analysed row.", row.ToParameters());
            }
        }

        public async Task SaveRows(List<AnalysedRow> rows)
        {
            foreach(AnalysedRow row in rows)
            {
                await SaveRow(row);
            }
        }
    }

}
