using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Handlers;
using AndroidBinding = mParticle.MAUI.AndroidBinding;
using NativeRokt = Com.Mparticle.Mparticleroktbinding.MParticleRoktBinding;

namespace mParticle.MAUI.Rokt;

/// <summary>
/// Android implementation of the cross-platform <see cref="RoktApi"/>. It bridges to the
/// mParticle Android Rokt kit (mParticle Android SDK 6, <c>com.mparticle.kits</c>) through the
/// <c>com.mparticle.mparticleroktbinding</c> helper, keeping the Kotlin Flow / roktsdk types
/// out of the managed binding surface.
/// </summary>
internal sealed class AndroidRoktApi : RoktApi
{
    private readonly AndroidBinding.MParticle _mparticleInstance;

    // Hold event subscriptions/listeners so they are not garbage collected while active.
    private static readonly List<object> EventSubscriptions = new List<object>();

    internal AndroidRoktApi(AndroidBinding.MParticle mparticleInstance)
    {
        _mparticleInstance = mparticleInstance;
    }

    internal override object NativeHandle => _mparticleInstance;

    public override void SelectPlacements(
        string identifier,
        Dictionary<string, string> attributes = null,
        Dictionary<string, RoktEmbeddedView> embeddedViews = null,
        RoktConfig config = null)
    {
        if (_mparticleInstance == null)
        {
            Console.WriteLine(SdkNotInitializedWarning);
            return;
        }

        var nativeViews = ConvertEmbeddedViews(embeddedViews);

        string colorMode = null;
        long cacheDurationSeconds = 0;
        IDictionary<string, string> cacheAttributes = null;
        if (config != null)
        {
            colorMode = ConvertColorMode(config.ColorMode);
            cacheDurationSeconds = config.CacheDuration ?? 0;
            cacheAttributes = config.CacheAttributes;
        }

        NativeRokt.SelectPlacements(identifier, attributes, nativeViews, colorMode, cacheDurationSeconds, cacheAttributes);
    }

    private static string ConvertColorMode(RoktColorMode colorMode)
    {
        switch (colorMode)
        {
            case RoktColorMode.Light:
                return "LIGHT";
            case RoktColorMode.Dark:
                return "DARK";
            case RoktColorMode.System:
            default:
                return "SYSTEM";
        }
    }

    public override void Events(string identifier, Action<RoktEvent> onEvent)
    {
        if (_mparticleInstance == null)
        {
            Console.WriteLine(SdkNotInitializedWarning);
            return;
        }

        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("identifier cannot be null or empty.", nameof(identifier));
        }

        if (onEvent == null)
        {
            throw new ArgumentNullException(nameof(onEvent));
        }

        var listener = new RoktEventCallbackWrapper(onEvent);
        var subscription = NativeRokt.SubscribeToEvents(identifier, listener);

        lock (EventSubscriptions)
        {
            EventSubscriptions.Add(listener);
            if (subscription != null)
            {
                EventSubscriptions.Add(subscription);
            }
        }
    }

    public override void GlobalEvents(Action<RoktEvent> onEvent)
    {
        Console.WriteLine("[mParticle MAUI SDK] Rokt global events subscription is not yet supported on Android.");
    }

    private static IDictionary<string, Com.Mparticle.Kits.RoktEmbeddedView> ConvertEmbeddedViews(
        Dictionary<string, RoktEmbeddedView> embeddedViews)
    {
        if (embeddedViews == null || embeddedViews.Count == 0)
        {
            return null;
        }

        var result = new Dictionary<string, Com.Mparticle.Kits.RoktEmbeddedView>();
        foreach (var kvp in embeddedViews)
        {
            if (kvp.Value?.Handler?.PlatformView is Com.Mparticle.Kits.RoktEmbeddedView nativeView)
            {
                result[kvp.Key] = nativeView;
            }
        }

        return result.Count > 0 ? result : null;
    }
}

public class RoktEmbeddedViewHandler : ViewHandler<RoktEmbeddedView, global::Android.Views.View>
{
    public static IPropertyMapper<RoktEmbeddedView, RoktEmbeddedViewHandler> PropertyMapper =
        new PropertyMapper<RoktEmbeddedView, RoktEmbeddedViewHandler>(ViewHandler.ViewMapper)
        {
        };

    public RoktEmbeddedViewHandler() : base(PropertyMapper)
    {
    }

    protected override global::Android.Views.View CreatePlatformView()
    {
        return new Com.Mparticle.Kits.RoktEmbeddedView(Platform.CurrentActivity);
    }
}

/// <summary>
/// Wraps the managed <see cref="RoktEvent"/> callback and converts the primitive parameter map
/// emitted by the Kotlin bridge back into strongly-typed cross-platform Rokt events.
/// </summary>
internal sealed class RoktEventCallbackWrapper : Java.Lang.Object, Com.Mparticle.Mparticleroktbinding.IRoktEventCallback
{
    private readonly Action<RoktEvent> _onEvent;

    public RoktEventCallbackWrapper(Action<RoktEvent> onEvent)
    {
        _onEvent = onEvent;
    }

    public void OnEvent(IDictionary<string, string> parameters)
    {
        var roktEvent = Convert(parameters);
        if (roktEvent != null)
        {
            _onEvent?.Invoke(roktEvent);
        }
    }

    private static RoktEvent Convert(IDictionary<string, string> p)
    {
        if (p == null)
        {
            return null;
        }

        var name = p.TryGetValue("event", out var e) ? e : null;
        var identifier = p.TryGetValue("placementId", out var id) ? id : null;

        switch (name)
        {
            case "InitComplete":
                return new RoktInitComplete(ParseBool(p, "status"));
            case "ShowLoadingIndicator":
                return new RoktShowLoadingIndicator();
            case "HideLoadingIndicator":
                return new RoktHideLoadingIndicator();
            case "PlacementReady":
                return new RoktPlacementReady(identifier);
            case "PlacementInteractive":
                return new RoktPlacementInteractive(identifier);
            case "PlacementClosed":
                return new RoktPlacementClosed(identifier);
            case "PlacementCompleted":
                return new RoktPlacementCompleted(identifier);
            case "PlacementFailure":
                return new RoktPlacementFailure(identifier);
            case "OfferEngagement":
                return new RoktOfferEngagement(identifier);
            case "PositiveEngagement":
                return new RoktPositiveEngagement(identifier);
            case "FirstPositiveEngagement":
                return new RoktFirstPositiveEngagement(
                    identifier,
                    _ => Console.WriteLine("[mParticle MAUI SDK] SetFulfillmentAttributes is not supported on Android."));
            case "OpenUrl":
                return new RoktOpenUrl(identifier, p.TryGetValue("url", out var url) ? url : null);
            case "CartItemInstantPurchase":
                return new RoktCartItemInstantPurchase(
                    identifier,
                    null,
                    p.TryGetValue("cartItemId", out var cartItemId) ? cartItemId : null,
                    p.TryGetValue("catalogItemId", out var catalogItemId) ? catalogItemId : null,
                    p.TryGetValue("currency", out var currency) ? currency : null,
                    p.TryGetValue("description", out var description) ? description : null,
                    p.TryGetValue("linkedProductId", out var linkedProductId) ? linkedProductId : null,
                    null,
                    ParseDecimal(p, "quantity"),
                    ParseDecimal(p, "totalPrice"),
                    ParseDecimal(p, "unitPrice"));
            default:
                return null;
        }
    }

    private static bool ParseBool(IDictionary<string, string> p, string key) =>
        p.TryGetValue(key, out var v) && bool.TryParse(v, out var b) && b;

    private static decimal? ParseDecimal(IDictionary<string, string> p, string key) =>
        p.TryGetValue(key, out var v) && decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
            ? d
            : (decimal?)null;
}
