using System;
using Foundation;
using ObjCRuntime;

namespace mParticle.MAUI.Rokt.Payments.iOSBinding
{
    // @interface MPRoktStripePaymentExtension : NSObject
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface MPRoktStripePaymentExtension
    {
        // +(id _Nullable)createWithApplePayMerchantId:(NSString * _Nonnull)applePayMerchantId countryCode:(NSString * _Nonnull)countryCode;
        [Static]
        [Export("createWithApplePayMerchantId:countryCode:")]
        [return: NullAllowed]
        NSObject Create(string applePayMerchantId, string countryCode);
    }

    // @interface MPRokt (Payments)
    [Category]
    [BaseType(typeof(global::mParticle.MAUI.iOSBinding.MPRokt))]
    interface MPRokt_Payments
    {
        // -(void)registerPaymentExtension:(id<RoktPaymentExtension> _Nonnull)paymentExtension;
        [Export("registerPaymentExtension:")]
        void RegisterPaymentExtension(NSObject paymentExtension);
    }
}
