using System.Collections.Generic;

namespace VpServiceAPI.Entities.Persons
{
    public sealed class Teacher
    {
        public string Title { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public List<string> Subjects { get; set; }
        public Teacher(string title, string fullName, string shortName, List<string> subjects)
        {
            Title = title;
            FullName = fullName;
            ShortName = shortName;
            Subjects = subjects;
        }

    }
}
