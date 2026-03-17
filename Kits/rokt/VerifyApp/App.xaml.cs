using mParticle.MAUI;

namespace VerifyApp;

public partial class App : Application
{
    // Set ROKT_API_KEY_IOS, ROKT_SECRET_KEY_IOS (or _ANDROID) env vars for local testing
    private static string RoktApiKey_iOS => Environment.GetEnvironmentVariable("ROKT_API_KEY_IOS") ?? "ENTER_YOUR_ROKT_API_KEY";
    private static string RoktSecretKey_iOS => Environment.GetEnvironmentVariable("ROKT_SECRET_KEY_IOS") ?? "ENTER_YOUR_ROKT_SECRET_KEY";

    private static string RoktApiKey_Android => Environment.GetEnvironmentVariable("ROKT_API_KEY_ANDROID") ?? "ENTER_YOUR_ROKT_API_KEY";
    private static string RoktSecretKey_Android => Environment.GetEnvironmentVariable("ROKT_SECRET_KEY_ANDROID") ?? "ENTER_YOUR_ROKT_SECRET_KEY";

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
                { UserIdentity.Email, "test@example.com" }
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
