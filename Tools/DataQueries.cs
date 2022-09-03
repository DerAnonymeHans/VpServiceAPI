using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Enums;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.StatProviding;

namespace VpServiceAPI.Tools
{
    public class DataQueries : IDataQueries
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


        public async Task<List<string>> GetRoutineData(string subject, string? name)
        {
            List<string> rows;
            if(name is null)
            {
                rows = await DataAccess.Load<string, dynamic>("SELECT `value` FROM `routine_data` WHERE `subject`=@subject", new { subject });
            }
            else
            {
                rows = await DataAccess.Load<string, dynamic>("SELECT `value` FROM `routine_data` WHERE `subject`=@subject AND `name`=@name", new { subject, name });
            }

            return rows;
        }
        public async Task SetRoutineData(string subject, string? name, string value)
        {
            if(name is null)
            {
                await DataAccess.Save("UPDATE `routine_data` SET `value`=@value WHERE `subject`=@subject", new { subject, value });
                return;
            }
            await DataAccess.Save("UPDATE `routine_data` SET `value`=@value WHERE `subject`=@subject AND `name`=@name", new { subject, name, value });
            
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
            await Save("INSERT INTO users(name, address, grade, status, sub_day) VALUES (@name, @address, @grade, 'NORMAL', '01.01.2022')", new { name = user.Name, address = user.Address, grade = user.Grade });
            DataAccess.SwitchToDB(1);
        }
    }


    public class UTestingDataQueries : IDataQueries
    {
        private readonly IDBAccess DataAccess;
        public UTestingDataQueries(IDBAccess dataAccess)
        {
            DataAccess = dataAccess;
        }


        public async Task<int> Delete<T>(string sql, T parameters)
        {
            return 0;
        }
        public async Task<List<T>> Load<T, U>(string sql, U parameters)
        {
            return await DataAccess.Load<T, U>(sql, parameters);
        }
        public async Task<int> Save<T>(string sql, T parameters)
        {
            return 0;
        }


        public async Task<List<T>> Select<T, U>(string table, string condition, U parameters)
        {
            return await Load<T, U>($"SELECT * FROM `{table}` WHERE {condition}", parameters);
        }


        public async Task<List<string>> GetRoutineData(string subject, string? name)
        {
            return RoutineData.Get(subject, name);
        }
        public async Task SetRoutineData(string subject, string? name, string value)
        {
            RoutineData.Set(subject, name, value);
        }

        public async Task AddStatEntitiy(string type, string name)
        {
            return;
        }

        public async Task<int> SaveAndGetId<T>(string sql, T parameters)
        {
            return await DataAccess.SaveAndGetId<T>(sql, parameters);
        }

        public async Task Upsert<T>(string updateSql, string insertSql, T parameters)
        {
            return;
        }

        public async Task AddUserToBackupDB(User user)
        {
            return;
        }
    }

    static class RoutineData
    {
        private static List<RoutineDataRow> Table = DefaultData();
        public static List<string> Get(string subject, string? name)
        {
            var result = new List<string>();
            foreach(var row in Table)
            {
                if (row.Subject != subject) continue;
                if (row.Name != name && name is not null) continue;
                result.Add(row.Value);
            }
            return result;
        }
        public static void Set(string subject, string? name, string value)
        {
            foreach (var row in Table)
            {
                if (row.Subject != subject) continue;
                if (row.Name != name && name is not null) continue;
                row.Value = value;
            }
        }
        public static void Reset()
        {
            Table = DefaultData();
        }
        public static List<RoutineDataRow> DefaultData()
        {
            var table = new List<RoutineDataRow>
            {
                new RoutineDataRow("DATETIME", "last_origin_datetime", "26.08.2022, 12:08"),
                new RoutineDataRow("DATETIME", "last_cache_delete", "26.08."),
                new RoutineDataRow("DATETIME", "last_stats_date", "26.08."),
                new RoutineDataRow("DATETIME", "last_affected_date", "29.08.2022."),

                new RoutineDataRow("EXTRA", "global_extra", ""),
                new RoutineDataRow("EXTRA", "special_extra", ""),
                new RoutineDataRow("EXTRA", "forced_artwork_name", "NICHTS"),

                new RoutineDataRow("CACHE", "information", ""),

                new RoutineDataRow("BACKUP", "date", ""),

                new RoutineDataRow("FORCE_MODE", "on_info_change", "false"),

                new RoutineDataRow("LAST_PLAN_CACHE", "5", ""),
                new RoutineDataRow("LAST_PLAN_CACHE", "6", ""),
                new RoutineDataRow("LAST_PLAN_CACHE", "7", ""),
                new RoutineDataRow("LAST_PLAN_CACHE", "8", ""),
                new RoutineDataRow("LAST_PLAN_CACHE", "9", ""),
                new RoutineDataRow("LAST_PLAN_CACHE", "10", ""),
                new RoutineDataRow("LAST_PLAN_CACHE", "11", ""),
                new RoutineDataRow("LAST_PLAN_CACHE", "12", ""),

                new RoutineDataRow("MODEL_CACHE", "5", ""),
                new RoutineDataRow("MODEL_CACHE", "6", ""),
                new RoutineDataRow("MODEL_CACHE", "7", ""),
                new RoutineDataRow("MODEL_CACHE", "8", ""),
                new RoutineDataRow("MODEL_CACHE", "9", ""),
                new RoutineDataRow("MODEL_CACHE", "10", ""),
                new RoutineDataRow("MODEL_CACHE", "11", ""),
                new RoutineDataRow("MODEL_CACHE", "12", ""),
                new RoutineDataRow("MODEL_CACHE", "12", ""),
                new RoutineDataRow("MODEL_CACHE", "global", ""),

                new RoutineDataRow("GRADE_MODE", "5", "false"),
                new RoutineDataRow("GRADE_MODE", "6", "NORMAL"),
                new RoutineDataRow("GRADE_MODE", "7", "NORMAL"),
                new RoutineDataRow("GRADE_MODE", "8", "NORMAL"),
                new RoutineDataRow("GRADE_MODE", "9", "NORMAL"),
                new RoutineDataRow("GRADE_MODE", "10", "NORMAL"),
                new RoutineDataRow("GRADE_MODE", "11", "NORMAL"),
                new RoutineDataRow("GRADE_MODE", "12", "NORMAL"),
                new RoutineDataRow("GRADE_MODE", "12", "NORMAL")
            };

            return table;
        }
    }
    public record RoutineDataRow
    {
        public RoutineDataRow(string subject, string name, string value)
        {
            Subject = subject;
            Name = name;
            Value = value;
        }

        public string Subject { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        
    }
}
