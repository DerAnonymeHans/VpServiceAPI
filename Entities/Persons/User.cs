using System;
using VpServiceAPI.Entities.Lernsax;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Entities.Persons
{
    public sealed class User
    {
        public int Id { get; set; }
        public string Name { get; init; }
        public string Address { get; init; }
        public string Grade { get; init; }
        public UserStatus Status { get; init; }
        public NotifyMode NotifyMode { get; init; }
        public string SubDay { get; init; }
        public string? ResetKey { get; init; }
        public string? PushSubscribtion { get; init; }
        public LernsaxCredentials? LernsaxCredentials { get; set; }
        public User(int id, string name, string address, string grade, string status, string mode, string sub_day, string? reset_key, string? push_subscribtion)
        {
            Id = id;
            Name = name;
            Address = address;
            Grade = grade;
            Status = status switch
            {
                "REQUEST" => UserStatus.REQUEST,
                "SKIP" => UserStatus.SKIP,
                "NORMAL" => UserStatus.NORMAL,
                _ => UserStatus.NORMAL
            };
            NotifyMode = mode switch
            {
                "EMAIL" => NotifyMode.EMAIL,
                "PWA" => NotifyMode.PWA,
                _ => NotifyMode.EMAIL
            };
            SubDay = sub_day;
            ResetKey = string.IsNullOrEmpty(reset_key) ? null : reset_key;
            PushSubscribtion = push_subscribtion;
        }
    }
}
