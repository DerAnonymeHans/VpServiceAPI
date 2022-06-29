using System.Collections.Generic;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IPlanAnalyser
    {
        public List<AnalysedRow> Begin(PlanModel planModel);
    }
}
