using System;

namespace VpServiceAPI.Interfaces
{
    public interface IMyLogger
    {
        public void Routine(string name);
        public void Debug(string message);
        public void Debug<T>(string message, T arg);
        public void Debug<T>(T arg);
        public void Info(string message);
        public void Info(LogArea area, string message);
        public void Info<T>(LogArea area, string message, T arg);
        public void Warn<T>(LogArea area, string message, T arg);
        public void Warn(LogArea area, Exception ex, string message);
        public void Warn<T>(LogArea area, Exception ex, string message, T arg);
        public void Error(LogArea area, Exception ex, string message);
        public void Error<T>(LogArea area, Exception ex, string message, T arg);
        
    }

    public enum LogArea
    {
        PlanProviding,
        PlanConverting,
        PlanChecking,
        Notification,
        Routine,
        PlanAnalysing,
        DataAccess,
        StatExtraction,
        StatAPI,
        UserAPI,
        Admin,
        Attack
    }
}
