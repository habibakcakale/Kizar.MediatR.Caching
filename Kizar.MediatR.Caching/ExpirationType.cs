namespace Kizar.MediatR.Caching
{
    public enum ExpirationType {
        AbsoluteExpiration,
        AbsoluteExpirationRelativeToNow,
        SlidingExpiration
    }
}