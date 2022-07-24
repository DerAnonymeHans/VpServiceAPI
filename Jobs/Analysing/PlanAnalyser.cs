using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.Checking;
using VpServiceAPI.Jobs.StatProviding;

namespace VpServiceAPI.Jobs.Analysing
{
    public class PlanAnalyser : IPlanAnalyser
    {
        private PlanModel PlanModel { get; set; } = new();
        private readonly IMyLogger Logger;
        private readonly ITeacherRepository TeacherRepository;
        private readonly string[] Lessons = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private readonly char[] Letters = new[] { 'a', 'b', 'c', 'd', 'e', 'f' };

        public PlanAnalyser(IMyLogger logger, ITeacherRepository teacherRepository)
        {
            Logger = logger;
            TeacherRepository = teacherRepository;
        }

        public List<AnalysedRow> Begin(PlanModel planModel)
        {
            PlanModel = planModel;

            return AnalyseTable();
        }

        private List<AnalysedRow> AnalyseTable()
        {
            var newTable = new List<AnalysedRow>();
            foreach(var row in PlanModel.Table)
            {
                var analysedRow = AnalyseRow(row);
                if (analysedRow is null) continue;
                analysedRow.year = ProviderHelper.CurrentSchoolYear;
                newTable.Add(analysedRow);
            }
            return newTable;
        }
        private AnalysedRow? AnalyseRow(PlanRow row)
        {
            try
            {
                bool isMatch = false;
                isMatch = Regex.IsMatch(row.Info, @"gesamte|bei");
                if (isMatch) return GesamteCase(row);

                isMatch = Regex.IsMatch(row.Lehrer, @"(?<=\().+(?=\))");
                if (isMatch) return ParenthesisCase(row);

                isMatch = Regex.IsMatch(row.Info, @"fällt aus");
                if (isMatch) return FälltAusCase(row);

                isMatch = Regex.IsMatch(row.Info, @"für");
                if (isMatch) return FürCase(row);

            }catch (Exception ex)
            {
                Logger.Error(LogArea.PlanAnalysing, ex, "Tried to analyse row.", row);
                return null;
            }
            return null;
        }

        private AnalysedRow GesamteCase(PlanRow row)
        {
            var substTeacher = TeacherRepository.GetTeacher(row.Info.Split(' ').Last());
            if (substTeacher is null) return ParenthesisCase(row);
            return new AnalysedRow
            {
                date = PlanModel.MetaData.AffectedDate.Date,
                extra = row.Info,
                type = "VER",

                missing_teacher = GetTeacher(Regex.Match(row.Lehrer, @"(?<=\().+(?=\))").Value),
                missing_subject = GetSubject(row.Fach),
                substitute_teacher = substTeacher.ShortName,
                substitute_subject = GetSubject(row.Fach),
                lesson = GetLessons(row.Stunde),
                class_name = GetClasses(row.Klasse)
            };
        }

        private AnalysedRow ParenthesisCase(PlanRow row)
        {
            return new AnalysedRow
            {
                date = PlanModel.MetaData.AffectedDate.Date,
                extra = row.Info,
                type = "AUS",

                missing_teacher = GetTeacher(Regex.Match(row.Lehrer, @"(?<=\().+(?=\))").Value),
                missing_subject = GetSubject(row.Fach),
                lesson = GetLessons(row.Stunde),
                class_name = GetClasses(row.Klasse)
            };
        }
        private AnalysedRow FälltAusCase(PlanRow row)
        {
            return new AnalysedRow
            {
                date = PlanModel.MetaData.AffectedDate.Date,
                extra = row.Info,
                type = "AUS",

                missing_teacher = GetTeacher(new Regex(@"[0-9a-zA-ZäÄöÖüÜß]+(?=\sfällt aus)").Match(row.Info).Value),
                missing_subject = GetSubject(new Regex(@"[0-9a-zA-ZäÄöÖüÜß]+(?=\s[a-zA-ZäÄöÖüÜß]+\sfällt aus)").Match(row.Info).Value),
                lesson = GetLessons(row.Stunde),
                class_name = GetClasses(row.Klasse)
            };
        }
        private AnalysedRow FürCase(PlanRow row)
        {
            return new AnalysedRow
            {
                date = PlanModel.MetaData.AffectedDate.Date,
                extra = row.Info,
                type = "VER",

                missing_teacher = GetTeacher(new Regex(@"(?<=für)\s[0-9a-zA-ZäÄöÖüÜß]+\s([0-9a-zA-ZäÄöÖüÜß]+)").Matches(row.Info)[0].Groups[1].Value),
                substitute_teacher = GetTeacher(row.Lehrer),
                missing_subject = GetSubject(new Regex(@"(?<=für\s)[0-9a-zA-ZäÄöÖüÜß]+").Match(row.Info).Value),
                substitute_subject = GetSubject(row.Fach),
                lesson = GetLessons(row.Stunde),
                class_name = GetClasses(row.Klasse)
            };
        }



        private string GetLessons(string field)
        {
            string firstNumber = new Regex(@"\d").Match(field).Value;
            string secondNumber = new Regex(@"\d", RegexOptions.RightToLeft).Match(field).Value;
            if(firstNumber == secondNumber) return firstNumber;

            int idxFirst = Array.IndexOf(Lessons, firstNumber);
            int idxSecond = Array.IndexOf(Lessons, secondNumber);

            return string.Join(",", Lessons.Skip(idxFirst).Take(1 + idxSecond - idxFirst));
        }
        private string GetClasses(string field)
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
        private string GetTeacher(string field)
        {
            return field.Contains(',') ? field[0..field.IndexOf(',')] : field;
        }
        private string GetSubject(string field)
        {
            return Regex.Match(field, "[a-zA-Z]+").Value.ToUpper();
        }

    }
}
