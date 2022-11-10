namespace VpServiceAPI.Enums
{
    public enum FailSuccessStatus
    {
        SUCCESS,
        FAIL,
        NULL
    }

    public enum PlanProvideStatus
    {
        PLAN_NOT_FOUND,
        PLAN_FOUND,
        ERROR
    }
    public enum UpdateCheckStatus
    {
        IS_NEW,
        NOT_NEW,
        UNCLEAR,
        NULL
    }
}
