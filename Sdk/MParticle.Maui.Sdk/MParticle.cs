using System.Collections.Generic;
using System.Threading.Channels;
using System.Reflection;
using Microsoft.Maui.Handlers;

#if __ANDROID__
using mParticle.MAUI.Android.Wrappers;
using mParticle.MAUI.Android;
using Android.Util;
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
        if (options.LocationTracking != null && options.LocationTracking.Enabled)
        {
            mparticle.BeginLocationTracking(options.LocationTracking.MinDistance, options.LocationTracking.MinDistance);
        }
        else
        {
            mparticle.EndLocationTracking();
        }
        if (options.PushRegistration != null && options.PushRegistration.IOSToken != null)
        {
            mparticle.PushNotificationToken = options.PushRegistration.IOSToken;
        }
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

    public override void Destroy()
    {
        iOSBinding.MParticle.SharedInstance = null;
    }

    public override void Upload()
    {
        iOSBinding.MParticle.SharedInstance.Upload();
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
        mParticle.MAUI.AndroidBinding.MParticle.Instance.LeaveBreadcrumb(breadcrumbName);
    }

    public override void LogCommerceEvent(CommerceEvent commerceEvent, bool shouldUploadEvent = true)
    {
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
        var mpEventType = mParticle.MAUI.AndroidBinding.MParticle.EventType.ValueOf(eventType.ToString());
        IDictionary<string, object> castedEventInfo = eventInfo.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        var mpEvent = new mParticle.MAUI.AndroidBinding.MPEvent.Builder(eventName, mpEventType).CustomAttributes(castedEventInfo).ShouldUploadEvent(shouldUploadEvent).Build();
        mParticle.MAUI.AndroidBinding.MParticle.Instance.LogEvent(mpEvent);
    }

    public override void LogScreen(string screenName, Dictionary<string, string> eventInfo)
    {
        mParticle.MAUI.AndroidBinding.MParticle.Instance.LogScreen(screenName, eventInfo);
    }

    public override void SetATTStatus(MPATTAuthorizationStatus status, long? attStatusTimestampMillis)
    {
    }

    public override void SetOptOut(bool optOut)
    {
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

        var instance = new MParticleSDKImpl();
        if (options.IdentityStateListener != null)
        {
            instance.Identity.AddIdentityStateListener(options.IdentityStateListener);
        }

        return instance;
    }

    public override IdentityApi Identity
    {
        get
        {
            return IdentityApiWrapper.GetInstance(mParticle.MAUI.AndroidBinding.MParticle.Instance.Identity());
        }
    }

    public override void Destroy()
    {
        mParticle.MAUI.AndroidBinding.MParticle.Instance = null;
    }

    public override void Upload()
    {
        mParticle.MAUI.AndroidBinding.MParticle.Instance.Upload();
    }
}
#endif
