using System;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Notification
{
    public class UserTask
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private readonly IExtraRepository ExtraRepository;
        public UserTask(IMyLogger logger, IDataQueries dataQueries, IExtraRepository extraRepository)
        {
            Logger = logger;
            DataQueries = dataQueries;
            ExtraRepository = extraRepository;
        }

        public async Task<IUserNotificationBody> Begin(User user)
        {
            var body = new UserNotificationBody
            {
                UserName = user.Name,
                SmallExtra = await ExtraRepository.GetRandSmallExtra()
            };
            return body;
        }


    }
}
