using Microsoft.Extensions.Primitives;
using System;
using System.Text.RegularExpressions;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Tools;

namespace VpServiceAPI.Jobs.StatProviding
{
    public static class ProviderHelper
    {
        public static string CurrentSchoolYear => new DateTime(DateTime.Now.Year - (DateTime.Now.Month < 8 ? 1 : 0), 1, 1).ToString("yy");
        public static string GetYear()
        {
            try
            {
                //var form = Tools.AppContext.Current.Request.Form;
                //var year = form["year"];
                //if (year.Count != 1) return CurrentSchoolYear;
                //year = year[0];
                var searchParams = Tools.AppContext.Current?.Request.Query;
                if (searchParams is null) return CurrentSchoolYear;
                if (!searchParams.ContainsKey("year")) return CurrentSchoolYear;
                var year = searchParams["year"];
                if (Regex.Match(year, @"\d{2}").Value != year) throw new AppException("Das angegebene Schuljahr muss folgende Struktur besitzen ('21', '22', '23', ...).");
                return year;
            }catch(AppException ex)
            {
                throw ex;
            }
        }
    }
}
