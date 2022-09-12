using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Tools
{
    public sealed class ConsoleOutputter : IOutputter
    {
        private static ConsoleColor ToConsoleColor(OutputColor color)
        {
            return color switch
            {
                OutputColor.Purple  => ConsoleColor.Magenta,
                OutputColor.Green   => ConsoleColor.Green,
                OutputColor.Blue    => ConsoleColor.Blue,
                OutputColor.Red     => ConsoleColor.Red,
                OutputColor.Yellow  => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };
        }
        public async Task Push(string output, OutputColor color = OutputColor.Blue)
        {
            Console.ForegroundColor = ToConsoleColor(color);
            Console.Write(output[0..19]);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(output[19..]);
        }

        public async Task Push(List<string> output, OutputColor color = OutputColor.Blue)
        {
            for(int i = 0; i < output.Count; i++)
            {
                string line = output[i];
                if(i == 0)
                {
                    Console.ForegroundColor = ToConsoleColor(color);
                    Console.Write(line[0..19]);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(line[19..]);
                    continue;
                }
                Console.WriteLine(line);
            }

        }
    }


    public sealed class DBOutputter : IOutputter
    {
        private readonly IDataQueries DataQueries;

        public DBOutputter(IDataQueries dataQueries)
        {
            DataQueries = dataQueries;
        }
        private static string ToType(OutputColor color)
        {
            return color switch
            {
                OutputColor.Purple => "ROUTINE",
                OutputColor.Green => "SUCCESS",
                OutputColor.Blue => "INFO",
                OutputColor.Red => "ERROR",
                OutputColor.Yellow => "WARN",
                _ => "NORMAL"
            };
        }
        public async Task Push(string output, OutputColor color = OutputColor.Blue)
        {
            string type = ToType(color);
            string time = output[0..19];
            string message = output[19..];
            await DataQueries.Save("INSERT INTO logs(time, type, message, extra) VALUES (@time, @type, @message, @extra)", new { time, type, message, extra = "" });
        }

        public async Task Push(List<string> output, OutputColor color = OutputColor.Blue)
        {
            string time = output[0][0..19];
            string type = ToType(color);
            string message = output[0][19..];
            List<string> extras = new();
            for (int i = 1; i < output.Count; i++) extras.Add(output[i]);
            string extra = string.Join(" | ", extras);
            await DataQueries.Save("INSERT INTO logs(time, type, message, extra) VALUES (@time, @type, @message, @extra)", new { time, type, message, extra });

        }
    }


    public sealed class ProdOutputter : IOutputter
    {
        private readonly ConsoleOutputter ConsoleOutputter;
        private readonly DBOutputter DBOutputter;
        public ProdOutputter(IDataQueries dataQueries)
        {
            ConsoleOutputter = new();
            DBOutputter = new(dataQueries);
        }

        public async Task Push(string output, OutputColor color = OutputColor.Blue)
        {
            await ConsoleOutputter.Push(output, color);
            await DBOutputter.Push(output, color);
        }

        public async Task Push(List<string> output, OutputColor color = OutputColor.Blue)
        {
            await ConsoleOutputter.Push(output, color);
            await DBOutputter.Push(output, color);
        }
    }
}
