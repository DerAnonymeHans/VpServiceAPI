using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Plan;

namespace VpServiceAPI.Interfaces
{
    public interface IAnalysedPlanSaver
    {
        public Task SaveRow(AnalysedRow row);
        public Task SaveRows(List<AnalysedRow> rows);
        public Task DeleteOldRows(string date);
    }
}
