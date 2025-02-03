using System;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Services;

public interface IServiceLocator
{
    public T? GetService<T>();
    public T GetRequiredService<T>() where T : notnull;
}

public class ServiceCollectionLocator(IServiceProvider services) : IServiceLocator
{
    public T? GetService<T>()
    {
        return services.GetService<T>();
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return services.GetRequiredService<T>();
    }
}