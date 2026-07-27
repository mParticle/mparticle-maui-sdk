using System;
using System.Collections.Generic;
using Android.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Handlers;
using AndroidBinding = mParticle.MAUI.AndroidBinding;

namespace mParticle.MAUI.Rokt;

internal sealed class AndroidRoktApi : RoktApi
{
    private const string SdkNotInitializedWarning =
        "[mParticle MAUI SDK] Warning: SDK has not been initialized. Please call Initialize() before utilizing mParticle SDK.";

    private readonly AndroidBinding.MParticle _mparticleInstance;
    private static readonly List<object> EventSubscriptions = new List<object>();

    internal AndroidRoktApi(AndroidBinding.MParticle mparticleInstance)
    {
        _mparticleInstance = mparticleInstance;
    }

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
        var javaAttributes = RoktInterop.ConvertToDictionary(attributes);
        var javaEmbeddedViews = RoktInterop.ConvertEmbeddedViewsToWeakReferenceDictionary(embeddedViews);
        var javaConfig = RoktInterop.ConvertToRoktConfig(config);
        var roktInstance = _mparticleInstance.Rokt();
        if (roktInstance != null)
        {
            // Android Rokt API: selectPlacements(identifier, attributes, callbacks, embeddedViews, fontTypefaces, config)
            roktInstance.SelectPlacements(identifier, javaAttributes, null, javaEmbeddedViews, null, javaConfig);
        }
        else
        {
            throw new InvalidOperationException("Rokt instance is not available. Make sure mParticle is properly initialized.");
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

        var roktInstance = _mparticleInstance.Rokt();
        if (roktInstance == null)
        {
            throw new InvalidOperationException("Rokt instance is not available. Make sure mParticle is properly initialized.");
        }

        var listener = RoktInterop.ConvertToRoktFlowEventListener(onEvent);
        var subscription = Com.Mparticle.Mparticlebinding.MParticleSdkBinding.SubscribeToEvents(roktInstance, identifier, listener);

        lock (EventSubscriptions)
        {
            EventSubscriptions.Add(listener);
            EventSubscriptions.Add(subscription);
        }
    }

    public override void GlobalEvents(Action<RoktEvent> onEvent)
    {
        Console.WriteLine("[mParticle MAUI SDK] Rokt global events subscription is not yet supported on Android.");
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
        return new Com.Mparticle.Rokt.RoktEmbeddedView(Platform.CurrentActivity);
    }
}

internal static class RoktInterop
{
    internal static IDictionary<string, string> ConvertToDictionary(Dictionary<string, string> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
            return null;

        return new Dictionary<string, string>(dictionary);
    }

    internal static IDictionary<string, Java.Lang.Ref.WeakReference> ConvertEmbeddedViewsToWeakReferenceDictionary(Dictionary<string, RoktEmbeddedView> embeddedViews)
    {
        if (embeddedViews == null || embeddedViews.Count == 0)
            return null;

        var dictionary = new Dictionary<string, Java.Lang.Ref.WeakReference>();
        foreach (var kvp in embeddedViews)
        {
            Com.Mparticle.Rokt.RoktEmbeddedView androidEmbeddedView;

            if (kvp.Value?.Handler?.PlatformView is Com.Mparticle.Rokt.RoktEmbeddedView platformView)
            {
                androidEmbeddedView = platformView;
            }
            else
            {
                androidEmbeddedView = new Com.Mparticle.Rokt.RoktEmbeddedView(Platform.CurrentActivity);
            }

            var weakRef = new Java.Lang.Ref.WeakReference(androidEmbeddedView);
            dictionary[kvp.Key] = weakRef;
        }
        return dictionary;
    }

    internal static Com.Mparticle.Rokt.RoktConfig ConvertToRoktConfig(RoktConfig config)
    {
        if (config == null)
            return null;

        var builder = new Com.Mparticle.Rokt.RoktConfig.Builder();
        builder.ColorMode(ConvertToRoktColorMode(config.ColorMode));

        if (config.CacheDuration.HasValue || (config.CacheAttributes != null && config.CacheAttributes.Count > 0))
        {
            var cacheDuration = config.CacheDuration ?? Com.Mparticle.Rokt.CacheConfig.DefaultCacheDurationSecs;
            var cacheConfig = new Com.Mparticle.Rokt.CacheConfig(cacheDuration, config.CacheAttributes);
            builder.CacheConfig(cacheConfig);
        }

        return builder.Build();
    }

    internal static Com.Mparticle.Rokt.RoktConfig.ColorMode ConvertToRoktColorMode(RoktColorMode colorMode)
    {
        switch (colorMode)
        {
            case RoktColorMode.Light:
                return Com.Mparticle.Rokt.RoktConfig.ColorMode.Light!;
            case RoktColorMode.Dark:
                return Com.Mparticle.Rokt.RoktConfig.ColorMode.Dark!;
            case RoktColorMode.System:
            default:
                return Com.Mparticle.Rokt.RoktConfig.ColorMode.System!;
        }
    }

    internal static Com.Mparticle.Mparticlebinding.IRoktFlowEventListener ConvertToRoktFlowEventListener(Action<RoktEvent> onEvent)
    {
        return new RoktFlowEventListenerWrapper(onEvent);
    }

    internal static RoktEvent ConvertToCrossPlatformRoktEvent(AndroidBinding.IRoktEvent roktEvent)
    {
        switch (roktEvent)
        {
            case AndroidBinding.IRoktEvent.InitComplete e:
                return new RoktInitComplete(e.Success);
            case AndroidBinding.IRoktEvent.ShowLoadingIndicator:
                return new RoktShowLoadingIndicator();
            case AndroidBinding.IRoktEvent.HideLoadingIndicator:
                return new RoktHideLoadingIndicator();
            case AndroidBinding.IRoktEvent.PlacementReady e:
                return new RoktPlacementReady(e.PlacementId);
            case AndroidBinding.IRoktEvent.PlacementInteractive e:
                return new RoktPlacementInteractive(e.PlacementId);
            case AndroidBinding.IRoktEvent.PlacementClosed e:
                return new RoktPlacementClosed(e.PlacementId);
            case AndroidBinding.IRoktEvent.PlacementCompleted e:
                return new RoktPlacementCompleted(e.PlacementId);
            case AndroidBinding.IRoktEvent.PlacementFailure e:
                return new RoktPlacementFailure(e.PlacementId);
            case AndroidBinding.IRoktEvent.OfferEngagement e:
                return new RoktOfferEngagement(e.PlacementId);
            case AndroidBinding.IRoktEvent.PositiveEngagement e:
                return new RoktPositiveEngagement(e.PlacementId);
            case AndroidBinding.IRoktEvent.FirstPositiveEngagement e:
                return new RoktFirstPositiveEngagement(
                    e.PlacementId,
                    _ => Console.WriteLine("[mParticle MAUI SDK] SetFulfillmentAttributes is not supported on Android."));
            case AndroidBinding.IRoktEvent.OpenUrl e:
                return new RoktOpenUrl(e.PlacementId, e.Url);
            case AndroidBinding.IRoktEvent.CartItemInstantPurchase e:
                return new RoktCartItemInstantPurchase(
                    e.PlacementId,
                    null,
                    e.CartItemId,
                    e.CatalogItemId,
                    e.Currency,
                    e.Description,
                    e.LinkedProductId,
                    null,
                    (decimal)e.Quantity,
                    (decimal)e.TotalPrice,
                    (decimal)e.UnitPrice);
            default:
                return null;
        }
    }

    private sealed class RoktFlowEventListenerWrapper : Java.Lang.Object, Com.Mparticle.Mparticlebinding.IRoktFlowEventListener
    {
        private readonly Action<RoktEvent> _onEvent;

        public RoktFlowEventListenerWrapper(Action<RoktEvent> onEvent)
        {
            _onEvent = onEvent;
        }

        public void OnEvent(AndroidBinding.IRoktEvent e)
        {
            var crossPlatformEvent = ConvertToCrossPlatformRoktEvent(e);
            if (crossPlatformEvent != null)
            {
                _onEvent?.Invoke(crossPlatformEvent);
            }
        }
    }
}
