﻿using VpServiceAPI.Enums;

namespace VpServiceAPI.Entities.Statistics
{
    public record StatEntity
    {
        public string Name { get; set; } = "";
        public EntityType Type { get; set; }
        public int Id { get; set; }
    }
}
