using System;
using System.Threading.Tasks;

namespace VpServiceAPI.Interfaces
{
    public interface IStatExtractor
    {
        public Task Begin(DateTime date);
    }
}
