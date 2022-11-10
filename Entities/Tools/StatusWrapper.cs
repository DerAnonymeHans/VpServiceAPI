namespace VpServiceAPI.Entities.Tools
{
    public record StatusWrapper<TEnum, TBody>(TEnum Status, TBody? Body);

    
}
