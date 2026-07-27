using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("mParticle.Maui.Kits.Rokt.Payments")]

namespace mParticle.MAUI.Rokt;

public enum RoktColorMode
{
    Light = 0,
    Dark = 1,
    System = 2
}

public sealed class RoktConfig
{
    public RoktColorMode ColorMode { get; set; }
    public int? CacheDuration { get; set; }
    public Dictionary<string, string> CacheAttributes { get; set; }

    public RoktConfig()
    {
        ColorMode = RoktColorMode.System;
        CacheAttributes = new Dictionary<string, string>();
    }
}

public abstract class RoktEvent
{
}

public sealed class RoktInitComplete : RoktEvent
{
    public bool Success { get; }

    public RoktInitComplete(bool success)
    {
        Success = success;
    }
}

public sealed class RoktShowLoadingIndicator : RoktEvent
{
}

public sealed class RoktHideLoadingIndicator : RoktEvent
{
}

public sealed class RoktPlacementReady : RoktEvent
{
    public string Identifier { get; }

    public RoktPlacementReady(string identifier)
    {
        Identifier = identifier;
    }
}

public sealed class RoktPlacementInteractive : RoktEvent
{
    public string Identifier { get; }

    public RoktPlacementInteractive(string identifier)
    {
        Identifier = identifier;
    }
}

public sealed class RoktPlacementClosed : RoktEvent
{
    public string Identifier { get; }

    public RoktPlacementClosed(string identifier)
    {
        Identifier = identifier;
    }
}

public sealed class RoktPlacementCompleted : RoktEvent
{
    public string Identifier { get; }

    public RoktPlacementCompleted(string identifier)
    {
        Identifier = identifier;
    }
}

public sealed class RoktPlacementFailure : RoktEvent
{
    public string Identifier { get; }

    public RoktPlacementFailure(string identifier)
    {
        Identifier = identifier;
    }
}

public sealed class RoktOfferEngagement : RoktEvent
{
    public string Identifier { get; }

    public RoktOfferEngagement(string identifier)
    {
        Identifier = identifier;
    }
}

public sealed class RoktPositiveEngagement : RoktEvent
{
    public string Identifier { get; }

    public RoktPositiveEngagement(string identifier)
    {
        Identifier = identifier;
    }
}

public sealed class RoktFirstPositiveEngagement : RoktEvent
{
    public string Identifier { get; }
    public Action<Dictionary<string, string>> SetFulfillmentAttributes { get; }

    public RoktFirstPositiveEngagement(string identifier, Action<Dictionary<string, string>> setFulfillmentAttributes)
    {
        Identifier = identifier;
        SetFulfillmentAttributes = setFulfillmentAttributes;
    }
}

public sealed class RoktOpenUrl : RoktEvent
{
    public string Identifier { get; }
    public string Url { get; }

    public RoktOpenUrl(string identifier, string url)
    {
        Identifier = identifier;
        Url = url;
    }
}

public sealed class RoktEmbeddedSizeChanged : RoktEvent
{
    public string Identifier { get; }
    public double UpdatedHeight { get; }

    public RoktEmbeddedSizeChanged(string identifier, double updatedHeight)
    {
        Identifier = identifier;
        UpdatedHeight = updatedHeight;
    }
}

public sealed class RoktCartItemInstantPurchaseInitiated : RoktEvent
{
    public string Identifier { get; }
    public string CatalogItemId { get; }
    public string CartItemId { get; }

    public RoktCartItemInstantPurchaseInitiated(string identifier, string catalogItemId, string cartItemId)
    {
        Identifier = identifier;
        CatalogItemId = catalogItemId;
        CartItemId = cartItemId;
    }
}

public sealed class RoktCartItemInstantPurchase : RoktEvent
{
    public string Identifier { get; }
    public string Name { get; }
    public string CartItemId { get; }
    public string CatalogItemId { get; }
    public string Currency { get; }
    public string Description { get; }
    public string LinkedProductId { get; }
    public string ProviderData { get; }
    public decimal? Quantity { get; }
    public decimal? TotalPrice { get; }
    public decimal? UnitPrice { get; }

    public RoktCartItemInstantPurchase(
        string identifier,
        string name,
        string cartItemId,
        string catalogItemId,
        string currency,
        string description,
        string linkedProductId,
        string providerData,
        decimal? quantity,
        decimal? totalPrice,
        decimal? unitPrice)
    {
        Identifier = identifier;
        Name = name;
        CartItemId = cartItemId;
        CatalogItemId = catalogItemId;
        Currency = currency;
        Description = description;
        LinkedProductId = linkedProductId;
        ProviderData = providerData;
        Quantity = quantity;
        TotalPrice = totalPrice;
        UnitPrice = unitPrice;
    }
}

public sealed class RoktCartItemInstantPurchaseFailure : RoktEvent
{
    public string Identifier { get; }
    public string CatalogItemId { get; }
    public string CartItemId { get; }
    public string Error { get; }

    public RoktCartItemInstantPurchaseFailure(string identifier, string catalogItemId, string cartItemId, string error)
    {
        Identifier = identifier;
        CatalogItemId = catalogItemId;
        CartItemId = cartItemId;
        Error = error;
    }
}

public sealed class RoktInstantPurchaseDismissal : RoktEvent
{
    public string Identifier { get; }

    public RoktInstantPurchaseDismissal(string identifier)
    {
        Identifier = identifier;
    }
}

public sealed class RoktCartItemDevicePay : RoktEvent
{
    public string Identifier { get; }
    public string CatalogItemId { get; }
    public string CartItemId { get; }
    public string PaymentProvider { get; }

    public RoktCartItemDevicePay(string identifier, string catalogItemId, string cartItemId, string paymentProvider)
    {
        Identifier = identifier;
        CatalogItemId = catalogItemId;
        CartItemId = cartItemId;
        PaymentProvider = paymentProvider;
    }
}

public abstract class RoktApi
{
    internal const string SdkNotInitializedWarning =
        "[mParticle MAUI SDK] Warning: SDK has not been initialized. Please call Initialize() before utilizing mParticle SDK.";

    /// <summary>
    /// Select placements for Rokt integration
    /// </summary>
    /// <param name="identifier">Placement identifier</param>
    /// <param name="attributes">Optional attributes dictionary</param>
    /// <param name="embeddedViews">Optional embedded views dictionary</param>
    /// <param name="config">Optional Rokt configuration</param>
    public abstract void SelectPlacements(
        string identifier,
        Dictionary<string, string> attributes = null,
        Dictionary<string, RoktEmbeddedView> embeddedViews = null,
        RoktConfig config = null);

    /// <summary>
    /// Subscribes to events for a specific placement identifier.
    /// </summary>
    /// <param name="identifier">Placement identifier.</param>
    /// <param name="onEvent">Event callback.</param>
    public abstract void Events(string identifier, Action<RoktEvent> onEvent);

    /// <summary>
    /// Subscribes to global Rokt events.
    /// </summary>
    /// <param name="onEvent">Event callback.</param>
    public abstract void GlobalEvents(Action<RoktEvent> onEvent);

    /// <summary>
    /// Platform-native Rokt handle for optional kits (e.g. Payments) to route calls
    /// through this receiver instead of the global singleton. Returns <c>null</c> when
    /// the underlying native Rokt is unavailable (e.g. SDK not initialized).
    /// </summary>
    internal virtual object NativeHandle => null;
}

public sealed class RoktEmbeddedView : Microsoft.Maui.Controls.View
{
}

internal sealed class NoOpRoktApi : RoktApi
{
    public override void SelectPlacements(
        string identifier,
        Dictionary<string, string> attributes = null,
        Dictionary<string, RoktEmbeddedView> embeddedViews = null,
        RoktConfig config = null)
    {
        Console.WriteLine(SdkNotInitializedWarning);
    }

    public override void Events(string identifier, Action<RoktEvent> onEvent)
    {
        Console.WriteLine(SdkNotInitializedWarning);
    }

    public override void GlobalEvents(Action<RoktEvent> onEvent)
    {
        Console.WriteLine(SdkNotInitializedWarning);
    }
}
