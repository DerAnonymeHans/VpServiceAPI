using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IUserRepository
    {
        public Task<List<User>> GetUsers(string status="NORMAL");
        public Task<User> GetUser(int id);
        public Task<User> ValidateUser(string name, string mail, string grade);
        public Task AddUserRequest(User user);
        public Task AcceptUser(string mail);
        public Task RejectUser(string mail);
    }
}
