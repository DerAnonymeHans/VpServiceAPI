using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using VpServiceAPI.Entities;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using System.Net.Mail;
using System;
using VpServiceAPI.Tools;

namespace VpServiceAPI.Repositories
{
    public class TestUserRepository : IUserRepository
    {
        public async Task AddUserRequest(User user)
        {
            return;
        }

        public Task<User> GetUser(int id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<User>> GetUsers(string status="NORMAL")
        {
            return new List<User> 
            { 
                new User
                {
                    Name = "Pascal",
                    Address = "pascal.setzer@gmail.com",
                    Grade = "11"
                },
            };
        }

        public async Task<User> ValidateUser(string? name, string? mail, string? grade)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new AppException("Der Name darf nicht leer sein.");
            if (name.Length < 2) throw new AppException("Der Name muss mindestens 2 Zeichen lang sein.");
            if (name.Length > 20) throw new AppException("Der Name darf nicht länger als 20 Zeichen lang sein.");
            var matches = new Regex(@"[^a-zA-ZäöüÄÖÜß\s]").Matches(name);
            if (matches.Count > 0) throw new AppException($"Der Name beinhaltet folgende nicht erlaubte Zeichen: '{string.Join(", ", matches.Cast<Match>().Select(m => m.Value))}'");

            if(string.IsNullOrWhiteSpace(mail)) throw new AppException("Die Email darf nicht leer sein.");
            try
            {
                new MailAddress(mail);
            }
            catch (FormatException)
            {
                throw new AppException("Bitte gib eine valide Email Addresse an");
            }

            if (string.IsNullOrWhiteSpace(grade)) throw new AppException("Die Klassenstufe darf nicht leer sein.");
            string[] grades = new[] { "5", "6", "7", "8", "9", "10", "11", "12" };
            if (Array.IndexOf(grades, grade) == -1) throw new AppException("Bitte gib eine valide Klassenstufe an (5-12)");

            return new User { Name = name, Address = mail, Grade = grade };
        }

        public Task AcceptUser(string mail)
        {
            throw new NotImplementedException();
        }

        public Task RejectUser(string mail)
        {
            throw new NotImplementedException();
        }
    }

    public class ProdUserRepository : IUserRepository
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        public ProdUserRepository(IMyLogger logger, IDataQueries dataQueries)
        {
            Logger = logger;
            DataQueries = dataQueries;
        }

        public async Task AddUserRequest(User user)
        {
            await DataQueries.Save("INSERT INTO users(name, address, grade, status, sub_day) VALUES (@name, @address, @grade, 'REQUEST', @date)", new { name = user.Name, address = user.Address, grade = user.Grade, date = DateTime.Now.ToString("dd.MM.yyyy") });
        }

        public Task<User> GetUser(int id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<User>> GetUsers(string status="NORMAL")
        {
            //return await DataQueries.Load<User, dynamic>("SELECT name, address, grade FROM `users` WHERE status=@status ORDER BY `grade`", new { status });
            return await DataQueries.GetUsers(status);
        }

        public async Task<User> ValidateUser(string name, string mail, string grade)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new AppException("Der Name darf nicht leer sein.");
            if (string.IsNullOrWhiteSpace(mail)) throw new AppException("Die Email darf nicht leer sein.");
            if (string.IsNullOrWhiteSpace(grade)) throw new AppException("Die Klassenstufe darf nicht leer sein.");

            try
            {
                AttackDetector.Detect(name, mail, grade);
            }catch(AppException ex)
            {
                Logger.Warn(LogArea.Attack, ex, "Somebody tried to attack on user validation");
                throw new AppException(ex.Message);
            }

            if (name.Length < 2) throw new AppException("Der Name muss mindestens 2 Zeichen lang sein.");
            if (name.Length > 20) throw new AppException("Der Name darf nicht länger als 20 Zeichen lang sein.");
            var matches = new Regex(@"[^a-zA-ZäöüÄÖÜß\s]").Matches(name);
            if (matches.Count > 0) throw new AppException($"Der Name beinhaltet folgende nicht erlaubte Zeichen: '{string.Join(", ", matches.Cast<Match>().Select(m => m.Value))}'");

            try
            {
                new MailAddress(mail);                
            }
            catch (FormatException)
            {
                throw new AppException("Bitte gib eine valide Email Addresse an");
            }

            string[] grades = new[] { "5", "6", "7", "8", "9", "10", "11", "12" };
            if (Array.IndexOf(grades, grade) == -1) throw new AppException("Bitte gib eine valide Klassenstufe an (5-12)");

            var res = (await DataQueries.Load<int, dynamic>("SELECT id FROM users WHERE address=@address", new { address = mail }));
            if(res.Count != 0) throw new AppException("Die Email Addresse ist bereits in den Verteiler aufgenommen und kann nicht nochmals hinzugefügt werden.");

            return new User { Name = name, Address = mail, Grade = grade };
        }

        public async Task AcceptUser(string mail)
        {
            await DataQueries.Save("UPDATE users SET status='NORMAL' WHERE address=@address", new { address = mail });
            User user = (await DataQueries.Load<User, dynamic>("SELECT name, address, grade FROM users WHERE address=@address", new { address = mail }))[0];
            await DataQueries.AddUserToBackupDB(user);
        }

        public async Task RejectUser(string mail)
        {
            await DataQueries.Delete("DELETE FROM users WHERE address=@address AND status='REQUEST'", new { address = mail });
        }
    }
}
