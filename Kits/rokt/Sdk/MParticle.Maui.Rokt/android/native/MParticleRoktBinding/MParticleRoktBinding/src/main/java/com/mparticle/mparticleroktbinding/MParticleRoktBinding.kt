package com.mparticle.mparticleroktbinding

import com.mparticle.MParticle
import com.mparticle.kits.RoktEmbeddedView
import com.mparticle.kits.rokt
import com.rokt.roktsdk.CacheConfig
import com.rokt.roktsdk.RoktConfig
import com.rokt.roktsdk.RoktEvent
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.launch
import java.lang.ref.WeakReference

/**
 * Receives Rokt events flattened into a primitive string map so the .NET binding does not
 * need to bind the Kotlin Flow / roktsdk contract types directly.
 */
fun interface RoktEventCallback {
    fun onEvent(params: Map<String, String>)
}

/**
 * Thin Java-friendly bridge over the mParticle Android Rokt kit (mParticle Android SDK 6).
 * The Rokt API and its Kotlin Flow / roktsdk types stay on the Kotlin side; the .NET Rokt kit
 * only binds the plain static methods below plus [RoktEmbeddedView].
 */
object MParticleRoktBinding {
    /**
     * @param colorMode "LIGHT" | "DARK" | "SYSTEM" (case-insensitive), or null when the caller
     *   passes no RoktConfig (the kit then applies its own default configuration).
     * @param cacheDurationSeconds cache duration in seconds; values <= 0 mean "not set".
     * @param cacheAttributes optional cache attributes; when non-empty a CacheConfig is applied.
     */
    @JvmStatic
    @JvmOverloads
    fun selectPlacements(
        identifier: String,
        attributes: Map<String, String>?,
        embeddedViews: Map<String, RoktEmbeddedView>?,
        colorMode: String? = null,
        cacheDurationSeconds: Long = 0,
        cacheAttributes: Map<String, String>? = null,
    ) {
        val rokt = MParticle.getInstance()?.rokt ?: return
        val safeAttributes = attributes ?: emptyMap()
        val views = embeddedViews?.mapValues { WeakReference(it.value) }
        val config = buildConfig(colorMode, cacheDurationSeconds, cacheAttributes)

        if (config == null) {
            // No RoktConfig supplied: preserve prior behavior and let the kit apply its default.
            if (views != null) {
                rokt.selectPlacements(identifier, safeAttributes, views)
            } else {
                rokt.selectPlacements(identifier, safeAttributes)
            }
        } else {
            rokt.selectPlacements(identifier, safeAttributes, views ?: emptyMap(), emptyMap(), config)
        }
    }

    private fun buildConfig(
        colorMode: String?,
        cacheDurationSeconds: Long,
        cacheAttributes: Map<String, String>?,
    ): RoktConfig? {
        if (colorMode == null && cacheDurationSeconds <= 0L && cacheAttributes.isNullOrEmpty()) {
            return null
        }

        val builder = RoktConfig.Builder()
        builder.colorMode(
            when (colorMode?.uppercase()) {
                "LIGHT" -> RoktConfig.ColorMode.LIGHT
                "DARK" -> RoktConfig.ColorMode.DARK
                else -> RoktConfig.ColorMode.SYSTEM
            },
        )

        if (cacheDurationSeconds > 0L || !cacheAttributes.isNullOrEmpty()) {
            val duration = if (cacheDurationSeconds > 0L) cacheDurationSeconds else CacheConfig.DEFAULT_CACHE_DURATION_SECS
            builder.cacheConfig(CacheConfig(duration, cacheAttributes ?: emptyMap()))
        }

        return builder.build()
    }

    @JvmStatic
    fun subscribeToEvents(
        identifier: String,
        callback: RoktEventCallback,
    ): Job? {
        val rokt = MParticle.getInstance()?.rokt ?: return null
        return CoroutineScope(Dispatchers.Main.immediate).launch {
            rokt.events(identifier).collect { event ->
                callback.onEvent(flatten(event))
            }
        }
    }

    private fun flatten(event: RoktEvent): Map<String, String> {
        val params = HashMap<String, String>()
        params["event"] = event::class.simpleName ?: "RoktEvent"
        placementId(event)?.let { params["placementId"] = it }
        when (event) {
            is RoktEvent.InitComplete -> params["status"] = event.success.toString()
            is RoktEvent.OpenUrl -> params["url"] = event.url
            is RoktEvent.CartItemInstantPurchase -> {
                params["cartItemId"] = event.cartItemId
                params["catalogItemId"] = event.catalogItemId
                params["currency"] = event.currency
                params["description"] = event.description
                params["linkedProductId"] = event.linkedProductId
                params["totalPrice"] = event.totalPrice.toString()
                params["quantity"] = event.quantity.toString()
                params["unitPrice"] = event.unitPrice.toString()
            }
            else -> {
                // No extra parameters for the remaining event types.
            }
        }
        return params
    }

    private fun placementId(event: RoktEvent): String? =
        when (event) {
            is RoktEvent.FirstPositiveEngagement -> event.identifier
            is RoktEvent.OfferEngagement -> event.identifier
            is RoktEvent.PlacementClosed -> event.identifier
            is RoktEvent.PlacementCompleted -> event.identifier
            is RoktEvent.PlacementFailure -> event.identifier
            is RoktEvent.PlacementInteractive -> event.identifier
            is RoktEvent.PlacementReady -> event.identifier
            is RoktEvent.PositiveEngagement -> event.identifier
            is RoktEvent.OpenUrl -> event.identifier
            is RoktEvent.CartItemInstantPurchase -> event.identifier
            else -> null
        }
}
