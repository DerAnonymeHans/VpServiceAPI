using System;
using System.Collections.Generic;
using VpServiceAPI.Entities.Lernsax;

namespace VpServiceAPI.Entities.Persons
{
    public sealed class UserWithLernsax
    {
        public User User { get; init; }
        public Lernsax.Lernsax Lernsax { get; init; }
        public UserWithLernsax(User user, Lernsax.Lernsax lernsax)
        {
            User = user;
            Lernsax = lernsax;
        }
    }

}
