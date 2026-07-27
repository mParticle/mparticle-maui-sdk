using System;
using System.Collections.Generic;
using mParticle.MAUI.Rokt;
#if __IOS__
using System.Linq;
using Foundation;
using CoreBinding = global::mParticle.MAUI.iOSBinding;
#endif

namespace mParticle.MAUI.Rokt.Payments;

/// <summary>
/// Adds Shoppable Ads support to the Rokt API. This capability only exists when the
/// <c>mParticle.Maui.Kits.Rokt.Payments</c> package is referenced, because it depends on a
/// natively registered Rokt payment extension (see <see cref="RoktPaymentExtension"/>).
/// </summary>
public static class RoktShoppableAdsExtensions
{
    /// <summary>
    /// Displays a Shoppable Ads overlay placement.
    /// </summary>
    /// <remarks>
    /// Implemented on iOS (requires iOS 15+ and a payment extension registered via
    /// <see cref="RoktPaymentExtension.Register(string, string)"/>). On Android this is a
    /// no-op until native support lands.
    /// </remarks>
    /// <param name="rokt">The Rokt API instance.</param>
    /// <param name="identifier">The view name / placement identifier.</param>
    /// <param name="attributes">Optional attributes for targeting.</param>
    /// <param name="config">Optional display configuration (color mode, caching).</param>
    public static void SelectShoppableAds(
        this RoktApi rokt,
        string identifier,
        Dictionary<string, string> attributes = null,
        RoktConfig config = null)
    {
        // Route through the receiver's native handle so behavior matches the sibling
        // RoktApi methods: a null handle means the SDK is not initialized (NoOp receiver).
        var handle = rokt?.NativeHandle;
        if (handle == null)
        {
            Console.WriteLine(RoktApi.SdkNotInitializedWarning);
            return;
        }
#if __IOS__
        if (handle is not CoreBinding.MPRokt native)
        {
            Console.WriteLine(RoktApi.SdkNotInitializedWarning);
            return;
        }

        var nsAttributes = ConvertToNSDictionary(attributes) ?? new NSDictionary<NSString, NSString>();
        var nsConfig = ConvertToMpRoktConfig(config);

        native.SelectShoppableAds(identifier, nsAttributes, nsConfig, null);
#else
        Console.WriteLine("[mParticle MAUI SDK] SelectShoppableAds is not yet supported on Android.");
#endif
    }

#if __IOS__
    private static CoreBinding.RoktConfig ConvertToMpRoktConfig(RoktConfig config)
    {
        if (config == null)
            return null;

        var builder = new CoreBinding.RoktConfigBuilder()
            .ColorMode(ConvertToMpRoktColorMode(config.ColorMode));

        if (config.CacheDuration.HasValue || (config.CacheAttributes != null && config.CacheAttributes.Any()))
        {
            var cacheDuration = config.CacheDuration.HasValue
                ? config.CacheDuration.Value
                : CoreBinding.RoktCacheConfig.MaxCacheDuration;
            var cacheAttributes = ConvertToNSDictionary(config.CacheAttributes);
            var cacheConfig = new CoreBinding.RoktCacheConfig(cacheDuration, cacheAttributes);
            builder = builder.CacheConfig(cacheConfig);
        }

        return builder.Build();
    }

    private static CoreBinding.RoktColorMode ConvertToMpRoktColorMode(RoktColorMode colorMode)
    {
        switch (colorMode)
        {
            case RoktColorMode.Light:
                return CoreBinding.RoktColorMode.Light;
            case RoktColorMode.Dark:
                return CoreBinding.RoktColorMode.Dark;
            case RoktColorMode.System:
            default:
                return CoreBinding.RoktColorMode.System;
        }
    }

    private static NSDictionary<NSString, NSString> ConvertToNSDictionary(Dictionary<string, string> dictionary)
    {
        if (dictionary == null || !dictionary.Any())
            return new NSDictionary<NSString, NSString>();

        return NSDictionary<NSString, NSString>.FromObjectsAndKeys(dictionary.Values.ToArray(), dictionary.Keys.ToArray());
    }
#endif
}
