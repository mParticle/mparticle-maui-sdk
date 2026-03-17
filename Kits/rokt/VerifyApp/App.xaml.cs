using mParticle.MAUI;

namespace VerifyApp;

public partial class App : Application
{
    private const string RoktApiKey_iOS = "us2-7de1f53116f9c54797128476b5f8e896";
    private const string RoktSecretKey_iOS = "q4lUWRRhuwN0DGKIoSoYL6uPSKE5EW-RpFfYPjZyuHigeKc2J2REs2j_gnb_vZ1q";

    private const string RoktApiKey_Android = "us2-29af8ce23b202f4dbd6f7d0da2b846c9";
    private const string RoktSecretKey_Android = "23NRDsJLnhz6bDwFTmh0ZJPNeG_vSr8UAaYKZ6UJotHtmFHTPH-Qws2CLk7WgOio";

    public App()
    {
        InitializeComponent();

        // Initialize mParticle + Rokt at app startup (like partner TestMaui/Cinemark)
        // This triggers the same code path as production - catches NuGet package issues
        InitializeMParticleRokt();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    private void InitializeMParticleRokt()
    {
        try
        {
            string apiKey = string.Empty;
            string apiSecret = string.Empty;

            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                apiKey = RoktApiKey_Android;
                apiSecret = RoktSecretKey_Android;
            }
            else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                apiKey = RoktApiKey_iOS;
                apiSecret = RoktSecretKey_iOS;
            }

            var options = new MParticleOptions()
            {
                ApiKey = apiKey,
                ApiSecret = apiSecret
            };

            options.Environment = mParticle.MAUI.Environment.Development;

            var identifyRequest = new IdentityApiRequest();
            identifyRequest.UserIdentities = new Dictionary<UserIdentity, string>()
            {
                { UserIdentity.Email, "dwest@cinemark.com" }
            };
            options.IdentifyRequest = identifyRequest;

            options.UnCaughtExceptionLogging = true;
            options.LogLevel = mParticle.MAUI.LogLevel.DEBUG;

            // Register the Rokt kit before initialization
            RoktKit.Register();

            MParticle.Instance.Initialize(options);

            Console.WriteLine("mParticle + Rokt SDK initialized successfully at app startup.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"mParticle + Rokt initialization failed: {ex.Message}");
        }
    }
}
