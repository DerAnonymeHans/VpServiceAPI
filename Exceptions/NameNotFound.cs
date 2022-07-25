﻿using System;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Exceptions
{
    public class NameNotFoundException : AppException
    {
        public NameNotFoundException(string name) : base($"Der Name '{name}' wurde nicht gefunden.")
        {

        }

        public NameNotFoundException(EntityType type, string name) : base($"Der Name '{name}' des Typs '{type.ToString()}' wurde nicht gefunden.")
        {

        }
    }
}