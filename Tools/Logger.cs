using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Tools
{
    public sealed class Logger : IMyLogger
    {
        private readonly IOutputter outputter;
        private Dictionary<string, Stopwatch> StopWatches = new();
        public Logger(IOutputter outputter)
        {
            this.outputter = outputter;
        }


        private static string CurrentDateTime { get { return DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); } }
        private static readonly string Space = "   ";
        private static readonly int MAX_CHARS = 200;
        private static string SpliceText(string? text, int lineLength)
        {
            return Regex.Replace(text ?? "", "(.{" + lineLength + "})", "$1" + Environment.NewLine + Space);
        }


        public void Error(LogArea area, Exception ex, string message)
        {
            var output = new List<string>
            {
                $"{CurrentDateTime}{Space}ERROR: {area}: {message}",
                $"{Space}Message: {SpliceText(ex.Message, MAX_CHARS)}",
                $"{Space}Trace: {SpliceText(ex.StackTrace, MAX_CHARS)}",
            };
            outputter.Push(output, OutputColor.Red);
        }

        public void Error<T>(LogArea area, Exception ex, string message, T arg)
        {
            var output = new List<string>
            {
                $"{CurrentDateTime}{Space}ERROR: {area}: {message}",
                $"{Space}Message: {SpliceText(ex.Message, MAX_CHARS)}",
                $"{Space}Arg: {SpliceText(JsonSerializer.Serialize(arg), MAX_CHARS)}",
                $"{Space}Trace: {SpliceText(ex.StackTrace, MAX_CHARS)}"
            };
            outputter.Push(output, OutputColor.Red);
        }

        public void Warn(LogArea area, Exception ex, string message)
        {
            var output = new List<string>
            {
                $"{CurrentDateTime}{Space}WARNING: {area}: {message}",
                $"{Space}Message: {SpliceText(ex.Message, MAX_CHARS)}",
            };
            outputter.Push(output, OutputColor.Yellow);
        }
        public void Warn<T>(LogArea area, Exception ex, string message, T arg)
        {
            var output = new List<string>
            {
                $"{CurrentDateTime}{Space}WARNING: {area}: {message}",
                $"{Space}Message: {SpliceText(ex.Message, MAX_CHARS)}",
                $"{Space}Arg: {SpliceText(JsonSerializer.Serialize(arg), MAX_CHARS)}",
            };
            outputter.Push(output, OutputColor.Yellow);
        }
        public void Warn<T>(LogArea area, string message, T arg)
        {
            var output = new List<string>
            {
                $"{CurrentDateTime}{Space}INFO: {area}: {message}",
                $"{Space}Arg: {SpliceText(JsonSerializer.Serialize(arg), MAX_CHARS)}"
            };
            outputter.Push(output, OutputColor.Yellow);
        }

        public void Info(string message)
        {
            outputter.Push($"{CurrentDateTime}{Space}INFO: {message}");
        }
        public void Info(LogArea area, string message)
        {
            outputter.Push($"{CurrentDateTime}{Space}INFO: {area}: {message}");
        }
        public void Info<T>(LogArea area, string message, T arg)
        {
            var output = new List<string>
            {
                $"{CurrentDateTime}{Space}INFO: {area}: {message}",
                $"{Space}Arg: {SpliceText(JsonSerializer.Serialize(arg), MAX_CHARS)}"
            };
            outputter.Push(output);
        }

        public void Routine(string name)
        {
            outputter.Push($"{CurrentDateTime}{Space}ROUTINE: {name}", OutputColor.Purple);
        }

        public void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("DEBUG: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }
        public void Debug<T>(string message, T arg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("DEBUG: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.WriteLine($"{Space}{SpliceText(JsonSerializer.Serialize(arg), MAX_CHARS)}");
        }
        public void Debug<T>(T arg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("DEBUG: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(SpliceText(JsonSerializer.Serialize(arg), MAX_CHARS));
        }


        public void StartTimer(string label)
        {
            var sw = new Stopwatch();
            sw.Start();
            StopWatches.Add(label, sw);
        }

        public void EndTimer(string label)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("TIMER: ");
            Console.ForegroundColor = ConsoleColor.White;
            var sw = StopWatches[label];
            sw.Stop();
            Console.WriteLine($"{label}: {sw.Elapsed.TotalMilliseconds}");
            StopWatches.Remove(label);
        }
    }
}
