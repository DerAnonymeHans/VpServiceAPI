using System.Collections.Generic;
using VpServiceAPI.Entities;
using VpServiceAPI.Entities.Plan;

namespace VpServiceAPI.Interfaces
{
    public interface IPlanAnalyser
    {
        public List<AnalysedRow> Analyse(PlanModel planModel);
    }
}
