using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Enums;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.StatProviding
{
    public sealed class GeneralProvider : IByGeneralProvider
    {
        private readonly IDataQueries DataQueries;
        private readonly IMyLogger Logger;
        private readonly IDBAccess DBAccess;
        public GeneralProvider(IDataQueries dataQueries, IMyLogger logger, IDBAccess dBAccess)
        {
            DataQueries = dataQueries;
            Logger = logger;
            DBAccess = dBAccess;
        }

        public async Task CheckDataAmount()
        {
            int rowCount = (await DataQueries.Load<int, dynamic>("SELECT COUNT(DISTINCT(date)) FROM vp_data WHERE year=@year", new { year = ProviderHelper.GetYear() }))[0];
            if (rowCount == 0) throw new AppException("Es scheint noch keine Daten für dieses Schuljahr zu geben. Versuche doch in ein älteres Schuljahr zu wechseln.");
        }

        public async Task CheckDataFreshness()
        {
            if (DBAccess.CurrentDB == 1) return;
            string backupDate = (await DataQueries.GetRoutineData(RoutineDataSubject.BACKUP, "date"))[0];
            throw new AppException($"Wegen eines Datenbank Problem kann es sein, dass die angezeigten Daten nicht ganz aktuell sind. Bei den Daten handelt es sich um ein Backup vom {backupDate}. Bitte entschuldige die Unannehmlichkeiten.");
        }

        public async Task<int> GetDaysCount()
        {
            return (await DataQueries.Load<int, dynamic>("SELECT COUNT(DISTINCT(date)) FROM vp_data WHERE year=@year", new { year = ProviderHelper.GetYear() }))[0];
        }

        public async Task<List<string>> GetNames(EntityType entityType)
        {
            return await DataQueries.Load<string, dynamic>("SELECT name FROM stat_entities WHERE type=@type AND year=@year", new { type = entityType.ToString(), year = ProviderHelper.GetYear() });
        }

        public async Task<List<string>> GetYears()
        {
            return await DataQueries.Load<string, dynamic>("SELECT DISTINCT(year) FROM `hard_stat_data` WHERE 1", new { });
        }
    }
}
