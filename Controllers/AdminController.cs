using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VpServiceAPI.Entities;
using VpServiceAPI.Enums;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using VpServiceAPI.WebResponse;

namespace VpServiceAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IMyLogger Logger;
        private readonly IRoutine Routine;
        private readonly IDataQueries DataQueries;
        private readonly IDBAccess DataAccess;
        private readonly IArtworkRepository ArtworkRepository;
        private readonly IStatExtractor StatExtractor;
        private readonly IUserRepository UserRepository;
        private readonly IExtraRepository ExtraRepository;
        private readonly IWebScraper WebScraper;

        private readonly WebResponder WebResponder;


        public AdminController(IMyLogger logger, IRoutine routine, IDataQueries dataQueries, IDBAccess dataAccess, IArtworkRepository artworkRepository, IStatExtractor statExtractor, IUserRepository userRepository, IExtraRepository extraRepository, IWebScraper webScraper)
        {
            Logger = logger;
            Routine = routine;
            DataQueries = dataQueries;
            DataAccess = dataAccess;
            ArtworkRepository = artworkRepository;
            StatExtractor = statExtractor;
            UserRepository = userRepository;
            ExtraRepository = extraRepository;

            WebResponder = new("Admin", LogArea.Admin, true, logger);
            WebScraper = webScraper;
        }


        [HttpPost]
        [Route("/Routine/BeginOnce")]
        public WebMessage RoutineBeginOnce()
        {
            return WebResponder.RunWithSync(Routine.BeginOnce, Request.Path.Value, "Started routine once", true);
        }

        [HttpPost]
        [Route("/Routine/Begin")]
        public WebMessage RoutineBegin()
        {
            return WebResponder.RunWithSync(Routine.Begin, Request.Path.Value, "Started routine");
        }
        [HttpPost]
        [Route("/Routine/ChangeInterval/{ms}")]
        public WebMessage RoutineChangeInterval(int ms = 30000)
        {
            if (ms < 30000) ms = 600_000;
            return WebResponder.RunWithSync(() => Routine.ChangeInterval(ms), Request.Path.Value, $"Changed interval to {ms}");
        }
        [HttpPost]
        [Route("/Routine/Stop")]
        public WebMessage RoutineStop()
        {
            return WebResponder.RunWithSync(Routine.End, Request.Path.Value, "Stopped routine");
        }
        [HttpGet]
        [Route("/Routine/Interval")]
        public WebResponse<int> GetRoutineInterval()
        {
            return WebResponder.RunWithSync(() => Routine.Interval, Request.Path.Value);
        }
        [HttpGet]
        [Route("/Routine/IsRunning")]
        public WebResponse<bool> GetRoutineIsRunning()
        {
            return WebResponder.RunWithSync(() => Routine.IsRunning, Request.Path.Value);
        }


        [HttpGet]
        [Route("/Notification/GlobalExtra")]
        public async Task<WebResponse<string>> GetGlobalExtra()
        {
            return await WebResponder.RunWith(async () => (await DataQueries.GetRoutineData(RoutineDataSubject.EXTRA, "global_extra"))[0], Request.Path.Value);
        }
        [HttpPost]
        [Route("/Notification/GlobalExtra/{value}")]
        public async Task<WebMessage> SetGlobalExtra(string value)
        {
            return await WebResponder.RunWith(async () => await DataQueries.SetRoutineData(RoutineDataSubject.EXTRA, "global_extra", value), Request.Path.Value, $"Set global extra to {value}");
        }

        [HttpGet]
        [Route("/Notification/SpecialExtra")]
        public async Task<WebResponse<string>> GetSpecialExtra()
        {
            return await WebResponder.RunWith(async () => (await DataQueries.GetRoutineData(RoutineDataSubject.EXTRA, "special_extra"))[0], Request.Path.Value);
        }
        [HttpPost]
        [Route("/Notification/SpecialExtra/{value}")]
        public async Task<WebMessage> SetSpecialExtra(string value)
        {
            return await WebResponder.RunWith(async () => await DataQueries.SetRoutineData(RoutineDataSubject.EXTRA, "special_extra", value), Request.Path.Value, $"Set special extra to {value}");
        }

        [HttpPost]
        [Route("/Notification/AddArtwork")]
        public async Task<WebMessage> AddArtwork(IFormFile artworkFile)
        {
            return await WebResponder.RunWith(async () =>
            {
                var artwork = ArtworkRepository.FormFileToArtwork(artworkFile, Request.Form);
                await ArtworkRepository.Add(artwork);
            }, Request.Path.Value, $"Added new Artwork {Request.Form["name"]}");
        }
        [HttpGet]
        [Route("/Notification/CurrentForcedArtwork")]
        public async Task<WebResponse<string>> GetCurrentForcedArtwork()
        {
            return await WebResponder.RunWith(async () => (await DataQueries.GetRoutineData(RoutineDataSubject.EXTRA, "forced_artwork_name"))[0], Request.Path.Value);            
        }
        [HttpPost]
        [Route("/Notification/ForceArtwork/{name}")]
        public async Task<WebMessage> SetForcedArtwork(string name)
        {
            return await WebResponder.RunWith(async () => await DataQueries.SetRoutineData(RoutineDataSubject.EXTRA, "forced_artwork_name", name), $"Set current forced artwork to {name}");
        }
        [HttpGet]
        [Route("/Notification/AllArtworks")]
        public async Task<WebResponse<List<string>>> GetAllArtworks()
        {
            return await WebResponder.RunWith(async () => await DataQueries.Load<string, dynamic>("SELECT name FROM artwork_data WHERE 1", new { }), Request.Path.Value);
        }

        [HttpGet]
        [Route("/Logs/Rows/{count}/{offset}")]
        public async Task<WebResponse<string[]>> GetLogRows(int count=1000, int offset=0)
        {
            return await WebResponder.RunWith(async () => 
                string.Join('|', 
                    await DataQueries.Load<LogRow, dynamic>(
                        "SELECT time, type, message, extra FROM logs ORDER BY STR_TO_DATE(`time`, '%d/%m/%Y %H:%i:%s') DESC, id DESC LIMIT @limit OFFSET @offset", 
                        new { limit = count, offset }
                    )
                ).Split('|')
            , Request.Path.Value);
        }
        [HttpPost]
        [Route("/DeleteOldLogs/Rows/{count}")]
        public async Task<WebMessage> DeleteLogRows(int count = 1000)
        {
            return await WebResponder.RunWith(async () =>
            {
                await DataQueries.Save<dynamic>(
                        "DELETE FROM `logs` WHERE 1 ORDER BY STR_TO_DATE(`time`, '%d/%m/%Y %H:%i:%s') ASC, id LIMIT @limit",
                        new { limit = count }
                    );
            }, Request.Path.Value);
        }
        [HttpGet]
        [Route("/Logs/Count")]
        public async Task<WebResponse<int>> GetLogCount()
        {
            return await WebResponder.RunWith(async () => (await DataQueries.Load<int, dynamic>("SELECT COUNT(id) FROM logs", new { }))[0], Request.Path.Value);
        }



        [HttpGet]
        [Route("MailRequests")]
        public async Task<WebResponse<List<User>>> GetMailRequests()
        {
            return await WebResponder.RunWith(async () => await UserRepository.GetUsers(UserStatus.REQUEST), Request.Path.Value);
        }
        [HttpPost]
        [Route("AcceptUser")]
        public async Task<WebMessage> AcceptUser()
        {
            string mail = Request.Form["mail"];
            return await WebResponder.RunWith(async () => await UserRepository.AcceptUser(mail), Request.Path.Value, $"Der Nutzer mit der mail {mail} wurde in den Verteiler aufgenommen");
        }
        [HttpPost]
        [Route("RejectUser")]
        public async Task<WebMessage> RejectUser()
        {
            string mail = Request.Form["mail"];
            return await WebResponder.RunWith(async () => await UserRepository.RejectUser(mail), Request.Path.Value);
        }

        [HttpGet]
        [Route("SmallExtraProposals")]
        public async Task<WebResponse<List<SmallExtra>>> GetSmallExtraProposals()
        {
            return await WebResponder.RunWith(ExtraRepository.GetProposals, Request.Path.Value);
        }
        [HttpPost]
        [Route("AcceptSmallExtra")]
        public async Task<WebMessage> AcceptSmallExtra()
        {
            string text = Request.Form["text"];
            return await WebResponder.RunWith(async () => await ExtraRepository.AcceptProposal(text), Request.Path.Value, $"Der Extra '{text}' wurde aufgenommen");
        }
        [HttpPost]
        [Route("RejectSmallExtra")]
        public async Task<WebMessage> RejectSmallExtra()
        {
            string text = Request.Form["text"];
            return await WebResponder.RunWith(async () => await ExtraRepository.RejectProposal(text), Request.Path.Value);
        }

        [HttpGet]
        [Route("Proposals")]
        public async Task<WebResponse<List<string>>> GetProposals()
        {
            return await WebResponder.RunWith(async () => await DataQueries.Load<string, dynamic>("SELECT text FROM proposals WHERE 1", new { }), Request.Path.Value);
        }

        [HttpGet]
        [Route("ForceModes")]
        public async Task<WebResponse<List<string>>> GetForceModes()
        {
            return await WebResponder.RunWith(async () => await DataQueries.Load<string, dynamic>("SELECT name FROM routine_data WHERE subject='FORCE_MODE'", new {}), Request.Path.Value);
        }
        [HttpGet]
        [Route("ForceMode/{name}")]
        public async Task<WebResponse<string>> GetForceMode(string name)
        {
            return await WebResponder.RunWith(async () => (await DataQueries.GetRoutineData(RoutineDataSubject.FORCE_MODE, name))[0], Request.Path.Value);
        }
        [HttpPost]
        [Route("ToggleForceMode/{name}")]
        public async Task<WebMessage> ToggleForceMode(string name)
        {
            try
            {
                await WebResponder.RunWith(async () => await DataQueries.Save("UPDATE routine_data SET value = IF(value = 'true', 'false', 'true') WHERE subject='FORCE_MODE' AND name=@name", new { name }), Request.Path.Value);
                return new WebMessage
                {
                    Message = $"Der FORCE_MODE {name} wurde getoggelt",
                    IsSuccess = true,
                };
            }catch(Exception ex)
            {
                return new WebMessage
                {
                    Message = ex.Message,
                    IsSuccess = false,
                };
            }
        }


        [HttpGet]
        [Route("GradeMode/{grade}")]
        public async Task<WebResponse<string>> GetGradeMode(string grade)
        {
            return await WebResponder.RunWith(async () => (await DataQueries.GetRoutineData(RoutineDataSubject.GRADE_MODE, grade))[0], Request.Path.Value);
        }
        [HttpPost]
        [Route("ChangeGradeMode/{grade}/{mode}")]
        public async Task<WebMessage> ChangeGradeMode(string grade, string mode)
        {
            try
            {
                var _mode = mode switch
                {
                    "normal" => GradeMode.NORMAL,
                    "force" => GradeMode.FORCE,
                    "stop" => GradeMode.STOP,
                    "special" => GradeMode.SPECIAL_EXTRA,
                    "special_force" => GradeMode.SPECIAL_EXTRA_FORCE,
                    _ => throw new AppException($"Specified mode '{mode}' is not available")
                };
                await WebResponder.RunWith(async () => await DataQueries.SetRoutineData(RoutineDataSubject.GRADE_MODE, grade, _mode.ToString()), Request.Path.Value);
                return new WebMessage
                {
                    Message = $"Der GRADE_MODE von {grade} wurde zu {_mode} geändert.",
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new WebMessage
                {
                    Message = ex.Message,
                    IsSuccess = false,
                };
            }
        }







        //private readonly Helper Helper = new Helper();

        //[HttpPost]
        //[Route("UpdatePlanData")]
        //public async void UpdatePlan()
        //{
        //    var rows = await DataQueries.Load<AnalysedRow, dynamic>("SELECT * FROM vp_data WHERE 1", new { });
        //    Logger.Debug(rows.Count);
        //    foreach(var row in rows)
        //    {
        //        //row.missing_teacher = Helper.GetTeacher(row.missing_teacher);
        //        //row.substitute_teacher = Helper.GetTeacher(row.substitute_teacher);
        //        row.missing_subject = Helper.GetSubject(row.missing_subject);
        //        row.substitute_subject = Helper.GetSubject(row.substitute_subject);
        //        //row.lesson = Helper.GetLessons(row.lesson);
        //        //row.class_name = Helper.GetClasses(row.class_name);

        //        //await DataQueries.Save("UPDATE vp_data SET missing_teacher=@missingTeacher, substitute_teacher=@substituteTeacher, missing_subject=@missingSubject, substitute_subject=@substituteSubject, lesson=@lesson, class_name=@className WHERE id=@id", new { missingTeacher = row.missing_teacher, substituteTeacher = row.substitute_teacher, missingSubject = row.missing_subject, substituteSubject = row.substitute_subject, lesson = row.lesson, className = row.class_name, id=row.id });
        //        await DataQueries.Save("UPDATE vp_data SET missing_subject=@missingSubject, substitute_subject=@substituteSubject WHERE id=@id", new { missingSubject = row.missing_subject, substituteSubject = row.substitute_subject, id=row.id });
        //    }
        //    Logger.Debug("Fertsch");
        //}

        [HttpPost]
        [Route("CreateAllStats")]
        public async void CreateStats()
        {
            //var date = new DateTime(2021, 11, 15);
            var date = new DateTime(2022, 06, 22);
            for (int i = 0; i < 300; i++)
            {
                Logger.Debug(date.ToString("dd.MM.yyyy"));
                if (date.ToString("dd.MM.yyyy") == "27.06.2022") return;
                await StatExtractor.Begin(date);
                date = date.AddDays(1);
            }
        }

        [HttpGet]
        [Route("/Test")]
        public async Task Test()
        {
            //var users = await UserRepository.GetUsers();
            //Logger.Debug(users);
            //Func<string, string> GetVar = (string name) => Environment.GetEnvironmentVariable(name);

            //DataAccess.ChangeConnection(GetVar("DB_2_HOST"), GetVar("DB_2_USER"), GetVar("DB_2_PW"), GetVar("DB_2_NAME"));
            //foreach(var user in users)
            //{
            //    await DataQueries.Save("INSERT INTO users(name, address, grade, status) VALUES (@name, @address, @grade, 'NORMAL')", new { name = user.Name, address = user.Address, grade = user.Grade });
            //}
        }


        //[HttpPost]
        //[Route("Transfer")]
        //public async void Save()
        //{
        //DataAccess.ChangeConnection("remotemysql.com", "xAF0qo85eJ", "a7oOyAjsoL", "xAF0qo85eJ");
        //var data = await DataQueries.Load<AnalysedRow, dynamic>("SELECT type, missing_teacher, substitute_teacher, missing_subject, substitute_subject, lesson, class AS class_name, date, room, extra FROM vp_data WHERE 1 ORDER BY date", new { });
        ////Logger.Debug(data);
        //Logger.Debug(data.Count);
        //DataAccess.ChangeConnection("remotemysql.com", "BpnKDg384A", "IU7oI1tAeT", "BpnKDg384A");
        //foreach (var row in data)
        //{
        //    await DataQueries.Save("INSERT INTO vp_data(type, missing_teacher, substitute_teacher, missing_subject, substitute_subject, lesson, class_name, date, room, extra) VALUES (@type, @miss_t, @sub_t, @miss_s, @sub_s, @lesson, @className, @date, @room, @extra)", new { type = row.type, miss_t = row.missing_teacher, sub_t = row.substitute_teacher, miss_s = row.missing_subject, sub_s = row.substitute_subject, lesson = row.lesson, className = row.class_name, date = row.date, room = row.room, extra = row.extra });
        //}
        //Logger.Debug("Fertsch");
        //}
    }

    public sealed class Helper
    {
        private readonly string[] Lessons = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private readonly char[] Letters = new[] { 'a', 'b', 'c', 'd', 'e', 'f' };

        public string GetLessons(string field)
        {
            string firstNumber = new Regex(@"\d").Match(field).Value;
            string secondNumber = new Regex(@"\d", RegexOptions.RightToLeft).Match(field).Value;
            if (firstNumber == secondNumber) return firstNumber;

            int idxFirst = Array.IndexOf(Lessons, firstNumber);
            int idxSecond = Array.IndexOf(Lessons, secondNumber);

            return string.Join(",", Lessons.Skip(idxFirst).Take(1 + idxSecond - idxFirst));
        }
        public string GetClasses(string field)
        {
            string grade = new Regex(@"\d+").Match(field).Value;

            if (grade == "11" || grade == "12") return field;
            if (field.Contains("/"))
            {
                field = field[0..field.IndexOf("/")];
            }

            char firstLetter = new Regex("[a-f]").Match(field).Value[0];
            char secondLetter = new Regex("[a-f]", RegexOptions.RightToLeft).Match(field).Value[0];
            if (firstLetter == secondLetter) return grade + firstLetter;

            int idxFirst = Array.IndexOf(Letters, firstLetter);
            int idxSecond = Array.IndexOf(Letters, secondLetter);

            return grade + string.Join($",{grade}", Letters.Skip(idxFirst).Take(1 + idxSecond - idxFirst));
        }
        public string GetTeacher(string field)
        {
            return field.Contains(',') ? field[0..field.IndexOf(',')] : field;
        }
        public string GetSubject(string field)
        {
            return Regex.Match(field, "[a-zA-Z]+").Value.ToUpper();
        }

    }
}
