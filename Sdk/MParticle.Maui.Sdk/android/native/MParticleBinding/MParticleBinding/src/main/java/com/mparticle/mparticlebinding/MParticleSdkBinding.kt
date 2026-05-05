package com.mparticle.mparticlebinding

import com.mparticle.Rokt
import com.mparticle.RoktEvent
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.collect
import kotlinx.coroutines.launch

fun interface RoktFlowEventListener {
    fun onEvent(event: RoktEvent)
}

class MParticleSdkBinding {
    companion object {
        @JvmStatic
        fun subscribeToEvents(
            rokt: Rokt,
            identifier: String,
            listener: RoktFlowEventListener,
        ): Job =
            CoroutineScope(Dispatchers.Main.immediate).launch {
                rokt.events(identifier).collect { event ->
                    listener.onEvent(event)
                }
            }
    }
}
