using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IDBAccess
    {
        public int CurrentDB { get; }
        public Task<List<T>> Load<T, U>(string sql, U parameters, int tryNumber = 1);
        public Task<int> Save<T>(string sql, T parameters, int tryNumber=1);
        public Task<int> Delete<T>(string sql, T parameters);
        public Task<int> SaveAndGetId<T>(string sql, T parameters);
        public void ChangeConnection(string? host, string? user, string? pw, string? dbName);
        public void SwitchToDB(int number);
    }
}
