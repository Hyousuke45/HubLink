using System;
using Microsoft.Extensions.Configuration;

namespace HubLink.Configuration;

public static class ConfigurationLoader
{
    public static ApiSettings LoadApiSettings()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var settings = new ApiSettings();
        configuration.GetSection("HubLinkApi").Bind(settings);

        if (settings.TimeoutSeconds <= 0)
        {
            settings.TimeoutSeconds = 5;
        }

        return settings;
    }
}