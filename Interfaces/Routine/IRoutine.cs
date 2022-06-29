namespace VpServiceAPI.Interfaces
{
    public interface IRoutine
    {
        public int Interval { get; }
        public bool IsRunning { get; }
        public void Begin();
        public void BeginOnce();
        public void End();
        public void ChangeInterval(int interval);
    }
}
