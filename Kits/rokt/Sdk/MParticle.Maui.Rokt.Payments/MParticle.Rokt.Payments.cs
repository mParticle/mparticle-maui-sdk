namespace mParticle.MAUI.Rokt.Payments
{
#if __IOS__
    using global::mParticle.MAUI.Rokt.Payments.iOSBinding;
    using PaymentsBinding = global::mParticle.MAUI.Rokt.Payments.iOSBinding;
    using CoreBinding = global::mParticle.MAUI.iOSBinding;
#endif

    /// <summary>
    /// Registers the Rokt Stripe / Apple Pay payment extension with the mParticle Rokt kit.
    /// Requires the core <c>mParticle.Maui.Kits.Rokt</c> package and the
    /// <c>com.apple.developer.in-app-payments</c> entitlement (with your Apple Pay
    /// merchant identifier) on iOS.
    /// </summary>
    public static class RoktStripePaymentExtension
    {
        /// <summary>
        /// Creates a native Rokt Stripe payment extension and registers it with the
        /// Rokt kit. Call once, after <c>MParticle.Instance.Initialize(options)</c>.
        /// On Android this is a no-op and returns <c>false</c>.
        /// </summary>
        /// <param name="applePayMerchantId">Apple Pay merchant identifier (e.g. <c>merchant.com.yourapp.rokt</c>).</param>
        /// <param name="countryCode">ISO country code. Defaults to <c>US</c>.</param>
        /// <returns><c>true</c> if the extension was registered, <c>false</c> otherwise.</returns>
        public static bool Register(string applePayMerchantId, string countryCode = "US")
        {
#if __IOS__
            var ext = new PaymentsBinding.MPRoktStripePaymentExtension(applePayMerchantId, countryCode);
            var rokt = CoreBinding.MParticle.SharedInstance?.Rokt;
            if (rokt == null)
            {
                return false;
            }
            rokt.RegisterPaymentExtension(ext);
            return true;
#else
            return false;
#endif
        }
    }
}
