using mParticle.MAUI.Android.Wrappers;
using Application = Android.App.Application;

namespace mParticle.MAUI.Android;

internal static class Utils
{
    internal static string ConvertToMpProductAction(ProductAction action)
    {
        switch (action)
        {
            case ProductAction.AddToCart:
                return "add_to_cart";
            case ProductAction.AddToWishlist:
                return "add_to_wishlist";
            case ProductAction.Checkout:
                return "checkout";
            case ProductAction.CheckoutOption:
                return "checkout_option";
            case ProductAction.Click:
                return "click";
            case ProductAction.Purchase:
                return "purchase";
            case ProductAction.Refund:
                return "refund";
            case ProductAction.RemoveFromWishlist:
                return "remove_from_wishlist";
            case ProductAction.RemoveFromCart:
                return "remove_from_cart";
            case ProductAction.ViewDetail:
                return "view_detail";
            default:
                return null;
        }
    }

    internal static string ConvertToMpPromotionAction(PromotionAction action)
    {
        switch (action)
        {
            case PromotionAction.Click:
                return "click";
            case PromotionAction.View:
                return "view";
            default:
                return null;
        }
    }

    internal static Android.CommerceBinding.Product ConvertToMpProduct(Product product)
    {
        var builder = new Android.CommerceBinding.Product
            .Builder(product.Name, product.Sku, product.Price)
            .CustomAttributes(product.customAttributes)
            .Category(product.Category)
            .CouponCode(product.CouponCode)
            .Quantity(product.Quantity)
            .Brand(product.Brand)
            .Variant(product.Variant);


        if (product.Position.HasValue)
            builder = builder.Position(new Java.Lang.Integer(product.Position.Value));

        return builder.Build();
    }

    internal static Android.CommerceBinding.Promotion ConvertToMpPromotion(Promotion promotion)
    {
        var bindingPromotion = new Android.CommerceBinding.Promotion();
        bindingPromotion.SetCreative(promotion.Creative);
        bindingPromotion.SetId(promotion.Id);
        bindingPromotion.SetName(promotion.Name);

        if (promotion.Position.HasValue)
            bindingPromotion.SetPosition(promotion.Position.Value.ToString());

        return bindingPromotion;
    }


    internal static Android.CommerceBinding.Impression ConvertToMpImpression(Impression impression)
    {
        var bindingImpression = new Android.CommerceBinding.Impression(impression.ImpressionListName, null);
        if (impression.Products != null)
        {
            foreach (var product in impression.Products)
            {
                bindingImpression.AddProduct(ConvertToMpProduct(product));
            }
        }

        return bindingImpression;
    }

    internal static Android.CommerceBinding.TransactionAttributes ConvertToMpTransactionAttributes(TransactionAttributes attributes)
    {
        var bindingTransaction = new Android.CommerceBinding.TransactionAttributes(attributes.TransactionId);
        bindingTransaction.SetCouponCode(attributes.CouponCode);
        bindingTransaction.SetAffiliation(attributes.Affiliation);

        if (attributes.Tax.HasValue)
            bindingTransaction.SetTax(new Java.Lang.Double(attributes.Tax.Value));

        if (attributes.Shipping.HasValue)
            bindingTransaction.SetShipping(new Java.Lang.Double(attributes.Shipping.Value));

        if (attributes.Revenue.HasValue)
            bindingTransaction.SetRevenue(new Java.Lang.Double(attributes.Revenue.Value));

        return bindingTransaction;
    }

    internal static AndroidBinding.MParticle.InstallType ConvertToMpInstallType(InstallType installType)
    {
        switch (installType)
        {
            case InstallType.AutoDetect:
                return AndroidBinding.MParticle.InstallType.AutoDetect;
            case InstallType.KnownInstall:
                return AndroidBinding.MParticle.InstallType.KnownInstall;
            case InstallType.KnownUpgrade:
                return AndroidBinding.MParticle.InstallType.KnownUpgrade;
            default:
                return AndroidBinding.MParticle.InstallType.AutoDetect;
        }
    }

    internal static AndroidBinding.MParticle.Environment ConvertToMpEnvironment(Environment environment)
    {
        switch (environment)
        {
            case Environment.AutoDetect:
                return AndroidBinding.MParticle.Environment.AutoDetect;
            case Environment.Development:
                return AndroidBinding.MParticle.Environment.Development;
            case Environment.Production:
                return AndroidBinding.MParticle.Environment.Production;
            default:
                return AndroidBinding.MParticle.Environment.AutoDetect;
        }
    }

    internal static AndroidBinding.IAttributionListener ConvertToMpAttributionListener(AttributionListener attributionListener)
    {
        return new AttributionListenerWrapper(attributionListener);
    }

    internal static AndroidBinding.AttributionError ConvertToMpAttributionError(AttributionError attributionError)
    {
        var bindingAttributionError = new AndroidBinding.AttributionError();
        return bindingAttributionError.SetMessage(attributionError.Message)
                               .SetServiceProviderId(attributionError.ServiceProviderId);
    }

    internal static IdentityBinding.IdentityApiRequest ConvertToMpIdentityRequest(IdentityApiRequest request)
    {
        if (request == null)
        {
            return null;
        }
        Android.IdentityBinding.IdentityApiRequest.Builder builder = Android.IdentityBinding.IdentityApiRequest.WithEmptyUser();
        if (request.UserIdentities != null)
        {
            foreach (var identityType in request.UserIdentities.Keys)
            {
                foreach (var androidIdentityType in AndroidBinding.MParticle.IdentityType.Values())
                {
                    if (identityType.ToString().Equals(androidIdentityType.Name()))
                    {
                        builder.UserIdentity(androidIdentityType, request.UserIdentities.GetValueOrDefault(identityType));
                    }
                }
            }
        }
        if (request.UserAliasHandler != null)
        {
            builder.UserAliasHandler(new UserAliasHandlerWrapper(request.UserAliasHandler));
        }
        return builder.Build();
    }

    internal static AndroidBinding.MParticleOptions ConvertToMpOptions(MParticleOptions options)
    {
        var builder = AndroidBinding.MParticleOptions.InvokeBuilder(Application.Context);
        builder.AndroidIdDisabled(options.IdDisabled);

        if (options.AttributionListener != null)
        {
            builder.AttributionListener(ConvertToMpAttributionListener(options.AttributionListener));
        }
        builder.InstallType(ConvertToMpInstallType(options.InstallType));
        builder.Environment(ConvertToMpEnvironment(options.Environment));
        builder.Credentials(options.ApiKey, options.ApiSecret);
        if (options.IdentifyRequest != null)
        {
            builder.Identify(ConvertToMpIdentityRequest(options.IdentifyRequest));
        }
        if (options.LocationTracking != null)
        {
            if (options.LocationTracking.Enabled)
            {
                builder.LocationTrackingEnabled(options.LocationTracking.Provider, options.LocationTracking.MinTime, options.LocationTracking.MinDistance);
            }
            else
            {
                builder.LocationTrackingDisabled();
            }
        }
        if (options.PushRegistration != null && options.PushRegistration.AndroidSenderId != null && options.PushRegistration.AndroidInstanceId != null)
        {
            builder.PushRegistration(options.PushRegistration.AndroidInstanceId, options.PushRegistration.AndroidSenderId);
        }
        builder.EnableUncaughtExceptionLogging(options.UnCaughtExceptionLogging);
        if (options.ConfigMaxAgeSeconds != null)
        {
            builder.ConfigMaxAgeSeconds(options.ConfigMaxAgeSeconds.Value);
        }
        return builder.Build();
    }

    internal static Product ConvertToXamProduct(CommerceBinding.Product product)
    {
        if (product == null)
        {
            return null;
        }
        return new Product(product.Name, product.Sku, product.UnitPrice, product.Quantity)
        {
            Brand = product.Brand,
            CouponCode = product.CouponCode,
            Position = product.Position.IntValue(),
            Category = product.Category,
            Variant = product.Variant,
            customAttributes = new Dictionary<string, string>(product.CustomAttributes)
        };
    }

    internal static Java.Util.IMap ConvertToJavaMap(Dictionary<string, string> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
            return null;

        var javaMap = new Java.Util.HashMap();
        foreach (var kvp in dictionary)
        {
            javaMap.Put(kvp.Key, kvp.Value);
        }
        return javaMap;
    }

    // New methods for the non-obfuscated AAR that return the correct types
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
            Com.Mparticle.Rokt.RoktEmbeddedView androidEmbeddedView = null;
            
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

        if (config.CacheDuration.HasValue || (config.CacheAttributes != null && config.CacheAttributes.Count > 0))
        {
            var cacheDuration = config.CacheDuration ?? Com.Mparticle.Rokt.CacheConfig.DefaultCacheDurationSecs;
            var cacheConfig = new Com.Mparticle.Rokt.CacheConfig(cacheDuration, config.CacheAttributes);
            builder.CacheConfig(cacheConfig);
        }

        return builder.Build();
    }

    internal static AndroidBinding.IMpRoktEventCallback ConvertToRoktEventCallback(RoktEventCallback callbacks)
    {
        return ConvertToRoktEventCallback(callbacks, null);
    }

    internal static AndroidBinding.IMpRoktEventCallback ConvertToRoktEventCallback(
        RoktEventCallback callbacks, 
        Action<string, double> heightCallback)
    {
        return new RoktEventCallbackWrapper(callbacks, heightCallback);
    }

    public class RoktEventCallbackWrapper : Java.Lang.Object, AndroidBinding.IMpRoktEventCallback
    {
        private readonly RoktEventCallback _callbacks;
        private readonly Action<string, double> _heightCallback;

        public RoktEventCallbackWrapper(RoktEventCallback callbacks, Action<string, double> heightCallback = null)
        {
            _callbacks = callbacks;
            _heightCallback = heightCallback;
        }

        public void OnLoad()
        {
            _callbacks?.OnLoad?.Invoke();
        }

        public void OnUnload(AndroidBinding.UnloadReasons reason)
        {
            _callbacks?.OnUnLoad?.Invoke(reason?.ToString() ?? "Unknown");
        }

        public void OnShouldShowLoadingIndicator()
        {
            _callbacks?.OnShouldShowLoadingIndicator?.Invoke();
        }

        public void OnShouldHideLoadingIndicator()
        {
            _callbacks?.OnShouldHideLoadingIndicator?.Invoke();
        }

        public void OnEmbeddedSizeChange(string identifier, int height)
        {
            // Call user's callback if provided
            _callbacks?.OnEmbeddedSizeChange?.Invoke(identifier, (float)height);
            
            // Call internal height management callback
            _heightCallback?.Invoke(identifier, height);
        }
    }

    public class AttributionListenerWrapper : Java.Lang.Object, AndroidBinding.IAttributionListener
    {
        AttributionListener _attributionListener;

        public AttributionListenerWrapper(AttributionListener attributionListener)
        {
            this._attributionListener = attributionListener;
        }

        public void OnError(AndroidBinding.AttributionError attributionError)
        {
            if (_attributionListener.OnAttributionError != null)
            {
                _attributionListener.OnAttributionError.Invoke(new AttributionError()
                {
                    ServiceProviderId = attributionError.ServiceProviderId,
                    Message = attributionError.Message
                });
            }
        }

        public void OnResult(AndroidBinding.AttributionResult attributionResult)
        {
            if (_attributionListener.OnAttributionResult != null)
            {
                _attributionListener.OnAttributionResult.Invoke(new AttributionResult()
                {
                    ServiceProviderId = attributionResult.ServiceProviderId,
                    Parameters = attributionResult.Parameters.ToString(),
                    LinkUrl = attributionResult.Link
                });
            }
        }
    }
}
