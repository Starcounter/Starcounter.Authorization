# Starcounter.Authorization

This library helps implement authorization in Starcounter apps. It is built on top of ASP.NET Core Authorization feature. This readme explains features of this library, but also some basic features of ASP.NET Core Authorization.

## Table of contents

- [Getting started](#getting-started)
- [Securing view-models](#securing-view-models)
- [Available services](#available-services)
  * [IAuthenticationTicketService<UserSession>](#iauthenticationticketservice-usersession)
  * [ICurrentUserProvider<TUser>](#icurrentuserprovider-tuser)
  * [ISignOutService](#isignoutservice)
  * [IAuthorizationEnforcement](#iauthorizationenforcement)
- [What happens when permissions are denied?](#what-happens-when-permissions-are-denied)

<small><i><a href='http://ecotrust-canada.github.io/markdown-toc/'>Table of contents generated with markdown-toc</a></i></small>

## Getting started

1. If your app doesn't use [`Starcounter.Startup`](https://github.com/Starcounter/Starcounter.Startup) already, start using it. It's possible to use `Starcounter.Authorization` without it, but this topic is not covered here
1. Install `Starcounter.Authorization` package in your app
1. Copy classes from the [Example directory](ExampleDataModel/) into your project, remember to change namespace to your own
1. In your `Startup` class, in the `ConfigureServices` method, add the following line:

```c#
services.AddStarcounterAuthorization<AuthorizationSettings, UserSession, TicketToSession, User>()
```

1. If your project won't build because of TODO weaver error, do the following:
  1. Add `<DisallowDatabaseClasses>True</DisallowDatabaseClasses>` to `<PropertyGroup>` section of your app's csproj
  1. Add references to `Starcounter.QueryProcessor` and `Starcounter.BindingInterfaces` to your app's project (References -> Add -> Assemblies -> Extensions)
  1. Add a new "Starcounter Class Library" project to your solution, move all of your database classes there and reference it from your app's project

If you want, you can now alter the `User` and `UserSession` classes to suit your needs.

## Securing view-models

To prevent unauthorized access to your view-models, annotate them with `[Authorize]`, like so:

```c#
[Authorize]
public partial class DogsViewModel: Json
```

This has two effects:
* A [middleware](https://github.com/Starcounter/Starcounter.Startup#middleware) prevents this view-model from loading if the current user is not signed in
* An [input handler](https://docs.starcounter.io/topic-guides/typed-json/code-behind#handling-input-events) is generated for every property of this view-model, checking if the user is signed in. Existing input handlers are wrapped in this checkers. What happens if the user is not signed in is [configurable](#what-happens-when-permissions-are-denied)

In summary, only signed in users can interact with this view-model.


## Available services

This library registers some services in the DI container.

### IAuthenticationTicketService<UserSession>

The only method of this service intended to be used by general applications is `GetCurrentAuthenticationTicket`. It returns the current authentication ticket as is foreshadowed by its name. The ticket will be available even if the user is not signed in, and it resembles a session in other Web Frameworks. 

### ICurrentUserProvider<TUser>

Its sole method returns the current user or `null` if none is signed in.

### ISignOutService

Its sole method signs out the current user by removing the current authentication ticket. The current page should be reloaded afterwards, or some features that expect an authentication ticket might not work properly.

### IAuthorizationEnforcement

Its methods allow checking if the current user meets certain [policies or requirements](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-2.1).


## What happens when permissions are denied?

There are two situations in which an unauthorized action may be prevented by this library. What happens then can be overriden by [configuring](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-2.1#configure-simple-options-with-a-delegate) `SecurityMiddlewareOptions`.

When the user first loads a page which they aren't authorized to access, the view-model is not created. Instead a `404` response is returned. To override this behavior change the `UnauthenticatedResponseCreator` property.

```c#
services.Configure<SecurityMiddlewareOptions>(options => options.WithUnauthenticatedResponseCreator(uri => new DenyPage()));
```

The code above results in `DenyPage` being sent instead of the default `404` response. `uri` parameter in the lambda is the original URI, relative to the application root.

When the user interacts with the view-model in a way that is not permitted by the library, the offending patch is not applied to the view-model and the original input handler is not invoked. Instead an exception is thrown, destroying the Palindrom session. To override this behavior change the `CheckDeniedHandler` property.

```c#
services.Configure<SecurityMiddlewareOptions>(options => options.WithCheckDeniedHandler((type, data, viewModel) => Expression.Throw(Expression.New(typeof(Exception)))));
```

The code above will result in `new Exception()` being thrown on offence. The lambda passed is executed during code generation to determine action to be taken on offence. It uses [Expression Trees API](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/). `type` parameter is the view-model type for which the handler is generated, `data` is the expression to access `Data` property of the view-model and `viewModel` is the expression to access the view-model itself.
