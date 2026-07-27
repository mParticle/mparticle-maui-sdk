using mParticle.MAUI;

namespace mParticle.MAUI.Rokt;

/// <summary>
/// Adds the Rokt entry point to the mParticle SDK. The <c>Rokt</c> accessor only exists
/// when the <c>mParticle.Maui.Kits.Rokt</c> package is referenced; the core SDK does not
/// expose any Rokt surface on its own.
/// </summary>
public static class MParticleRoktExtensions
{
    extension(MParticleSDK sdk)
    {
        /// <summary>
        /// Returns an implementation of the Rokt API bound to the current mParticle instance.
        /// </summary>
        public RoktApi Rokt
        {
            get
            {
                if (sdk == null)
                {
                    return new NoOpRoktApi();
                }
#if __IOS__
                var native = sdk.GetBindingInstance() as mParticle.MAUI.iOSBinding.MParticle;
                var rokt = native?.Rokt;
                return rokt == null ? new NoOpRoktApi() : new IosRoktApi(rokt);
#elif __ANDROID__
                var native = sdk.GetBindingInstance() as mParticle.MAUI.AndroidBinding.MParticle;
                return native == null ? new NoOpRoktApi() : new AndroidRoktApi(native);
#else
                return new NoOpRoktApi();
#endif
            }
        }
    }
}
