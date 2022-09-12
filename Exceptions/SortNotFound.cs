using System;

namespace VpServiceAPI.Exceptions
{
    public sealed class SortNotFoundException : AppException
    {
        public SortNotFoundException(string name) : base($"Die Sortierung nach '{name}' wurde nicht gefunden.")
        {

        }
    }
}
