plugins {
    alias(libs.plugins.android.library)
    alias(libs.plugins.jetbrains.kotlin.android)
}

android {
    namespace = "com.mparticle.mparticleroktbinding"
    compileSdk = 34

    defaultConfig {
        minSdk = 21

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro",
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }
    kotlinOptions {
        jvmTarget = "1.8"
    }
}

configurations {
    create("copyDependencies")
}

dependencies {
    implementation(libs.mparticle.kit.rokt)
    implementation(libs.coroutines)
    implementation(libs.kotlin.stdlib)
    implementation(libs.androidx.lifecycle.common.jvm)
    implementation(libs.androidx.lifecycle.runtime.android)
    "copyDependencies"(libs.mparticle.kit.rokt)
}

project.afterEvaluate {
    tasks.register<Copy>("copyDeps") {
        rootSpec.eachFile {
            if (this.name.contains(".aar")) {
                // Rename mparticle AAR to the expected name
                if (this.name.contains("android-rokt-kit")) {
                    this.name = "com.mparticle-android-rokt-kit-5.74.3.aar"
                } else {
                    val groupName = this.file.parentFile.parentFile.parentFile.parentFile.name
                    this.name = groupName + "-" + this.name
                }
            }
        }
        from(configurations["copyDependencies"])
        into("$buildDir/outputs/deps")

        // Set duplicate handling strategy
        duplicatesStrategy = DuplicatesStrategy.FAIL
    }
    tasks.named("preBuild") { finalizedBy("copyDeps") }
}
