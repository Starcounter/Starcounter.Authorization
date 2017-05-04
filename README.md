Table of contents
=================
* [How to start](#how-to-start)
* [Authorization Library](#authorization-library)
* [Routing, Middleware and Context - Concepts](#routing-middleware-and-context---concepts)
  * [Page](#page)
  * [RoutingInfo](#routinginfo)
  * [Router](#router)
  * [Middleware](#middleware)
  * [Context](#context)
* [Foundation](#foundation)
  * [Permissions](#permissions)
  * [Define authorization rules](#define-authorization-rules)
  * [Check the rules](#check-the-rules)
* [Automatic checking of permissions - SecurityMiddleware](#automatic-checking-of-permissions---securitymiddleware)
  * [Prerequisite: Router](#prerequisite-router)
  * [Restricting access to a page](#restricting-access-to-a-page)
  * [Contextual permissions](#contextual-permissions)

# How to start
Recommended quick start configuration to play around with this library is to use sample [apps provided by Starcounter](https://github.com/StarcounterApps):

1. [Launcher](https://github.com/StarcounterApps/Launcher) - provides a common UI frame (menu)
2. [UserAdmin](https://github.com/StarcounterApps/UserAdmin) - allows to create system users
3. [SignIn](https://github.com/StarcounterApps/SignIn) - authenticates a user with a username and a password
4. [People](https://github.com/StarcounterApps/People) - gives a way to organize people into groups that may be used with this library

App set above is the sample that allows dealing with users and groups in Simplified data model provided with Starcounter. Thanks to that you will be able to apply instructions above and build a working solution.

# Authorization Library

This library helps the developer to prevent the user from accessing / acting on data he doesn't have privileges to.

[It's available on nuget as Starcounter.Authorization](https://www.nuget.org/packages/Starcounter.Authorization)

# Routing, Middleware and Context - Concepts

It is useful to understand these concepts before moving on to authorization.

### Page

Page is a class that is responsible for handling a request. It's usually a Json view-model (note that `Json` class defines an implicit conversion to `Response`). It should be annotated with `UrlAttribute` to associate it with a specific URL:

```cs
[Url("/ChecklistDesigner/checklists/{?}")]
partial class ChecklistPage : IBound<Checklist>
```

### RoutingInfo

Object of `RoutingInfo` class contains all the necessary information about the current request:
* `SelectedPageType` (`Type`) - the type associated with this request. Middleware can use this to decide weather it should act on the request or not (e.g. check if the type is annotated with specific attribute). The pageCreator should use this to construct the actual Page
* `Request` (`Request`) - the original Starcounter request. Can be used to retrieve headers or raw request body
* `Arguments` (`string[]`) - the array of URL arguments (that go into "{?}" slots). Usually used by `ContextMiddleware` to create the Context
* `Context` (`object`) - it has separate section later. Remains null until some middleware (usually `ContextMiddleware`) sets it. 

### Router

Router is a mechanism (in form of a class `Router`) that is responsible for accepting HTTP requests, choosing appropriate Page type for them and feeding the requests into the Page to retrieve response. Usually router needs to be configured by its constructor:

```cs
public Router(Func<RoutingInfo, Response> pageCreator)
```

`pageCreator` is a function that should create a response and sets the Context in it if necessary. Usually it just instantiates an object of `RoutingInfo.SelectedPageType` and calls `PageContextSupport.HandleContext`. If you create the router via `Router.CreateDefault()` it will always create the page using the default constructor and call `PageContextSupport.HandleContext`. You can override this method to create the Pages differently, e.g. using Dependency Injection

### Middleware

Middleware is a mechanism to transform the request or the response before or after the page creation. Middleware has to implement `IPageMiddleware` interface and has to be registered with Router with `Router.AddMiddleware`. `IPageMiddleware` defines one method:
```cs
Response Run(RoutingInfo routingInfo, Func<Response> next);
```

It is run on every request handled by the Router. The `next` function runs pageCreator and all the middleware that were registered after this one and returns the resulting response. If you want to add behavior before handling the response, just add it before call to next. To add behavior after handling the response, add it after the call to next:

```cs
Response Run(RoutingInfo routingInfo, Func<Response> next)
{
  Logger.LogRequestStart(routingInfo);
  Response originalResponse = next();
  Logger.LogRequestEnd(routingInfo);
  return originalResponse;
}
```

If you want to prevent the original handling from happening, just don't call next and return an alternative response:

```cs
Response Run(RoutingInfo routingInfo, Func<Response> next)
{
  if(IsAuthorized(routingInfo)))
  {
    return next();
  }
  else
  {
    return Response.FromStatusCode(403);
  }
}
```

Middleware usually decide weather (and how) to act on the request basing on the `RoutingInfo.SelectedPageType` and its associated attributes.

### Context

Context is the data represented in URL arguments. Page `PersonPage` associated with URL `/people/person/{id}` would probably have Context of type `Person` (and would probably be `IBound<Person>`). If the type of the Page is `IBound<T>`, the Context type will be inferred to `T`. You can also explicitly specify the Context type by implementing the `IPageContext<T>` interface.

Context was introduced to let the middleware reason about the object(s) associated with the Page before the Page is constructed. If the Page is `IBound<T>` we can set its `Data` property to T and then act on it (e.g. decide weather the user is authorized to interact with the Page). But before the page is constructed (when majority of Middleware is run) the Context can't be retrieved from the Page object, since there is no Page object yet. That's why the `Context` property of the `RoutingInfo` has been introduced.

It's usually the best to register the `ContextMiddleware`, since it will automatically set the `RoutingInfo.Context` property if the Context type is a database type (which is almost always true). If you want to set the Context your way, just create and register your own Middleware that does it.

If you want to customise the the Context type, way that is created or handled for your page, follow this example:

```cs
[Url("/items/{0}/{1}")]
public partial class ItemPage: IPageContext<Tuple<Item, SubItem>>, IBound<SubItem>
//                             ^^-- this way you override the Context type, which would be inferred to SubItem
{
    private Item _item;
    
    [UriToContext]
    // this attribute marks the method that will be used to create the Context
    public static Tuple<Item, SubItem> UriToContext(string[] args)
    // its return type matches the Context type. It accepts arguments from URL
    {
        return Tuple.Create(Items.ById(args[0]), SubItems.ById(args[1]));
    }
    
    public void HandleContext(Tuple<Item, SubItem> context)
    // this method is defined in the IPageContext interface. It is invoked after the Page is created
    {
        this._item = context.Item;
        this.Data = context.SubItem;
    }
}
```

Middleware that are registered after the `ContextMiddleware` (or any other that sets the Context) can use Context to reason about the request. For example the `SecurityMiddleware` uses Context to check if the user is authorized to access the page.

If you specify you own pageCreator (the Func that you pass to `Router` constructor) you should also handle the Context somehow. This usually means setting `Data` property for `IBound` pages and if you don't need your own custom behavior you can just call `PageContextSupport.HandleContext` to do that for you.


## Foundation

### Permissions

Permission is a core concept in Authorization. A Permission represents specific action that we can allow the user to perform. This could be "listing all users" or "displaying details of user Joozek" or "changing the details of user Joozek". The granularity of Permissions (you could have just one "changing the details of user X" or specific Permission for each property of the user) is up to the developer and it's recommended to limit the scope of permissions to an object (so "change user X" is probably better then "change any user" and "change last name of user X").

Whenever a user will perform an action, like opening the page, clicking a button, a Permission will be checked to verify if he is allowed to do it. Grouping Permissions together ("show list of users or companies") is discouraged, because it's easy to group them later in the configuration  with AuthorizationRules.

Let's define some example database classes that will be useful in this guide:

```cs
[Database]
public class Invoice : Something
{
    public bool IsSettled;
}

[Database]
public class InvoiceRow : Something
{
    public string Product;
    public decimal Price;
}

[Database]
public class InvoiceInvoiceRow : Relation
{
    [SynonymousTo(nameof(From))]
    public Invoice Invoice;
    [SynonymousTo(nameof(To))]
    public InvoiceRow InvoiceRow;
}
```

And now, let's define permissions that we'll use to describe the authorization rules later:

```cs
public class ListInvoices: Permission
{
}

public class DisplayInvoice: Permission
{
    public Invoice Invoice {get; private set;}
    public DisplayInvoice(Invoice invoice)
{

this.Invoice = invoice;
}
}
```

Permissions can be anything as long as they derive from `Permission`, but some parts of this library work best if their constructors accept only serializables and/or database types.

### Define authorization rules

Now let's define rules that will decide whether the user will be granted the permissions or not.

```cs
var rules = new AuthorizationRules();
rules.AddRule(new ClaimRule<ListInvoices, SystemUserClaim>((claim, permission) => claim.SystemUser.Name == "admin"));
```

Rules implement `IAuthorizationRule<TPermission>` interface, which defines one method:

```cs
public bool Evaluate(
IEnumerable<Claim> claims, // each Claim represents a fact about the current user. Most popular ones are
// SystemUserClaim and PersonClaim
IAuthorizationEnforcement authorizationEnforcement, // this can be used to check if the current user has other permissions
TPermission permission) // the actual permission to check
```

`SystemUserClaim` checks whether a claim of specified type (SystemUserClaim) exists and matches the provided predicate. Here, we effectively check if current user has a name "admin".

Claims are a way obtain information about the current user without direct dependency on authentication system. There are two built in claims:
* SystemUserClaim - if present it means that a current user is signed in as a specific SystemUser
* PersonClaim - if present it means that a current user represents specific person. It is preferred mean of establishing user's identity, since it's usually more convenient to manage People than Users

There are other built-in rules in `Starcounter.Authorization.Core.Rules` namespace. Feel free to use them or create your own.

### Check the rules

`AuthorizationEnforcement` is a class responsible for checking the if the current user has a specific permission with regard to a rule set. Let's create it using the rules we defined in previous steps:

```cs
var enforcement = AuthorizationEnforcement(rules, new SystemUserAuthentication());
```

The second argument passed is a authentication backend (a something that obtains the claims about a current user). It's usually enough to use the built in `SystemUserAuthentication`. It provides `SystemUserClaim` and `PersonClaim` for signed in users.

We can now use our new `IAuthorizationEnforcement` to check if we have a permission:

```cs
bool canWe = enforcement.CheckPermission(new ListInvoices()); // note that the outcome depends on whether we are currently signed in as "admin"
```

## Automatic checking of permissions - SecurityMiddleware

Manual checking of permissions is powerful, but usually we need something that checks the permissions for us. That's what SecurityMiddleware is doing - it's preventing users from accessing pages they don't have access to and from making interactions they are not allowed to do.

### Prerequisite: Router

`SecurityMiddleware` is a feature that depends on Router. If you don't know what it is - go ahead and check it out // `TODO` LINK

To enable `SecurityMiddleware` add it to a Router:

```cs
router.AddMiddleware(new SecurityMiddleware(
enforcement,
info => Response.FromStatusCode(403), // a function returning a Response to give instead of a restricted Page
PageSecurity.CreateThrowingDeniedHandler<Exception>())); // defines the behavior in case of forbidden interaction
```
`TODO` describe checkDeniedHandler

### Restricting access to a page

Primary mean of managing access to pages is the `RequiredPermissionAttribute`. Let's use it to mark secure our example page:

```cs
[Url("/invoices/invoices")]
[RequirePermission(typeof(ListInvoices))]
public partial class InvoicesPage : Json
{
// ...
}
```

After adding this attribute, every time a user opens the URL "/invoices/invoices", the Authorization library checks whether he has is granted a ListInvoices Permission. If he is not, then he is presented with 403 Forbidden page (or any other response configured in `SecurityMiddleware` constructor). What's more, every time a user tries to change a property on this page or runs a handler on this page, the same permission is checked. In case of failure, an Exception is thrown (this also can be customised using the `SecurityMiddleware` constructor).

If you want to check a different permission before allowing a user to execute a handler, you can add the `RequiredPermissionAttribute` to the handler itself:

```cs
[Url("/invoices/invoices")]
[RequirePermission(typeof(ListInvoices))]
public partial class InvoicesPage : Json
{
    [RequirePermission(typeof(AddInvoice))]
    // this overrides the required permission
    public void Handle(Input.AddInvoice)
{
// ...
}
}
```
This also works with subpages:
```cs
[Url("/invoices/invoices")]
[RequirePermission(typeof(ListInvoices))]
public partial class InvoicesPage : Json
{
    [RequirePermission(typeof(AddInvoice))]
    // this overrides the required permission
    [InvoicesPage_json.SomeDialog]
    public partial class DialogItem : Json
{
// ...
}
}
```

If you want to disable permission checking in some part (subpage / handler) of your Page you can just mark it as `[RequirePermission(null)]`

```cs
[Url("/invoices/invoices")]
[RequirePermission(typeof(ListInvoices))]
public partial class InvoicesPage : Json
{
    [RequirePermission(null)]
    // this overrides the required permission
    public void Handle(Input.Back)
{
// ...
}
}
```

### Contextual permissions

So far, we've only checked permissions that accept no arguments in their constructors. That's easy - the library creates the permission objects using their default constructors. Another popular case is when the permission pertains to a specific object - like this one:

```cs
public class DisplayInvoice: Permission
{
    public Invoice Invoice {get; private set;}
    public DisplayInvoice(Invoice invoice)
{
this.Invoice = invoice;
}
}
```

To check that kind of Permission automatically, the Page's Context type would need to match the Permission's constructor parameter type (`Invoice`). This usually just means that the page is `IBound<Invoice>`:

```cs
[Url("/invoices/invoices/{0}")]
[RequirePermission(typeof(DisplayInvoice))]
public partial class InvoicePage : Json, IBound<Invoice>
{
}
```

In above example, upon requesting "/invoices/invoices/Abc", the following would happen:

* Context would be retrieved - in this case an Invoice object with id "Abc"
* In case no such object exists - 404 error
* A Permission would be constructed and checked - in this case `DisplayInvoice` with context passed as a constructor argument
* In case the permission is refused - 403 error

note: Remember, that in order to have the Context retrieved automatically you need the `ContextMiddleware`

The permission to be checked could also be directly specified using the `CustomCheckClassAttribute` (name subject to discussion and change)):

```cs
[Url("/invoices/invoices/{0}")]
public partial class InvoicePage : Json, IBound<Invoice>
{
    [CustomCheckClass]
    public static Permission CreatePermissionToCheck(Invoice context) => new DisplayInvoice(context);
}
```

Method marked with this attribute should be public and static, return `Permission` and accept context as its argument. Whatever permission it returns is then checked to see if the user can access the page.
