using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.Checking;
using VpServiceAPI.Jobs.StatProviding;
using VpServiceAPI.Entities.Plan;

namespace VpServiceAPI.Jobs.Analysing
{
    public sealed class PlanAnalyser : IPlanAnalyser
    {
        private readonly IMyLogger Logger;
        private readonly ITeacherRepository TeacherRepository;
        private readonly string[] Lessons = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private readonly char[] Letters = new[] { 'a', 'b', 'c', 'd', 'e', 'f' };

        public PlanAnalyser(IMyLogger logger, ITeacherRepository teacherRepository)
        {
            Logger = logger;
            TeacherRepository = teacherRepository;
        }

        public List<AnalysedRow> Analyse(PlanModel planModel)
        {
            var analysedRows = new List<AnalysedRow>();
            foreach (var row in planModel.Rows)
            {
                var analysedRow = AnalyseRow(row);
                if (analysedRow is null) continue;

                if (analysedRow.ClassName.Length == 0 
                    || analysedRow.MissingSubject.Length == 0 
                    || analysedRow.MissingTeacher.Length == 0
                    || analysedRow.Lesson.Length == 0) continue;

                if(analysedRow.Type == "VER")
                {
                    if(analysedRow.SubstituteSubject.Length == 0 || analysedRow.SubstituteTeacher.Length == 0) continue;
                }

                analysedRow.Date = planModel.AffectedDate.Date;
                analysedRow.Extra = row.Info;
                analysedRow.Year = ProviderHelper.CurrentSchoolYear;

                analysedRows.Add(analysedRow);
            }
            return analysedRows;
        }
        public AnalysedRow? AnalyseRow(PlanRow row)
        {
            try
            {
                bool isMatch = false;
                isMatch = Regex.IsMatch(row.Info, @"\sgesamte\s|\sbei\s|\smit\s");
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
            }
            return null;
        }

        private AnalysedRow GesamteCase(PlanRow row)
        {
            var substTeacher = TeacherRepository.GetTeacher(row.Info.Split(' ').Last());
            if (substTeacher is null) return ParenthesisCase(row);
            return new AnalysedRow("VER")
            {
                MissingTeacher = GetTeacher(Regex.Match(row.Lehrer, @"(?<=\().+(?=\))").Value),
                MissingSubject = GetSubject(row.Fach),
                SubstituteTeacher = substTeacher.ShortName,
                SubstituteSubject = GetSubject(row.Fach),
                Lesson = GetLessons(row.Stunde),
                ClassName = GetClasses(row.Klasse)
            };
        }

        private AnalysedRow ParenthesisCase(PlanRow row)
        {
            return new AnalysedRow("AUS")
            {
                MissingTeacher = GetTeacher(Regex.Match(row.Lehrer, @"(?<=\().+(?=\))").Value),
                MissingSubject = GetSubject(row.Fach),
                Lesson = GetLessons(row.Stunde),
                ClassName = GetClasses(row.Klasse)
            };
        }
        private AnalysedRow FälltAusCase(PlanRow row)
        {
            return new AnalysedRow("AUS")
            {
                MissingTeacher = GetTeacher(new Regex(@"[0-9a-zA-ZäÄöÖüÜß]+(?=\sfällt aus)").Match(row.Info).Value),
                MissingSubject = GetSubject(new Regex(@"[\wÄÖÜäöüß/]+(?=\s[a-zA-ZäÄöÖüÜß/]+\sfällt aus)").Match(row.Info).Value),
                Lesson = GetLessons(row.Stunde),
                ClassName = GetClasses(row.Klasse)
            };
        }
        private AnalysedRow FürCase(PlanRow row)
        {
            return new AnalysedRow("VER")
            {
                MissingTeacher = GetTeacher(new Regex(@"(?<=für)\s[\wÄÖÜäöüß/]+\s([0-9a-zA-ZäÄöÖüÜß]+)").Matches(row.Info)[0].Groups[1].Value),
                SubstituteTeacher = GetTeacher(row.Lehrer),
                MissingSubject = GetSubject(new Regex(@"(?<=für\s)[\wÄÖÜäöüß/]+").Match(row.Info).Value),
                SubstituteSubject = GetSubject(row.Fach),
                Lesson = GetLessons(row.Stunde),
                ClassName = GetClasses(row.Klasse)
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
            return Regex.Match(field, ".+").Value.ToUpper();
        }

    }
}
