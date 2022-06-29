using System;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using VpServiceAPI.Interfaces;
using MySql.Data.MySqlClient;
using Dapper;
using System.Threading.Tasks;

namespace VpServiceAPI.Tools
{
    public class DBAccess : IDataAccess
    {

        private string DBConString = $"server={Environment.GetEnvironmentVariable("DB_HOST")};uid={Environment.GetEnvironmentVariable("DB_USER")};pwd={Environment.GetEnvironmentVariable("DB_PW")};database={Environment.GetEnvironmentVariable("DB_NAME")}";
        //private readonly IMyLogger Logger;
        public DBAccess(/*IMyLogger logger*/)
        {
            //Logger = logger;
            var host = Environment.GetEnvironmentVariable("DB_HOST");
            ChangeConnection(
                Environment.GetEnvironmentVariable("DB_HOST"),
                Environment.GetEnvironmentVariable("DB_USER"),
                Environment.GetEnvironmentVariable("DB_PW"),
                Environment.GetEnvironmentVariable("DB_NAME")
            );
        }
        public async Task<List<T>> Load<T, U>(string sql, U parameters)
        {
            var con = new MySqlConnection(DBConString);
            var rows = await con.QueryAsync<T>(sql, parameters);
            con.Close();
            return rows.ToList();
        }

        public async Task<int> Save<T>(string sql, T parameters)
        {
            var con = new MySqlConnection(DBConString);
            int affectedRows = await con.ExecuteAsync(sql, parameters);
            con.Close();
            return affectedRows;
        }

        public async Task<int> Delete<T>(string sql, T parameters)
        {
            return await Save(sql, parameters);
        }

        public void ChangeConnection(string? host, string? user, string? pw, string? dbName)
        {
            DBConString = $"SERVER={host}; DATABASE={dbName}; UID={user}; PASSWORD={pw}";
            //Logger.Warn(LogArea.DataAccess, "Changed Database Connection to:", DBConString);
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
    }
}
