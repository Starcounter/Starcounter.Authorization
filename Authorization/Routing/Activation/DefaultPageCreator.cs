using System;
using System.Linq;
using System.Reflection;

namespace Starcounter.Authorization.Routing.Activation
{
    /// <summary>
    /// Creates a router that will create pages using default constructor and call <see cref="PageContextSupport.HandleContext"/>
    /// Works only with pages that are <see cref="IResource"/> - this category includes <see cref="Json"/>, so covers most cases
    /// </summary>
    /// <param name="routingInfo"></param>
    /// <returns></returns>
    public class DefaultPageCreator : IPageCreator
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultPageCreator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public DefaultPageCreator()
        {
        }

        public Response Create(RoutingInfo routingInfo)
        {
            try
            {
                var page = Activator.CreateInstance(routingInfo.SelectedPageType);
                if (_serviceProvider != null && page is IInitPageWithDependencies pageWithDependencies)
                {
                    var init = routingInfo.SelectedPageType.GetMethod("Init", BindingFlags.Instance|BindingFlags.Public);
                    var arguments = init.GetParameters().Select(GetParamValue).ToArray();
                    init.Invoke(pageWithDependencies, arguments);
                }
                if (page is IInitPage initPage)
                {
                    initPage.Init();
                }
                PageContextSupport.HandleContext(page, routingInfo.Context);
                return new Response() { Resource = (IResource)page };
            }
            catch (Exception e)
            {
                throw new Exception($"Could not create page of type '{routingInfo.SelectedPageType}'", e);
            }
        }

        private object GetParamValue(ParameterInfo parameter)
        {
            try
            {
                var paramValue = _serviceProvider.GetService(parameter.ParameterType);
                if (paramValue == null)
                {
                    throw new Exception($"Could not instantiate required service of type '{parameter.ParameterType}'");
                }

                return paramValue;
            }
            catch (Exception e)
            {
                throw new Exception($"Could not instantiate param '{parameter}'", e);
            }
        }
    }
}