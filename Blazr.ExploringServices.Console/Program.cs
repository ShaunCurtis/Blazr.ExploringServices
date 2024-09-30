using Blazr.ExploringServices;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

var services = new ServiceCollection();
services.AddSingleton<DisposableSingletonService>();
services.AddScoped<DisposableScopedService>();
services.AddTransient<DisposableTransientService>();

var rootProvider = services.BuildServiceProvider();

var serviceScope =rootProvider.CreateScope();
var serviceProvider = serviceScope.ServiceProvider;

var singletonService1 = rootProvider.GetRequiredService<DisposableSingletonService>();
Console.WriteLine($"Singleton Service ID:{singletonService1.ServiceId.ToString().Substring(0, 4)}");

var singletonService2 = serviceProvider.GetRequiredService<DisposableSingletonService>();
Console.WriteLine($"Singleton Service ID:{singletonService2.ServiceId.ToString().Substring(0,4)}");

var scopedService1 = rootProvider.GetRequiredService<DisposableScopedService>();
Console.WriteLine($"Singleton Service ID:{scopedService1.ServiceId.ToString().Substring(0, 4)}");

var ScopedService2 = serviceProvider.GetRequiredService<DisposableScopedService>();
Console.WriteLine($"Singleton Service ID:{ScopedService2.ServiceId.ToString().Substring(0, 4)}");

serviceScope.Dispose();
serviceScope = null;

var scopedService = serviceProvider.GetRequiredService<DisposableScopedService>();

//var transient = serviceProvider.GetService<TransientService>();

//if (transient is null)
//    Console.WriteLine("Transient is null!");

var transientService1 = serviceProvider.GetRequiredService<DisposableTransientService>();
var transientService2 = serviceProvider.GetRequiredService<DisposableTransientService>();

// release our one reference
transientService1 = null;

Console.ReadLine();

serviceScope.Dispose();
serviceScope = null;

Console.ReadLine();

