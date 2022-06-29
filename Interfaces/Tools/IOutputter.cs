
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VpServiceAPI.Interfaces
{
    public interface IOutputter
    {
        public Task Push(string output, OutputColor color = OutputColor.Blue);
        public Task Push(List<string> output, OutputColor color = OutputColor.Blue);
    }

    public enum OutputColor
    {
        Blue,
        Green,
        Red,
        Yellow,
        Purple
    }
}
