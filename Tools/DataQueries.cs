using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Tools
{
    public class DataQueries : IDataQueries
    {
        private readonly IDataAccess DataAccess;
        public DataQueries(IDataAccess dataAccess)
        {
            DataAccess = dataAccess;
        }


        public async Task<int> Delete<T>(string sql, T parameters)
        {
            return await DataAccess.Delete<T>(sql, parameters);
        }
        public async Task<List<T>> Load<T, U>(string sql, U parameters)
        {
            return await DataAccess.Load<T, U>(sql, parameters);
        }
        public async Task<int> Save<T>(string sql, T parameters)
        {
            return await DataAccess.Save(sql, parameters);
        }


        public async Task<List<T>> Select<T, U>(string table, string condition, U parameters)
        {
            return await Load<T, U>($"SELECT * FROM `{table}` WHERE {condition}", parameters);
        }


        public async Task<List<string>> GetRoutineData(string subject, string? name)
        {
            List<string> rows;
            if(name is null)
            {
                rows = await DataAccess.Load<string, dynamic>("SELECT `value` FROM `routine_data` WHERE `subject`=@subject", new { subject = subject });
            }
            else
            {
                rows = await DataAccess.Load<string, dynamic>("SELECT `value` FROM `routine_data` WHERE `subject`=@subject AND `name`=@name", new { subject = subject, name = name });
            }

            return rows;
        }
        public async Task SetRoutineData(string subject, string name, string value)
        {
            await DataAccess.Save("UPDATE `routine_data` SET `value`=@value WHERE `subject`=@subject AND `name`=@name", new { subject = subject, name = name, value = value });
        }

        public void ChangeConnection(string? host, string? user, string? pw, string? dbName)
        {
            throw new System.NotImplementedException();
        }

        public async Task AddStatEntitiy(string type, string name)
        {
            await Save("INSERT INTO `stat_entities`(`type`, `name`) VALUES (@type, @name)", new { type = type, name = name });

        }

        public async Task<int> SaveAndGetId<T>(string sql, T parameters)
        {
            return await DataAccess.SaveAndGetId<T>(sql, parameters);
        }

        public async Task Upsert<T>(string updateSql, string insertSql, T parameters)
        {
            int rows = await Save(updateSql, parameters);
            if (rows > 0) return;
            await Save(insertSql, parameters);
        }
    }
}
