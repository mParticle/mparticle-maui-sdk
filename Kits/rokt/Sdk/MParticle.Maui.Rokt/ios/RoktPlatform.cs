using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Foundation;
using UIKit;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using iOSBinding = mParticle.MAUI.iOSBinding;

namespace mParticle.MAUI.Rokt;

internal sealed class IosRoktApi : RoktApi
{
    private readonly iOSBinding.MPRokt _roktInstance;
    private static readonly Dictionary<string, RoktEmbeddedView> EmbeddedViews = new();

    internal IosRoktApi(iOSBinding.MPRokt roktInstance)
    {
        _roktInstance = roktInstance;
    }

    internal override object NativeHandle => _roktInstance;

    public override void SelectPlacements(
        string identifier,
        Dictionary<string, string> attributes = null,
        Dictionary<string, RoktEmbeddedView> embeddedViews = null,
        RoktConfig config = null)
    {
        if (embeddedViews != null)
        {
            foreach (var kvp in embeddedViews)
            {
                EmbeddedViews[kvp.Key] = kvp.Value;
            }
        }

        var nsAttributes = RoktInterop.ConvertToNSDictionary(attributes);
        var nsEmbeddedViews = RoktInterop.ConvertEmbeddedViewsToNSDictionary(embeddedViews);
        var nsConfig = RoktInterop.ConvertToMpRoktConfig(config);

        // Keep embedded view heights in sync with native iOS size events.
        Action<iOSBinding.RoktEvent> enhancedCallbacks = roktEvent =>
        {
            if (roktEvent is iOSBinding.RoktEmbeddedSizeChanged sizeChanged &&
                EmbeddedViews.TryGetValue(sizeChanged.Identifier, out var view))
            {
                view.HeightRequest = sizeChanged.UpdatedHeight;
            }
        };

        _roktInstance.SelectPlacements(identifier, nsAttributes, nsEmbeddedViews, nsConfig, enhancedCallbacks);
    }

    public override void Events(string identifier, Action<RoktEvent> onEvent)
    {
        _roktInstance.Events(identifier, RoktInterop.ConvertToMpRoktEventCallback(onEvent));
    }

    public override void GlobalEvents(Action<RoktEvent> onEvent)
    {
        _roktInstance.GlobalEvents(RoktInterop.ConvertToMpRoktEventCallback(onEvent));
    }
}

public class RoktEmbeddedViewHandler : ViewHandler<RoktEmbeddedView, iOSBinding.RoktEmbeddedView>
{
    public static IPropertyMapper<RoktEmbeddedView, RoktEmbeddedViewHandler> PropertyMapper =
        new PropertyMapper<RoktEmbeddedView, RoktEmbeddedViewHandler>(ViewHandler.ViewMapper);

    public RoktEmbeddedViewHandler() : base(PropertyMapper)
    {
    }

    protected override iOSBinding.RoktEmbeddedView CreatePlatformView()
    {
        var embeddedView = new iOSBinding.RoktEmbeddedView();
        embeddedView.TranslatesAutoresizingMaskIntoConstraints = false;
        return embeddedView;
    }
}

internal static class RoktInterop
{
    internal static iOSBinding.RoktConfig ConvertToMpRoktConfig(RoktConfig config)
    {
        if (config == null)
            return null;

        var builder = new iOSBinding.RoktConfigBuilder()
            .ColorMode(ConvertToMpRoktColorMode(config.ColorMode));

        if (config.CacheDuration.HasValue || (config.CacheAttributes != null && config.CacheAttributes.Any()))
        {
            var cacheDuration = config.CacheDuration.HasValue
                ? config.CacheDuration.Value
                : iOSBinding.RoktCacheConfig.MaxCacheDuration;
            var cacheAttributes = ConvertToNSDictionary(config.CacheAttributes);
            var cacheConfig = new iOSBinding.RoktCacheConfig(cacheDuration, cacheAttributes);
            builder = builder.CacheConfig(cacheConfig);
        }

        return builder.Build();
    }

    internal static iOSBinding.RoktColorMode ConvertToMpRoktColorMode(RoktColorMode colorMode)
    {
        switch (colorMode)
        {
            case RoktColorMode.Light:
                return iOSBinding.RoktColorMode.Light;
            case RoktColorMode.Dark:
                return iOSBinding.RoktColorMode.Dark;
            case RoktColorMode.System:
            default:
                return iOSBinding.RoktColorMode.System;
        }
    }

    internal static Action<iOSBinding.RoktEvent> ConvertToMpRoktEventCallback(Action<RoktEvent> onEvent)
    {
        if (onEvent == null)
        {
            return null;
        }

        return roktEvent =>
        {
            var crossPlatformEvent = ConvertToCrossPlatformRoktEvent(roktEvent);
            if (crossPlatformEvent != null)
            {
                onEvent.Invoke(crossPlatformEvent);
            }
        };
    }

    internal static RoktEvent ConvertToCrossPlatformRoktEvent(iOSBinding.RoktEvent roktEvent)
    {
        switch (roktEvent)
        {
            case iOSBinding.RoktInitComplete e:
                return new RoktInitComplete(e.Success);
            case iOSBinding.RoktShowLoadingIndicator:
                return new RoktShowLoadingIndicator();
            case iOSBinding.RoktHideLoadingIndicator:
                return new RoktHideLoadingIndicator();
            case iOSBinding.RoktPlacementReady e:
                return new RoktPlacementReady(e.Identifier);
            case iOSBinding.RoktPlacementInteractive e:
                return new RoktPlacementInteractive(e.Identifier);
            case iOSBinding.RoktPlacementClosed e:
                return new RoktPlacementClosed(e.Identifier);
            case iOSBinding.RoktPlacementCompleted e:
                return new RoktPlacementCompleted(e.Identifier);
            case iOSBinding.RoktPlacementFailure e:
                return new RoktPlacementFailure(e.Identifier);
            case iOSBinding.RoktOfferEngagement e:
                return new RoktOfferEngagement(e.Identifier);
            case iOSBinding.RoktPositiveEngagement e:
                return new RoktPositiveEngagement(e.Identifier);
            case iOSBinding.RoktFirstPositiveEngagement e:
                return new RoktFirstPositiveEngagement(
                    e.Identifier,
                    attributes => e.SetFulfillmentAttributes?.Invoke(ConvertToNSDictionary(attributes)));
            case iOSBinding.RoktOpenUrl e:
                return new RoktOpenUrl(e.Identifier, e.Url);
            case iOSBinding.RoktEmbeddedSizeChanged e:
                return new RoktEmbeddedSizeChanged(e.Identifier, (double)e.UpdatedHeight);
            case iOSBinding.RoktCartItemInstantPurchaseInitiated e:
                return new RoktCartItemInstantPurchaseInitiated(e.Identifier, e.CatalogItemId, e.CartItemId);
            case iOSBinding.RoktCartItemInstantPurchase e:
                return new RoktCartItemInstantPurchase(
                    e.Identifier,
                    e.Name,
                    e.CartItemId,
                    e.CatalogItemId,
                    e.Currency,
                    e.ItemDescription,
                    e.LinkedProductId,
                    e.ProviderData,
                    ConvertToNullableDecimal(e.Quantity),
                    ConvertToNullableDecimal(e.TotalPrice),
                    ConvertToNullableDecimal(e.UnitPrice));
            case iOSBinding.RoktCartItemInstantPurchaseFailure e:
                return new RoktCartItemInstantPurchaseFailure(e.Identifier, e.CatalogItemId, e.CartItemId, e.Error);
            case iOSBinding.RoktInstantPurchaseDismissal e:
                return new RoktInstantPurchaseDismissal(e.Identifier);
            case iOSBinding.RoktCartItemDevicePay e:
                return new RoktCartItemDevicePay(e.Identifier, e.CatalogItemId, e.CartItemId, e.PaymentProvider);
            default:
                return null;
        }
    }

    internal static decimal? ConvertToNullableDecimal(NSDecimalNumber value)
    {
        if (value == null)
        {
            return null;
        }

        var stringValue = value.StringValue;
        if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedInvariant))
        {
            return parsedInvariant;
        }

        if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var parsedCurrent))
        {
            return parsedCurrent;
        }

        return null;
    }

    internal static NSDictionary<NSString, NSString> ConvertToNSDictionary(Dictionary<string, string> dictionary)
    {
        if (dictionary == null || !dictionary.Any())
            return new NSDictionary<NSString, NSString>();

        return NSDictionary<NSString, NSString>.FromObjectsAndKeys(dictionary.Values.ToArray(), dictionary.Keys.ToArray());
    }

    internal static NSDictionary ConvertEmbeddedViewsToNSDictionary(Dictionary<string, RoktEmbeddedView> embeddedViews)
    {
        if (embeddedViews == null || !embeddedViews.Any())
        {
            return null;
        }

        var filteredViews = embeddedViews
            .Where(kvp => kvp.Value?.Handler is { PlatformView: UIKit.UIView })
            .Select(kvp => new KeyValuePair<string, iOSBinding.RoktEmbeddedView>(
                kvp.Key,
                (kvp.Value.Handler?.PlatformView as iOSBinding.RoktEmbeddedView)!
            ))
            .Where(kvp => kvp.Value != null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (!filteredViews.Any())
        {
            return null;
        }

        var keys = filteredViews.Keys.ToArray();
        var values = filteredViews.Values.ToArray();

        var nativeDictionary = NSDictionary.FromObjectsAndKeys(
            values.Cast<object>().ToArray(),
            keys.Cast<object>().ToArray()
        );

        return nativeDictionary;
    }
}
