namespace VpServiceAPI.Entities
{
    public record StatusWrapper<TBody>(Status Status, TBody? Body);

    public enum Status
    {
        SUCCESS,
        FAIL,
        NULL
    }
}
