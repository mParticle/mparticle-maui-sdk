using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Foundation;
using mParticle.MAUI.iOSBinding;

namespace mParticle.MAUI.iOS.Utils
{
    internal static class Utils
    {
        internal static iOSBinding.MPInstallationType ConvertToMpInstallType(InstallType installType)
        {
            switch (installType)
            {
                case InstallType.AutoDetect:
                    return iOSBinding.MPInstallationType.Autodetect;
                case InstallType.KnownInstall:
                    return iOSBinding.MPInstallationType.KnownInstall;
                case InstallType.KnownUpgrade:
                    return iOSBinding.MPInstallationType.KnownInstall;
                default:
                    return iOSBinding.MPInstallationType.Autodetect;
            }
        }

        internal static iOSBinding.MPEnvironment ConvertToMpEnvironment(Environment environment)
        {
            switch (environment)
            {
                case Environment.AutoDetect:
                    return iOSBinding.MPEnvironment.AutoDetect;
                case Environment.Development:
                    return iOSBinding.MPEnvironment.Development;
                case Environment.Production:
                    return iOSBinding.MPEnvironment.Production;
                default:
                    return iOSBinding.MPEnvironment.AutoDetect;
            }
        }

        internal static iOSBinding.MPILogLevel ConvertToMpLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.INFO:
                case LogLevel.VERBOSE:
                    return iOSBinding.MPILogLevel.Verbose;
                case LogLevel.DEBUG:
                    return iOSBinding.MPILogLevel.Debug;
                case LogLevel.WARNING:
                    return iOSBinding.MPILogLevel.Warning;
                case LogLevel.ERROR:
                    return iOSBinding.MPILogLevel.Error;
                case LogLevel.NONE:
                    return iOSBinding.MPILogLevel.None;
                default:
                    if (iOSBinding.MParticle.SharedInstance.Environment.Equals(iOSBinding.MPEnvironment.Production))
                    {
                        return iOSBinding.MPILogLevel.None;
                    }
                    else
                    {
                        return iOSBinding.MPILogLevel.Debug;
                    }
            }
        }


        internal static iOSBinding.MParticleOptions ConvertToMpOptions(MParticleOptions options)
        {
            var mpOptions = new iOSBinding.MParticleOptions();

            mpOptions.InstallType = ConvertToMpInstallType(options.InstallType);
            mpOptions.Environment = ConvertToMpEnvironment(options.Environment);
            mpOptions.ApiKey = options.ApiKey;
            mpOptions.ApiSecret = options.ApiSecret;
            if (options.ConfigMaxAgeSeconds != null)
            {
                mpOptions.ConfigMaxAgeSeconds = options.ConfigMaxAgeSeconds.Value;
            }
            if (options.IdentifyRequest != null)
            {
                mpOptions.IdentifyRequest = ConvertToMpIdentityRequest(options.IdentifyRequest);
            }
            if (options.AttributionListener != null)
            {
                mpOptions.OnAttributionCompleted = ConvertToMpAttributionListener(options.AttributionListener);
            }
            if (options.IdentityStateListener != null)
            {
                mpOptions.OnIdentifyComplete = ConvertToMpIdentifyCompleteListener(options.IdentityStateListener);
            }
            return mpOptions;
        }

        internal static iOSBinding.MPProduct ConvertToMpProduct(Product product)
        {
            var bindingProduct = new iOSBinding.MPProduct();
            bindingProduct.Sku = product.Sku;
            bindingProduct.Name = product.Name;
            bindingProduct.Price = new NSNumber(product.Price);
            bindingProduct.Quantity = new NSNumber(product.Quantity);
            bindingProduct.Brand = product.Brand;
            bindingProduct.Category = product.Category;
            bindingProduct.CouponCode = product.CouponCode;
            bindingProduct.Variant = product.Variant;

            if (product.customAttributes != null)
            {
                foreach (var kvp in product.customAttributes)
                {
                    bindingProduct.SetObject((NSObject)new NSString(kvp.Value), new NSString(kvp.Key));
                }
            }
            return bindingProduct;
        }

        internal static Product ConvertToXamProduct(iOSBinding.MPProduct product)
        {
            {
                if (product == null)
                {
                    return null;
                }
                var customKeys = product.AllKeys.ToDictionary(key => key.ToString(), key => product.ObjectForKeyedSubscript(key.ToString()).ToString());
                return new Product(product.Name, product.Sku, product.Price?.DoubleValue ?? 0, product.Quantity.DoubleValue)
                {
                    Brand = product.Brand,
                    CouponCode = product.CouponCode,
                    Position = Convert.ToInt32(product.Position),
                    Category = product.Category,
                    Variant = product.Variant,
                    customAttributes = customKeys
                };
            }
        }

        internal static iOSBinding.MPIdentityApiRequest ConvertToMpIdentityRequest(IdentityApiRequest request)
        {
            if (request == null)
            {
                return null;
            }
            var mpRequest = new iOSBinding.MPIdentityApiRequest();
            if (request.UserIdentities != null)
            {
                request.UserIdentities.ToList().ForEach(pair =>
                {
                    mpRequest.SetIdentity(new NSString(pair.Value), (MPUserIdentity)pair.Key);

                });
            }
            return mpRequest;
        }

        internal static MPTransactionAttributes ConvertToMpTransactionAttributes(TransactionAttributes transactionAttributes)
        {
            var bindingTransactions = new MPTransactionAttributes();
            bindingTransactions.TransactionId = transactionAttributes.TransactionId;
            bindingTransactions.Affiliation = transactionAttributes.Affiliation;
            bindingTransactions.CouponCode = transactionAttributes.CouponCode;
            bindingTransactions.Shipping = transactionAttributes.Shipping.HasValue ? transactionAttributes.Shipping.Value : 0;
            bindingTransactions.Tax = transactionAttributes.Tax.HasValue ? transactionAttributes.Tax.Value : 0;
            bindingTransactions.Revenue = transactionAttributes.Revenue.HasValue ? transactionAttributes.Revenue.Value : 0;
            return bindingTransactions;
        }

        internal static iOSBinding.OnAttributionCompleted ConvertToMpAttributionListener(AttributionListener attributionListener)
        {
            return new iOSBinding.OnAttributionCompleted((attributionResult, nsError) =>
            {
                if (attributionResult != null && attributionListener.OnAttributionResult != null)
                {
                    attributionListener.OnAttributionResult(new AttributionResult()
                    {
                        //TODO 
                        //is this correct??
                        Parameters = attributionResult.KitName,
                        ServiceProviderId = attributionResult.KitCode.Int32Value,
                        LinkUrl = attributionResult.LinkInfo.Description
                    });
                }
                if (nsError != null && attributionListener.OnAttributionError != null)
                {
                    attributionListener.OnAttributionError(new AttributionError()
                    {
                        Message = nsError.ToString(),
                        ServiceProviderId = attributionResult.KitCode == null ? attributionResult.KitCode.Int32Value : 0
                    });
                }
            });
        }

        internal static iOSBinding.OnIdentifyComplete ConvertToMpIdentifyCompleteListener(OnUserIdentified identityListener)
        {
            return new iOSBinding.OnIdentifyComplete((request, error) =>
            {
                if (identityListener != null)
                {
                    if (request != null && request.User != null)
                    {
                        identityListener.Invoke(new MParticleUserWrapper(request.User));
                    }
                    else if (error != null)
                    {
                        // Log the error for debugging
                        Console.WriteLine($"mParticle Identity Error: {error.LocalizedDescription} (Code: {error.Code})");
                        
                        // Only invoke with null if this is a recoverable error
                        // For authentication errors (401), we might want to handle differently
                        if (error.Code == 401)
                        {
                            Console.WriteLine("mParticle Authentication failed. Please check your API key and secret.");
                        }
                        
                        identityListener.Invoke(null);
                    }
                }
            });
        }

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
                var cacheAttributes = ConvertToNSDictionary<NSString, NSString>(config.CacheAttributes);
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
                        attributes => e.SetFulfillmentAttributes?.Invoke(ConvertToNSDictionary<NSString, NSString>(attributes)));
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

        internal static NSDictionary<NSString, NSString> ConvertToNSDictionary<T, V>(Dictionary<string, string> dictionary) where T : NSString where V : NSString
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

            // Filter to only include views that have valid platform handlers and views
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
            
            // Create NSDictionary with native RoktEmbeddedView instances
            var keys = filteredViews.Keys.ToArray();
            var values = filteredViews.Values.ToArray();
            
            var nativeDictionary = NSDictionary.FromObjectsAndKeys(
                values.Cast<object>().ToArray(),
                keys.Cast<object>().ToArray()
            );
            
            return nativeDictionary;
        }
    }
}
