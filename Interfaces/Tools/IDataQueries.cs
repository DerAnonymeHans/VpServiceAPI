using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IDataQueries
    {
        public Task<List<T>> Load<T, U>(string sql, U parameters);
        public Task<int> Save<T>(string sql, T parameters);
        public Task<int> Delete<T>(string sql, T parameters);
        public Task<int> SaveAndGetId<T>(string sql, T parameters);
        public Task<List<string>> GetRoutineData(string subject, string? name);
        public Task SetRoutineData(string subject, string? name, string value);
        public Task<List<User>> GetUsers(string status);
        public Task AddUserToBackupDB(User user);
        public Task<List<T>> Select<T, U>(string table, string condition, U parameters);
        public Task AddStatEntitiy(string type, string name);
        public Task Upsert<T>(string updateSql, string insertSql, T parameters);
    }
}
