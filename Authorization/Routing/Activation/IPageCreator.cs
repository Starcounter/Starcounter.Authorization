namespace Starcounter.Authorization.Routing.Activation
{
    public interface IPageCreator
    {
        Response Create(RoutingInfo routingInfo);
    }
}