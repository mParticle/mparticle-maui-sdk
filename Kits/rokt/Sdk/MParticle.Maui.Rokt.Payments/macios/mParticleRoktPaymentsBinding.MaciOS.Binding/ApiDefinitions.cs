using System;
using Foundation;
using ObjCRuntime;
using mParticle.MAUI.iOSBinding;

namespace mParticle.MAUI.Rokt.Payments.iOSBinding
{
    // @interface MPRoktStripePaymentExtension : NSObject <RoktPaymentExtension>
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface MPRoktStripePaymentExtension
    {
        // -(instancetype _Nullable)initWithApplePayMerchantId:(NSString * _Nonnull)applePayMerchantId countryCode:(NSString * _Nonnull)countryCode;
        [Export("initWithApplePayMerchantId:countryCode:")]
        NativeHandle Constructor(string applePayMerchantId, string countryCode);
    }

    // @interface MPRokt (Payments)
    [Category]
    [BaseType(typeof(MPRokt))]
    interface MPRokt_Payments
    {
        // -(void)registerPaymentExtension:(id<RoktPaymentExtension> _Nonnull)paymentExtension;
        [Export("registerPaymentExtension:")]
        void RegisterPaymentExtension(NSObject paymentExtension);
    }
}
 