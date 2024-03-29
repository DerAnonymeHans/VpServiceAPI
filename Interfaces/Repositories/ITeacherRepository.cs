﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Persons;

namespace VpServiceAPI.Interfaces
{
    public interface ITeacherRepository
    {
        public Task UpdateTeacherList();
        public Teacher? GetTeacher(string name);
        public DateTime LastUpdate { get; set; }
    }
}
