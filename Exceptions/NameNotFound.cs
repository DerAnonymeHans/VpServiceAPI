using System;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Exceptions
{
    public sealed class NameNotFoundException : AppException
    {
        public NameNotFoundException(string name) : base($"Der Name '{name}' wurde nicht gefunden.")
        {

        }

        public NameNotFoundException(EntityType type, string name) : base($"Der Name '{name}' des Typs '{type}' wurde nicht gefunden.")
        {

        }
    }
}
