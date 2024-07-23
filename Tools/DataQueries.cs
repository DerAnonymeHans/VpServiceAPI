using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Persons;
using VpServiceAPI.Enums;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.StatProviding;

namespace VpServiceAPI.Tools
{
    public sealed class DataQueries : IDataQueries
    {
        private readonly IDBAccess DataAccess;
        public DataQueries(IDBAccess dataAccess)
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


        public async Task<List<string>> GetRoutineData(RoutineDataSubject subject, string? name)
        {
            List<string> rows;
            if(name is null)
            {
                rows = await DataAccess.Load<string, dynamic>("SELECT `value` FROM `routine_data` WHERE `subject`=@subject", new { subject=subject.ToString() });
            }
            else
            {
                rows = await DataAccess.Load<string, dynamic>("SELECT `value` FROM `routine_data` WHERE `subject`=@subject AND `name`=@name", new { subject = subject.ToString(), name });
            }

            return rows;
        }
        public async Task SetRoutineData(RoutineDataSubject subject, string? name, string value)
        {
            if(name is null)
            {
                await DataAccess.Save("UPDATE `routine_data` SET `value`=@value WHERE `subject`=@subject", new { subject = subject.ToString(), value });
                return;
            }
            await DataAccess.Save("UPDATE `routine_data` SET `value`=@value WHERE `subject`=@subject AND `name`=@name", new { subject = subject.ToString(), name, value });
            
        }

        public async Task AddStatEntitiy(string type, string name)
        {
            await Save("INSERT INTO `stat_entities`(`type`, `name`, `year`) VALUES (@type, @name, @year)", new { type, name, year = ProviderHelper.CurrentSchoolYear });
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

        public async Task AddUserToBackupDB(User user)
        {
            if (DataAccess.CurrentDB == 2) return;
            DataAccess.SwitchToDB(2);
            await Save("INSERT INTO users(name, address, grade, status, sub_day, mode) VALUES (@name, @address, @grade, 'NORMAL', '01.01.2022', 'EMAIL')", new { name = user.Name, address = user.Address, grade = user.Grade });
            DataAccess.SwitchToDB(1);
        }
    }

}
