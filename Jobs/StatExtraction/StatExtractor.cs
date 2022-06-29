﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Enums;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.StatExtraction
{
    public class StatExtractor : IStatExtractor
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private DateTime Date { get; set; }

        private List<AnalysedRow> Rows { get; set; } = new();
        

        public StatExtractor(IMyLogger logger, IDataQueries dataQueries)
        {
            Logger = logger;
            DataQueries = dataQueries;

        }
        public async Task Begin(DateTime date)
        {
            Date = date;
            if (await StatsAlreadyExtracted()) return;
            await SetLastStatDate();

            Logger.Info(LogArea.StatExtraction, "Begin extracting stats...");
            var rows = await GetRows();
            if (rows is null)
            {
                Logger.Info(LogArea.StatExtraction, "Stopped extracting stats because rows is null");
                return;
            };
            Rows = rows;
            Logger.Info(LogArea.StatExtraction, $"Row count is {Rows.Count}");
            await CycleRows();
            Logger.Info(LogArea.StatExtraction, "Finished extracting stats.");
        }
        private async Task<bool> StatsAlreadyExtracted()
        {
            try
            {
                string lastTime = (await DataQueries.GetRoutineData("DATETIME", "last_stats_date"))[0];
                return lastTime == Date.ToString("dd.MM.");
            }catch(Exception ex)
            {
                Logger.Error(LogArea.StatExtraction, ex, "Tried to get last_stats_date");
                return true;
            }
        }
        private async Task SetLastStatDate()
        {
            try
            {
                await DataQueries.SetRoutineData("DATETIME", "last_stats_date", Date.ToString("dd.MM."));
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.StatExtraction, ex, "Tried to update last_stats_date");
            }
        }
        private async Task<List<AnalysedRow>?> GetRows()
        {
            try
            {
                return await DataQueries.Load<AnalysedRow, dynamic>("SELECT * FROM vp_data WHERE date=@date", new { date = Date.ToString("dd.MM.yyyy") });
            }catch(Exception ex)
            {
                Logger.Error(LogArea.StatExtraction, ex, "Tried to extract analysed rows for date " + Date.ToString("dd.MM.yyyy"));
                return null;
            }
        }
        private async Task CycleRows()
        {

            foreach(var row in Rows)
            {
                List<string> classNames = row.class_name.Split(',').ToList();
                classNames.Add(new Regex(@"\d+").Match(classNames[0]).Value);
                classNames.Add("kepler");

                int i = 0;
                foreach(string className in classNames)
                {
                    await ExtractData(row, className, i>0);
                    i++;
                }

            }
        }
        private async Task ExtractData(AnalysedRow row, string className, bool rowAlreadyChanged)
        {
            var matches = new Regex(@"\d").Matches(row.lesson);
            int[] lessons = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach(Match match in matches)
            {
                int idx = int.Parse(match.Value) - 1;
                lessons[idx] = 1;
            }
            int lessonCount = matches.Count;
            var missingTriangle = new RelationTriangle(Logger, DataQueries, row.missing_teacher, className, row.missing_subject, Date, lessons)
            {
                LessonCount = lessonCount,
                Attendance = Attendance.Missing,
                WasRowAlreadyChanged = rowAlreadyChanged
            };

            await missingTriangle.Save();


            if (row.type == "VER")
            {
                var substituteTriangle = new RelationTriangle(Logger, DataQueries, row.substitute_teacher, className, row.substitute_subject, Date, lessons)
                {
                    LessonCount = lessonCount,
                    Attendance = Attendance.Substituting,
                    WasRowAlreadyChanged = rowAlreadyChanged
                };
                await substituteTriangle.Save();
            }
        }
    }

    

   
}
