namespace mParticle.MAUI
{
    /// <summary>
    /// Provides kit registration for the Rokt mParticle integration.
    /// </summary>
    public static class RoktKit
    {
        /// <summary>
        /// Registers the Rokt kit with mParticle.
        /// This method should be called before initializing mParticle.
        /// On iOS, this replicates the +load method behavior for kit registration.
        /// On Android, this is a no-op as kit registration happens automatically.
        /// </summary>
        public static void Register()
        {
            #if __IOS__
                var kitRegister = new mParticle.MAUI.iOSBinding.MPKitRegister("Rokt", "MPKitRokt");
                mParticle.MAUI.iOSBinding.MParticle.RegisterExtension(kitRegister);
                System.Console.WriteLine("MPKitRokt registered as extension with mParticle");
            #elif __ANDROID__
                // On Android the Rokt API moved out of android-core into the optional
                // android-rokt-kit (mParticle Android SDK 6). Inject the kit-backed Rokt
                // implementation and embedded-view factory into core's hooks so that
                // MParticle.Instance.Rokt and the RoktEmbeddedView handler route to the kit.
                AndroidRoktHooks.RoktApiProvider = () => new mParticle.MAUI.Rokt.Android.AndroidRoktApiWrapper();
                AndroidRoktHooks.EmbeddedViewFactory = context => new Com.Mparticle.Kits.RoktEmbeddedView(context);
                System.Console.WriteLine("MPKitRokt registered as extension with mParticle");
            #endif
        }
    }
}
