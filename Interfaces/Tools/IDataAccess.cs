using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IDataAccess
    {
        public Task<List<T>> Load<T, U>(string sql, U parameters);
        public Task<int> Save<T>(string sql, T parameters);
        public Task<int> Delete<T>(string sql, T parameters);
        public Task<int> SaveAndGetId<T>(string sql, T parameters);
        public void ChangeConnection(string? host, string? user, string? pw, string? dbName);
    }
}
