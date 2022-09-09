using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface ITeacherRepository
    {
        public Task UpdateTeacherList();
        public Teacher? GetTeacher(string name);
        public bool ShouldUpdateTeacherList { get; }
    }
}
