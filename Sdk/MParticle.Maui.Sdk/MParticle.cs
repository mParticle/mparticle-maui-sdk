using System.Collections.Generic;
using System.Threading.Channels;
using System.Reflection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;

#if __ANDROID__
using mParticle.MAUI.Android.Wrappers;
using mParticle.MAUI.Android;
using Android.Util;
using Android.Views;
using Object = Java.Lang.Object;
using Java.Util;
using Kotlin.Jvm.Functions;
#endif

#if __IOS__
using Foundation;
using UIKit;
using System;
using System.Collections.Generic;
using System.Linq;
using mParticle.MAUI.iOSBinding;
using mParticle.MAUI.iOS.Utils;
#endif

namespace mParticle.MAUI;

public static class MParticle
{
    static Lazy<MParticleSDK> TTS = new Lazy<MParticleSDK>(() => CreateInstance(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    public static MParticleSDK Instance
    {
        get
        {
            var ret = TTS.Value;
            if (ret == null)
            {
                throw NotImplementedInReferenceAssembly();
            }
            return ret;
        }
    }

    static MParticleSDK CreateInstance()
    {
        return new MParticleSDKImpl();
    }

    internal static Exception NotImplementedInReferenceAssembly()
    {
        return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the mParticle.MAUI NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
}

#if __IOS__

public class MParticleSDKImpl : MParticleSDK
{
    internal MParticleSDKImpl()
    {
    }

    public override Environment Environment
    {
        get
        {
            if (!_isInitialized)
            {
                Console.WriteLine(SdkNotInitializedWarning);
                return Environment.AutoDetect;
            }

            switch (iOSBinding.MParticle.SharedInstance.Environment)
            {
                case iOSBinding.MPEnvironment.Development:
                    return Environment.Development;
                case iOSBinding.MPEnvironment.Production:
                    return Environment.Production;
                default:
                    return Environment.AutoDetect;
            }
        }
    }

    public override MParticleSDK Initialize(MParticleOptions options)
    {
        if (options.IdentityStateListener != null)
        {
            Identity.AddIdentityStateListener(options.IdentityStateListener);
        }
        iOSBinding.MParticle.SharedInstance.StartWithOptions(Utils.ConvertToMpOptions(options));
        var mparticle = iOSBinding.MParticle.SharedInstance;
        
        // Set wrapper SDK after starting
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        
        // Runtime print to show the version being used
        Console.WriteLine($"[mParticle MAUI SDK] iOS Using version: {version}");
        
        iOSBinding.MParticle.SetWrapperSdkInternal(iOSBinding.MPWrapperSdk.MPWrapperSdkMaui, version);
        
        mparticle.LogLevel = Utils.ConvertToMpLogLevel(options.LogLevel);
        // mParticle Apple SDK 9 removed begin/end location tracking selectors.
        // Keep LocationTracking as Android-only behavior for now.
        if (options.PushRegistration != null && options.PushRegistration.IOSToken != null)
        {
            mparticle.PushNotificationToken = options.PushRegistration.IOSToken;
        }
        
        _isInitialized = true;
        return this;
    }

    public override void LeaveBreadcrumb(string breadcrumbName)
    {
        iOSBinding.MParticle.SharedInstance.LeaveBreadcrumb(breadcrumbName);
    }

    public override void LogCommerceEvent(CommerceEvent commerceEvent, bool shouldUploadEvent = true)
    {
        var bindingCommerceEvent = new iOSBinding.MPCommerceEvent();

        if (commerceEvent.TransactionAttributes != null)
            bindingCommerceEvent.TransactionAttributes = Utils.ConvertToMpTransactionAttributes(commerceEvent.TransactionAttributes);

        bindingCommerceEvent.ScreenName = commerceEvent.ScreenName;
        bindingCommerceEvent.Currency = commerceEvent.Currency;
        bindingCommerceEvent.SetCustomAttributes(ConvertToNSDictionary<NSString, NSObject>(commerceEvent.CustomAttributes));
        bindingCommerceEvent.CheckoutOptions = commerceEvent.CheckoutOptions;

        if (commerceEvent.Products != null)
        {
            bindingCommerceEvent.Action = ConvertToMpProductAction(commerceEvent.ProductAction);
            foreach (var product in commerceEvent.Products)
            {
                bindingCommerceEvent.AddProduct(Utils.ConvertToMpProduct(product));
            }
        }

        if (commerceEvent.CheckoutStep != null)
            bindingCommerceEvent.CheckoutStep = commerceEvent.CheckoutStep.Value;

        if (commerceEvent.NonInteractive.HasValue)
            bindingCommerceEvent.NonInteractive = commerceEvent.NonInteractive.Value;

        if (commerceEvent.Promotions != null)
        {
            bindingCommerceEvent.PromotionContainer = new MPPromotionContainer(ConvertToMpPromotionAction(commerceEvent.PromotionAction), null);
            foreach (var promotion in commerceEvent.Promotions)
            {
                bindingCommerceEvent.PromotionContainer.AddPromotion(ConvertToMpPromotion(promotion));
            }
        }

        if (commerceEvent.Impressions != null)
        {
            foreach (var impression in commerceEvent.Impressions)
            {
                foreach (var product in impression.Products)
                {
                    bindingCommerceEvent.AddImpression(Utils.ConvertToMpProduct(product), impression.ImpressionListName);
                }
            }
        }

        bindingCommerceEvent.ShouldUploadEvent = shouldUploadEvent;

        iOSBinding.MParticle.SharedInstance.LogCommerceEvent(bindingCommerceEvent);
    }

    private MPCommerceEventAction ConvertToMpProductAction(ProductAction action)
    {
        switch (action)
        {
            case ProductAction.AddToCart:
                return MPCommerceEventAction.AddToCart;
            case ProductAction.AddToWishlist:
                return MPCommerceEventAction.AddToWishList;
            case ProductAction.Checkout:
                return MPCommerceEventAction.Checkout;
            case ProductAction.CheckoutOption:
                return MPCommerceEventAction.CheckoutOptions;
            case ProductAction.Click:
                return MPCommerceEventAction.Click;
            case ProductAction.Purchase:
                return MPCommerceEventAction.Purchase;
            case ProductAction.Refund:
                return MPCommerceEventAction.Refund;
            case ProductAction.RemoveFromWishlist:
                return MPCommerceEventAction.RemoveFromWishlist;
            case ProductAction.RemoveFromCart:
                return MPCommerceEventAction.RemoveFromCart;
            case ProductAction.ViewDetail:
                return MPCommerceEventAction.ViewDetail;
            default:
                return 0;
        }
    }

    private MPPromotionAction ConvertToMpPromotionAction(PromotionAction action)
    {
        switch (action)
        {
            case PromotionAction.Click:
                return MPPromotionAction.Click;
            case PromotionAction.View:
            default:
                return MPPromotionAction.View;
        }
    }

    private MPPromotion ConvertToMpPromotion(Promotion promotion)
    {
        var bindingPromotion = new MPPromotion();
        bindingPromotion.Creative = promotion.Creative;
        bindingPromotion.PromotionId = promotion.Id;
        bindingPromotion.Name = promotion.Name;
        bindingPromotion.Position = promotion.Position?.ToString();
        return bindingPromotion;
    }

    public override void LogEvent(string eventName, EventType eventType, Dictionary<string, string> eventInfo, bool shouldUploadEvent = true)
    {
        var mpEventType = (MPEventType)Enum.Parse(typeof(iOSBinding.MPEventType), eventType.ToString());
        var mpEvent = new MPEvent(eventName, mpEventType);
        mpEvent.CustomAttributes = ConvertToNSDictionary<NSString, NSObject>(eventInfo);
        mpEvent.ShouldUploadEvent = shouldUploadEvent;
        iOSBinding.MParticle.SharedInstance.LogEvent(mpEvent);
    }

    public override void LogScreen(string screenName, Dictionary<string, string> eventInfo)
    {
        iOSBinding.MParticle.SharedInstance.LogScreen(screenName, ConvertToNSDictionary<NSString, NSObject>(eventInfo));
    }

    public override void SetATTStatus(MPATTAuthorizationStatus status, long? attStatusTimestampMillis)
    {
        iOSBinding.MParticle.SharedInstance.setATTStatus((iOSBinding.MPATTAuthorizationStatus)status, attStatusTimestampMillis);
    }

    public override void SetOptOut(bool optOut)
    {
        iOSBinding.MParticle.SharedInstance.OptOut = optOut;
    }

    private static NSDictionary<T, V> ConvertToNSDictionary<T, V>(Dictionary<string, string> dictionary) where T : NSString where V : NSObject
    {
        if (dictionary == null || !dictionary.Any())
            return new NSDictionary<T, V>();

        return NSDictionary<T, V>.FromObjectsAndKeys(dictionary.Values.ToArray(), dictionary.Keys.ToArray());
    }

    public override object GetBindingInstance()
    {
        return iOSBinding.MParticle.SharedInstance;
    }

    public override IdentityApi Identity
    {
        get
        {
            return IdentityApiWrapper.GetInstance(iOSBinding.MParticle.SharedInstance.Identity);
        }
    }

    public override RoktApi Rokt
    {
        get
        {
            if (!_isInitialized)
            {
                return new NoOpRoktApiWrapper();
            }
            return new RoktApiWrapper(iOSBinding.MParticle.SharedInstance.Rokt);
        }
    }

    public override void Destroy()
    {
        iOSBinding.MParticle.SharedInstance = null;
    }

    public override void Upload()
    {
        iOSBinding.MParticle.SharedInstance.Upload();
    }
}

public class RoktApiWrapper : RoktApi
{
    private readonly iOSBinding.MPRokt _roktInstance;
    private static readonly Dictionary<string, RoktEmbeddedView> EmbeddedViews = new();

    internal RoktApiWrapper(iOSBinding.MPRokt roktInstance)
    {
        _roktInstance = roktInstance;
    }

    public override void SelectPlacements(string identifier, 
        Dictionary<string, string> attributes = null,
        Dictionary<string, RoktEmbeddedView> embeddedViews = null,
        RoktConfig config = null)
    {
        
        // Store embedded views for height updates
        if (embeddedViews != null)
        {
            foreach (var kvp in embeddedViews)
            {
                EmbeddedViews[kvp.Key] = kvp.Value;
            }
        }

        var nsAttributes = Utils.ConvertToNSDictionary<NSString, NSString>(attributes);
        var nsEmbeddedViews = Utils.ConvertEmbeddedViewsToNSDictionary(embeddedViews);
        var nsConfig = Utils.ConvertToMpRoktConfig(config);
        
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

    public override void SelectShoppableAds(
        string identifier,
        Dictionary<string, string> attributes = null,
        RoktConfig config = null)
    {
        var nsAttributes = Utils.ConvertToNSDictionary<NSString, NSString>(attributes)
            ?? new NSDictionary<NSString, NSString>();
        var nsConfig = Utils.ConvertToMpRoktConfig(config);

        _roktInstance.SelectShoppableAds(identifier, nsAttributes, nsConfig, null);
    }

    public override void Events(string identifier, Action<RoktEvent> onEvent)
    {
        _roktInstance.Events(identifier, Utils.ConvertToMpRoktEventCallback(onEvent));
    }

    public override void GlobalEvents(Action<RoktEvent> onEvent)
    {
        _roktInstance.GlobalEvents(Utils.ConvertToMpRoktEventCallback(onEvent));
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

#endif

#if __ANDROID__

public class MParticleSDKImpl : MParticleSDK
{
    internal MParticleSDKImpl()
    {
    }

    public override Environment Environment
    {
        get
        {
            if (!_isInitialized)
            {
                Console.WriteLine(SdkNotInitializedWarning);
                return Environment.AutoDetect;
            }

            var environment = mParticle.MAUI.AndroidBinding.MParticle.Instance.GetEnvironment();
            if (environment == mParticle.MAUI.AndroidBinding.MParticle.Environment.AutoDetect)
                return MAUI.Environment.AutoDetect;
            else if (environment == mParticle.MAUI.AndroidBinding.MParticle.Environment.Production)
                return MAUI.Environment.Production;
            else
                return MAUI.Environment.Development;
        }
    }

    public override void LeaveBreadcrumb(string breadcrumbName)
    {
        if (!_isInitialized)
        {
            Console.WriteLine(SdkNotInitializedWarning);
            return;
        }
        mParticle.MAUI.AndroidBinding.MParticle.Instance.LeaveBreadcrumb(breadcrumbName);
    }

    public override void LogCommerceEvent(CommerceEvent commerceEvent, bool shouldUploadEvent = true)
    {
        if (!_isInitialized)
        {
            Console.WriteLine(SdkNotInitializedWarning);
            return;
        }
        Android.CommerceBinding.CommerceEvent.Builder bindingCommerceEventBuilder = null;

        if (commerceEvent.ProductAction > 0 && commerceEvent.Products != null && commerceEvent.Products.Length > 0)
        {
            bindingCommerceEventBuilder = new Android.CommerceBinding.CommerceEvent.Builder(Utils.ConvertToMpProductAction(commerceEvent.ProductAction), Utils.ConvertToMpProduct(commerceEvent.Products[0]));
            var temp = new List<Product>(commerceEvent.Products);
            temp.RemoveAt(0);
            commerceEvent.Products = temp.ToArray();
        }
        else if (commerceEvent.Promotions != null && commerceEvent.Promotions.Length > 0)
        {
            bindingCommerceEventBuilder = new Android.CommerceBinding.CommerceEvent.Builder(Utils.ConvertToMpPromotionAction(commerceEvent.PromotionAction), Utils.ConvertToMpPromotion(commerceEvent.Promotions[0]));
            var temp = new List<Promotion>(commerceEvent.Promotions);
            temp.RemoveAt(0);
            commerceEvent.Promotions = temp.ToArray();
        }
        else
        {
            bindingCommerceEventBuilder = new Android.CommerceBinding.CommerceEvent.Builder(Utils.ConvertToMpImpression(commerceEvent.Impressions[0]));
            var temp = new List<Impression>(commerceEvent.Impressions);
            temp.RemoveAt(0);
            commerceEvent.Impressions = temp.ToArray();
        }

        if (bindingCommerceEventBuilder == null)
            return;

        if (commerceEvent.TransactionAttributes != null)
            bindingCommerceEventBuilder.TransactionAttributes(Utils.ConvertToMpTransactionAttributes(commerceEvent.TransactionAttributes));

        bindingCommerceEventBuilder.Screen(commerceEvent.ScreenName);
        bindingCommerceEventBuilder.Currency(commerceEvent.Currency);
        bindingCommerceEventBuilder.CustomAttributes(commerceEvent.CustomAttributes);
        bindingCommerceEventBuilder.CheckoutOptions(commerceEvent.CheckoutOptions);

        if (commerceEvent.Products != null)
        {
            foreach (var product in commerceEvent.Products)
            {
                bindingCommerceEventBuilder.AddProduct(Utils.ConvertToMpProduct(product));
            }
        }

        if (commerceEvent.CheckoutStep != null)
            bindingCommerceEventBuilder.CheckoutStep(new Java.Lang.Integer(commerceEvent.CheckoutStep.Value));

        if (commerceEvent.NonInteractive.HasValue)
            bindingCommerceEventBuilder.NonInteraction(commerceEvent.NonInteractive.Value);

        if (commerceEvent.Promotions != null)
        {
            foreach (var promotion in commerceEvent.Promotions)
            {
                bindingCommerceEventBuilder.AddPromotion(Utils.ConvertToMpPromotion(promotion));
            }
        }

        if (commerceEvent.Impressions != null)
        {
            foreach (var impression in commerceEvent.Impressions)
            {
                bindingCommerceEventBuilder.AddImpression(Utils.ConvertToMpImpression(impression));
            }
        }

        bindingCommerceEventBuilder.ShouldUploadEvent(shouldUploadEvent);

        mParticle.MAUI.AndroidBinding.MParticle.Instance.LogEvent(bindingCommerceEventBuilder.Build());
    }

    public override void LogEvent(string eventName, EventType eventType, Dictionary<string, string> eventInfo, bool shouldUploadEvent = true)
    {
        if (!_isInitialized)
        {
            Console.WriteLine(SdkNotInitializedWarning);
            return;
        }
        var mpEventType = mParticle.MAUI.AndroidBinding.MParticle.EventType.ValueOf(eventType.ToString());
        IDictionary<string, object> castedEventInfo = eventInfo.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        var mpEvent = new mParticle.MAUI.AndroidBinding.MPEvent.Builder(eventName, mpEventType).CustomAttributes(castedEventInfo).ShouldUploadEvent(shouldUploadEvent).Build();
        mParticle.MAUI.AndroidBinding.MParticle.Instance.LogEvent(mpEvent);
    }

    public override void LogScreen(string screenName, Dictionary<string, string> eventInfo)
    {
        if (!_isInitialized)
        {
            Console.WriteLine(SdkNotInitializedWarning);
            return;
        }
        mParticle.MAUI.AndroidBinding.MParticle.Instance.LogScreen(screenName, eventInfo);
    }

    public override void SetATTStatus(MPATTAuthorizationStatus status, long? attStatusTimestampMillis)
    {
    }

    public override void SetOptOut(bool optOut)
    {
        if (!_isInitialized)
        {
            Console.WriteLine(SdkNotInitializedWarning);
            return;
        }
        mParticle.MAUI.AndroidBinding.MParticle.Instance.OptOut = new Java.Lang.Boolean(optOut);
    }

    public override object GetBindingInstance()
    {
        return mParticle.MAUI.AndroidBinding.MParticle.Instance;
    }

    public override MParticleSDK Initialize(MParticleOptions options)
    {
        var boundOptions = Utils.ConvertToMpOptions(options);

        // Start MParticle first
        mParticle.MAUI.AndroidBinding.MParticle.Start(boundOptions);
        
        // Set wrapper SDK after starting
        var mpInstance = mParticle.MAUI.AndroidBinding.MParticle.Instance;
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        
        mpInstance.SetWrapperSdk(
            mParticle.MAUI.AndroidBinding.WrapperSdk.WrapperMaui, 
            version
        );

        if (options.IdentityStateListener != null)
        {
            Identity.AddIdentityStateListener(options.IdentityStateListener);
        }

        _isInitialized = true;
        return this;
    }

    public override IdentityApi Identity
    {
        get
        {
            return IdentityApiWrapper.GetInstance(mParticle.MAUI.AndroidBinding.MParticle.Instance.Identity());
        }
    }

    public override RoktApi Rokt
    {
        get
        {
            // Rokt moved out of android-core into the optional android-rokt-kit in
            // mParticle Android SDK 6. The Android Rokt implementation now lives in the
            // MParticle.Maui.Kits.Rokt package, which injects itself through
            // AndroidRoktHooks.RoktApiProvider when the app calls RoktKit.Register().
            // When the kit is not present, core falls back to a no-op bridge.
            return AndroidRoktHooks.RoktApiProvider?.Invoke() ?? new NoOpRoktApiWrapper();
        }
    }

    public override void Destroy()
    {
        mParticle.MAUI.AndroidBinding.MParticle.Instance = null;
    }

    public override void Upload()
    {
        if (!_isInitialized)
        {
            Console.WriteLine(SdkNotInitializedWarning);
            return;
        }
        mParticle.MAUI.AndroidBinding.MParticle.Instance.Upload();
    }
}

public class RoktEmbeddedViewHandler : ViewHandler<RoktEmbeddedView, global::Android.Views.View>
{
    public static IPropertyMapper<RoktEmbeddedView, RoktEmbeddedViewHandler> PropertyMapper = new PropertyMapper<RoktEmbeddedView, RoktEmbeddedViewHandler>(ViewHandler.ViewMapper)
    {
    };

    public RoktEmbeddedViewHandler() : base(PropertyMapper)
    {
    }

    protected override global::Android.Views.View CreatePlatformView()
    {
        // The concrete Rokt embedded view ships in the android-rokt-kit and is supplied by
        // the MParticle.Maui.Kits.Rokt package via AndroidRoktHooks.EmbeddedViewFactory.
        // Without the kit, core provides a neutral placeholder so the control still renders.
        var context = Platform.CurrentActivity ?? (global::Android.Content.Context)global::Android.App.Application.Context;
        return AndroidRoktHooks.EmbeddedViewFactory?.Invoke(context)
               ?? new global::Android.Widget.FrameLayout(context);
    }
}

/// <summary>
/// Injection hooks that let the optional MParticle.Maui.Kits.Rokt package provide the
/// Android Rokt implementation without core taking a compile-time dependency on the kit.
/// The kit populates these from <c>RoktKit.Register()</c>; when it is absent, core Rokt
/// APIs are no-ops and the embedded view is a neutral placeholder.
/// </summary>
public static class AndroidRoktHooks
{
    /// <summary>Factory that produces the Android <see cref="RoktApi"/> implementation.</summary>
    public static Func<RoktApi> RoktApiProvider { get; set; }

    /// <summary>Factory that produces the native Rokt embedded platform view.</summary>
    public static Func<global::Android.Content.Context, global::Android.Views.View> EmbeddedViewFactory { get; set; }
}
#endif

// No-op implementation for when SDK is not initialized
public class NoOpRoktApiWrapper : RoktApi
{
    public override void SelectPlacements(
        string identifier,
        Dictionary<string, string> attributes = null,
        Dictionary<string, RoktEmbeddedView> embeddedViews = null,
        RoktConfig config = null)
    {
        Console.WriteLine(MParticleSDK.SdkNotInitializedWarning);
    }

    public override void SelectShoppableAds(
        string identifier,
        Dictionary<string, string> attributes = null,
        RoktConfig config = null)
    {
        Console.WriteLine(MParticleSDK.SdkNotInitializedWarning);
    }

    public override void Events(string identifier, Action<RoktEvent> onEvent)
    {
        Console.WriteLine(MParticleSDK.SdkNotInitializedWarning);
    }

    public override void GlobalEvents(Action<RoktEvent> onEvent)
    {
        Console.WriteLine(MParticleSDK.SdkNotInitializedWarning);
    }
}

public class RoktEmbeddedView : Microsoft.Maui.Controls.View {}
