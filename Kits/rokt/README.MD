# mParticle MAUI SDK

This is the repository of the mParticle Maui Rokt Kit, which is compatible with iOS and Android apps.

## Installation

This SDK is distributed via the public NuGet gallery: https://www.nuget.org/packages/mParticle.MAUI.Kits.Rokt/.

To use the Rokt kit you need to ensure you have both mParticle and the Rokt kit.

```
dotnet add package mParticle.MAUI
dotnet add package mParticle.MAUI.Kits.Rokt
```

### Initialization and registration

The SDK must be initialized with your mParticle workspace key and secret prior to use. This call should be placed in your MAUI application initialization, *as early as possible*:

```cs
using mParticle.MAUI;

namespace MyProject
{
    public class Example
    {
        void Example ()
        {
            string key = "";
            string secret = "";
            #if __ANDROID__
                key = "ENTER_YOUR_API_KEY";
                secret = "ENTER_YOUR_API_SECRET";
            #elif __IOS__
                key = "ENTER_YOUR_API_KEY";
                secret = "ENTER_YOUR_API_SECRET";
            #endif

            var options = new MParticleOptions()
            {
                ApiKey = key,
                ApiSecret = secret
            }

            // Register the Rokt kit with mParticle before initialization
            RoktKit.Register();

            // Initialize mParticle
            MParticle.Instance.Initialize(options);
        }
    }
}
```

In your app class you'll also need to configure Maui handlers for the RoktEmbeddedView.

```cs
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler(typeof(RoktEmbeddedView), typeof(RoktEmbeddedViewHandler));
            });

        return builder.Build();
    }
}
```

For Android you also need to ensure your activty extends `MauiAppCompatActivity`
