using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Client.ViewModels;
using Client.Views;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Client;

public partial class App : Application
{
    private IHost? _host;
    
    public IHost GlobalHost => _host ?? throw new InvalidOperationException("The application has not been initialized.");
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        try
        {
            _host = CreateHostBuilder().Build();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = GlobalHost.Services.GetRequiredService<MainWindowViewModel>(),
                };
                
                desktop.Exit += async (sender, args) =>
                {
                    await GlobalHost.StopAsync();
                    GlobalHost.Dispose();
                    _host = null;
                };
            }

            DataTemplates.Add(GlobalHost.Services.GetRequiredService<ViewLocator>());
            
            var dbContext = GlobalHost.Services.GetRequiredService<DataContext>();
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }
            
            base.OnFrameworkInitializationCompleted();
            await GlobalHost.StartAsync();
        }
        catch (Exception e)
        { 
            if (_host is {})
                GlobalHost.Services.GetRequiredService<ILogger<App>>().LogCritical(e, "Failed to start the application.");
            throw;
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    
    private IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder(Environment.GetCommandLineArgs())
            .ConfigureServices((context, service) =>
            {
                service
                    .AddTransient<ViewLocator>()
                    .AddTransient<MainWindowViewModel>()
                    .AddView<CodeToFormViewModel, CodeToForm>();

                service.AddDbContext<DataContext>(cfg =>
                {
                    cfg.UseSqlite("Data Source=data.db");
                });
            });

    }
}

