using System;

namespace VpServiceAPI.Exceptions
{
    public class SortNotFoundException : AppException
    {
        public SortNotFoundException(string name) : base($"Die Sortierung nach '{name}' wurde nicht gefunden.")
        {

        }
    }
}
