# Exploring Services

Services are an integral part of any .NetCore application.  They provide a convenient way to implement *Inversion of Control* and *Abstraction* god practices.

.NetCore defines the functionality of the Service Container in the `IServiceProvider` interface and a basic out-of-the-box implementation in `ServiceProvider`.  There are other providers, but most don't use them: the basic implementation suffices.

Many .NetCore application templates configure the `IServiceCollection` and build the `IServiceProvider` as part of the startup.  The `WebApplication` builder looks like this:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();
```

Within `Program` you can get the provider like this:

```csharp
IServiceProvider serviceProvider = app.Services;
```

Note that there's no simple way to access the the `IServiceProvider` from normal code. The temptation is to do something like this:


```csharp
public record ServiceProviderUtilities
{
    private readonly IServiceProvider _serviceProvider;

    private ServiceProviderUtilities(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static ServiceProviderUtilities? instance;

    public static ServiceProviderUtilities GetInstance()
    {
        ArgumentNullException.ThrowIfNull(instance);
        return instance;
    }

    public static void SetInstance(IServiceProvider serviceProvider)
    {
        if (instance != null)
            throw new Exception("An instance already exists");

        instance = new ServiceProviderUtilities(serviceProvider);
    }
}
```

**DON'T** unless you really know what you're doing.  You'll misuse it and create memory leaks. I'll explain why shortly.

## Building a Provider

It's simple to build and use a container.  

Add a console application.

Some simple service classes:

```csharp
public class DisposableSingletonService : IDisposable
{
    public readonly Guid ServiceId = Guid.NewGuid();
    public DisposableSingletonService()
        => Console.WriteLine($"Disposable Singleton Service Created => {ServiceId.ToString().Substring(0,4)}.");

    public void Dispose()
        => Console.WriteLine("Disposable Singleton Service Disposed.");
}
```
```csharp
public class DisposableScopedService : IDisposable
{
    public readonly Guid ServiceId = Guid.NewGuid();
    public DisposableScopedService()
        => Console.WriteLine($"Disposable Scoped Service Created => {ServiceId.ToString().Substring(0, 4)}.");
    public void Dispose()
        => Console.WriteLine("Disposable Scoped Service Disposed.");
}
```
```csharp
public class TransientService { }
```
```csharp
public class DisposableTransientService : IDisposable
{
    public DisposableTransientService()
        => Console.WriteLine($"Disposable Transient Service Created => {ServiceId.ToString().Substring(0, 4)}.");
    public readonly Guid ServiceId = Guid.NewGuid();
    public void Dispose()
        => Console.WriteLine("Disposable Transient Service Disposed.");
}
```

You can now build a DI container in `Program`:

```csharp
var services = new ServiceCollection();

services.AddSingleton<DisposableSingletonService>();
services.AddScoped<DisposableScopedService>();
services.AddTransient<DisposableTransientService>();

var serviceProvider = services.BuildServiceProvider();
```

And run the following code:

```csharp
var singletonService = serviceProvider.GetRequiredService<DisposableSingletonService>();
var scopedService = serviceProvider.GetRequiredService<DisposableScopedService>();

var transientService1 = serviceProvider.GetRequiredService<DisposableTransientService>();
var transientService2 = serviceProvider.GetRequiredService<DisposableTransientService>();

// release our one reference
transientService1 = null;

Console.ReadLine();

serviceProvider.Dispose();
serviceProvider = null;

Console.ReadLine();
```

The result is:

```text
Disposable Singleton Service Created => 5ba4.
Disposable Scoped Service Created => 981f.
Disposable Transient Service Created => 5252.
Disposable Transient Service Created => 110e.
  > Wait here
Disposable Transient Service Disposed => 110e.
Disposable Transient Service Disposed => 5252.
Disposable Scoped Service Disposed => 981f.
Disposable Singleton Service Disposed => 5ba4.
  > Wait here
```

All the services are disposed when the DI container is disposed.

## Root Service Provider

Check the methods are available on the `IServerProvider`.  There's lots of `GetService` methods that I'll look at shortly.  The only mention of `Scoped` is `CreateScope` and `CreateAsyncScope`.

When we created the `IServiceProvider` above, we created a root service provider.  Within this container, *Singleton* and *Scoped* services have the same scope: the lifetime of the container.

To use scoped services we need to create a `Scoped` container.

Modify the ServiceProvider code to create two IServiceProviders.

```csharp
var services = new ServiceCollection();
services.AddSingleton<DisposableSingletonService>();
services.AddScoped<DisposableScopedService>();
services.AddTransient<DisposableTransientService>();

var rootProvider = services.BuildServiceProvider();

var serviceScope =rootProvider.CreateScope();
var serviceProvider = serviceScope.ServiceProvider;
```

`serviceScope` is created within the root provider:  it's a child provider.

We can test the singleton:

```csharp
var singletonService1 = rootProvider.GetRequiredService<DisposableSingletonService>();
Console.WriteLine($"Singleton Service ID:{singletonService1.ServiceId.ToString().Substring(4)}");

var singletonService2 = serviceProvider.GetRequiredService<DisposableSingletonService>();
Console.WriteLine($"Singleton Service ID:{singletonService2.ServiceId.ToString().Substring(4)}");
```

And the result is:

```text
Disposable Singleton Service Created => 4473.
Singleton Service ID:4473
Singleton Service ID:4473
```

Both providers return the same instance of the service.  Singletons instances are held by the root provider.  The scoped provider delegates responsibility singletons to the root.  It creates and manages *Scoped* and *Transient* services.

We can test scoped Services:

```csharp
var scopedService1 = rootProvider.GetRequiredService<DisposableScopedService>();
Console.WriteLine($"Singleton Service ID:{scopedService1.ServiceId.ToString().Substring(0, 4)}");

var ScopedService2 = serviceProvider.GetRequiredService<DisposableScopedService>();
Console.WriteLine($"Singleton Service ID:{ScopedService2.ServiceId.ToString().Substring(0, 4)}");

serviceScope.Dispose();
serviceScope = null;
```

And the result is:

```text
Disposable Scoped Service Created => 8134.
Singleton Service ID:8134
Disposable Scoped Service Created => c1ee.
Singleton Service ID:c1ee
Disposable Scoped Service Disposed => c1ee.
```

Two services are created and when dispose is called on the scoped provider, the scoped service within the provider is disposed.

This distinction is key.  *Scoped* services should only be requested from *Scoped* providers.  You should **NEVER** request *Scoped* services from the root service provider.  Which is what you are very likely to do if you get an instance of the root service provider through the *singleton* pattern above.

.NetCore applications are designed to build and provide *Scoped* providers in different contexts.  

 - In a `HttpRequest` context, the HttpRequest context creates a Scoped Provider which it injects into the HttpRequest based objects such as controllers, minmial API's and Razor.

 - In Blazor Server the SPA Hub context creates a scoped Provider the Render uses to inject services into components.


## Manual Injection

While injecting via the constructor is the most common way of injecting services, it's not the only way.

`IServiceProvider` has a set of methods to resolve services manually from the container.

The two most convenient are:

```csharp
TService service = IServiceProvider.GetRequiredService<TService>();
```

And: 

```csharp
TService? service = IServiceProvider.GetService<TService>();
```

The important difference is `GetRequiredService` throws an exception if the service doesn't exist, while `GetService` returns a `null`.

There are use cases where a service is optional.  You use a notification service if one is registered, but it's not critical to the application.

It is argued that using manual injection is a *Service Locator Pattern* and thus an *anti-pattern* to be avoided.

The principle arguments are that:
1. It hides the service dependancies.
2. It moves exceptions from compile-time to run-time.
3. It makes testing more difficult.

All true, but they can be applied to many pieces of code.  They're not show stoppers.  

Consider this service:

```csharp
public class BadService 
{
    private readonly IServiceProvider _serviceProvider;

    public BadService(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;     

    public void DoSomething()
    {
        var service = _serviceProvider.GetRequiredService<DisposableScopedService>();
        // Do something with service
    }
}
```

This highlights the *Service Locator* problem.  The service acquisition is now buried down in the code.  The exception is only raised when the method is called.  This is **BAD PRACTICE**.

On the other hand, consider this service:

```csharp
public class GoodService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DisposableScopedService? _scopedService;

    public GoodService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _scopedService = _serviceProvider.GetService<DisposableScopedService>();

        if (_scopedService is not null)
        {
            // Do some service registration
        }
    }

    public void DoSomething()
    {
        if (_scopedService is not null)
        {
            // Do something with service
        }
    }
}
```

The service acquisition occurs in the constructor.  There's nothing wrong with this pattern.

### Blazor Component Injection

Consider a Blazor component.  Services are injected using the [Inject] attribute like this:

```csharp
@inject WeatherForecastService Service
```

They are not declared in the constructor.

```csharp
public Weather() :base()
{
    // can't use WeatherForecastService here   
}
```

So how?

The Blazor Hub Sesssion [whether running on the Server or the Web Browser] has a Session scoped IServiceProvider.  Once a component has been initialized, the Renderer locates the `Inject` attribute properties in the component, and sets those properties to the appropriate service from the Hub service container.

There are two key points to note:
1. The hub session provides the scoped ServiceProvider and disposes it when the hub session goes out-of-scope.
2. Injection is an implementation of the *Service Locator Pattern* anti-pattern.