using System.Collections.Generic;
using System.Threading.Tasks;

namespace VpServiceAPI.Interfaces
{
    public interface IDataQueries : IDataAccess
    {
        public Task<List<string>> GetRoutineData(string subject, string? name);
        public Task SetRoutineData(string subject, string name, string value);
        public Task<List<T>> Select<T, U>(string table, string condition, U parameters);
        public Task AddStatEntitiy(string type, string name);
        public Task Upsert<T>(string updateSql, string insertSql, T parameters);
    }
}
