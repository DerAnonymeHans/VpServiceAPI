using System;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using VpServiceAPI.Interfaces;
using MySql.Data.MySqlClient;
using Dapper;
using System.Threading.Tasks;
using VpServiceAPI.Exceptions;

namespace VpServiceAPI.Tools
{
    public sealed class DBAccess : IDBAccess
    {
        private string DBConString { get; set; } = "";
        public int CurrentDB { get; private set; }

        public DBAccess()
        {
            SwitchToDB(1);
        }
        public async Task<List<T>> Load<T, U>(string sql, U parameters, int tryNumber=1)
        {
            try
            {
                var con = new MySqlConnection(DBConString);
                var rows = await con.QueryAsync<T>(sql, parameters);
                con.Close();
                return rows.ToList();
            }
            catch (MySqlException ex)
            {
                if (ex.Message != "Unable to connect to any of the specified MySQL hosts.") throw ex;
                if (tryNumber == 2) throw ex;
                SwitchToDB(tryNumber + 1);
                return await Load<T, U>(sql, parameters, tryNumber + 1);
            }
        }

        public async Task<int> Save<T>(string sql, T parameters, int tryNumber = 1)
        {
            try
            {
                var con = new MySqlConnection(DBConString);
                int affectedRows = await con.ExecuteAsync(sql, parameters);
                con.Close();
                return affectedRows;
            }
            catch (MySqlException ex)
            {
                if (ex.Message != "Unable to connect to any of the specified MySQL hosts.") throw ex;
                if (tryNumber == 2) throw ex;
                SwitchToDB(tryNumber + 1);
                return await Save(sql, parameters, tryNumber + 1);
            }
        }

        public async Task<int> Delete<T>(string sql, T parameters)
        {
            return await Save(sql, parameters);
        }

        public void ChangeConnection(string? host, string? user, string? pw, string? dbName)
        {
            DBConString = $"SERVER={host}; DATABASE={dbName}; UID={user}; PASSWORD={pw}";


            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Changed Database Connection to: " + DBConString);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public async Task<int> SaveAndGetId<T>(string sql, T parameters)
        {
            var con = new MySqlConnection(DBConString);
            await con.ExecuteAsync(sql, parameters);
            int id = (await con.QueryAsync<int>(sql, parameters)).ToList()[0];

            con.Close();
            return id;
        }

        public void SwitchToDB(int number)
        {
            CurrentDB = number;
            Func<string, string> GetDBVar = (string name) => 
                Environment.GetEnvironmentVariable($"DB_{number}_{name}") 
                ?? throw new AppException($"Failed to switch to DB {number} because of missing env var 'DB_{number}_{name}'");

            ChangeConnection(GetDBVar("HOST"), GetDBVar("USER"), GetDBVar("PW"), GetDBVar("NAME"));
        }
    }
}
