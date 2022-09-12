using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Repositories
{
    public sealed class TeacherRepository : ITeacherRepository
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private readonly IWebScraper WebScraper;
        private List<Teacher> Teachers = new();

        public bool ShouldUpdateTeacherList => Teachers.Count == 0;

        public TeacherRepository(IMyLogger logger, IDataQueries dataQueries, IWebScraper webScraper)
        {
            Logger = logger;
            DataQueries = dataQueries;
            WebScraper = webScraper;
        }
        public Teacher? GetTeacher(string name)
        {
            foreach(var teacher in Teachers)
            {
                if (teacher.FullName == name || teacher.ShortName == name) return teacher;
            }
            return null;
        }

        public async Task UpdateTeacherList()
        {
            string HTML;
            var newTeachers = new List<Teacher>();
            try
            {
                HTML = await WebScraper.GetFromKepler("/kollegiuml");                
                HTML = HTML[HTML.IndexOf("<article")..HTML.IndexOf("</article>")];
                string tableBody = HTML[HTML.IndexOf("<tbody")..HTML.IndexOf("</tbody>")];

                for (int i = 0; i < 150; i++)
                {
                    // selects <tr> row
                    string tagName = "tr";
                    int trStartIdx = tableBody.IndexOf($"<{tagName}");
                    int trEndIdx = tableBody.IndexOf($"</{tagName}>");
                    if (trStartIdx == -1) break;
                    string rowStr = tableBody.Substring(trStartIdx + tagName.Length + 1, trEndIdx - trStartIdx);

                    // matches all >..</td> in the row, loops through them and selects the content
                    string[] rowArr = new Regex(@"(?<=>)[\w\s,äöüÄÖÜß]*(?=</td>)").Matches(rowStr)
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();

                    tableBody = tableBody[(trEndIdx + tagName.Length + 3)..];
                    if (rowArr.Length == 0 || rowArr is null) continue;
                    newTeachers.Add(new Teacher(
                        rowArr[0].Split(' ')[0],
                        rowArr[0].Split(' ')[1],
                        rowArr[1],
                        rowArr[2].Split(',')
                            .Select(sub => sub.Trim())
                            .ToList()
                        )
                    );
                }
            }
            catch(Exception ex)
            {
                Logger.Error(LogArea.Routine, ex, "Tried to update teacher list");
                return;
            }
            Teachers = newTeachers;
        }
    }
}
