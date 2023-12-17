using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Solstice
{
    [HarmonyPatch(typeof(TimeWidget), nameof(TimeWidget.Start))]
    internal class TimeWidget_Start
    {
        internal static void Postfix(TimeWidget __instance)
        {
            TimeWidgetUpdater.Initialize(__instance);
        }
    }

    [HarmonyPatch(typeof(TimeWidget), nameof(TimeWidget.UpdateIconPositions))]
    internal class TimeWidget_UpdateIconPositions
    {
        internal static bool Prefix(TimeWidget __instance, float angleDegrees)
        {
            if (!Solstice.Enabled) return true;


            //Make the icon transition during polar days cleaner
            float normalizedTime = GameManager.GetUniStorm().m_NormalizedTime;
            if (normalizedTime < 0.0000001f || normalizedTime > 0.9999999f)
            {
                return false;
            }

            TimeWidgetUpdater.Update(__instance, angleDegrees);
            return false;
        }
    }

    [HarmonyPatch(typeof(UniStormWeatherSystem), nameof(UniStormWeatherSystem.Init))]
    internal class UniStormWeatherSystem_Init
    {
        internal static void Postfix(UniStormWeatherSystem __instance)
        {
            Solstice.BlizzardDawnColors = __instance.m_WeatherStateConfigs[0].m_DawnColors;
            Solstice.ClearDawnColors = __instance.m_WeatherStateConfigs[1].m_DawnColors; ;
            Solstice.ClearAuroraDawnColors = __instance.m_WeatherStateConfigs[2].m_DawnColors; ;
            Solstice.CloudyDawnColors = __instance.m_WeatherStateConfigs[3].m_DawnColors; ;
            Solstice.DenseFogDawnColors = __instance.m_WeatherStateConfigs[4].m_DawnColors; ;
            Solstice.HeavySnowDawnColors = __instance.m_WeatherStateConfigs[5].m_DawnColors; ;
            Solstice.LightFogDawnColors = __instance.m_WeatherStateConfigs[6].m_DawnColors; ;
            Solstice.LightSnowDawnColors = __instance.m_WeatherStateConfigs[7].m_DawnColors; ;
            Solstice.PartlyCloudyDawnColors = __instance.m_WeatherStateConfigs[8].m_DawnColors; ;
            Solstice.ToxicFogDawnColors = __instance.m_WeatherStateConfigs[9].m_DawnColors; ;
            Solstice.ElectrostaticFogDawnColors = __instance.m_WeatherStateConfigs[10].m_DawnColors; ;

            Solstice.BlizzardAfternoonColors = __instance.m_WeatherStateConfigs[0].m_AfternoonColors; ;
            Solstice.ClearAfternoonColors = __instance.m_WeatherStateConfigs[1].m_AfternoonColors; ;
            Solstice.ClearAuroraAfternoonColors = __instance.m_WeatherStateConfigs[2].m_AfternoonColors; ;
            Solstice.CloudyAfternoonColors = __instance.m_WeatherStateConfigs[3].m_AfternoonColors; ;
            Solstice.DenseFogAfternoonColors = __instance.m_WeatherStateConfigs[4].m_AfternoonColors; ;
            Solstice.HeavySnowAfternoonColors = __instance.m_WeatherStateConfigs[5].m_AfternoonColors; ;
            Solstice.LightFogAfternoonColors = __instance.m_WeatherStateConfigs[6].m_AfternoonColors; ;
            Solstice.LightSnowAfternoonColors = __instance.m_WeatherStateConfigs[7].m_AfternoonColors; ;
            Solstice.PartlyCloudyAfternoonColors = __instance.m_WeatherStateConfigs[8].m_AfternoonColors; ;
            Solstice.ToxicFogAfternoonColors = __instance.m_WeatherStateConfigs[9].m_AfternoonColors; ;
            Solstice.ElectrostaticFogAfternoonColors = __instance.m_WeatherStateConfigs[10].m_AfternoonColors; ;

            Solstice.Init(__instance);
        }
    }

    // SetMoonPhase is called everyday at NOON
    [HarmonyPatch(typeof(UniStormWeatherSystem), nameof(UniStormWeatherSystem.SetMoonPhase))]
    internal class UniStormWeatherSystem_SetMoonPhase
    {
        internal static void Prefix(UniStormWeatherSystem __instance)
        {
            if (!Solstice.Enabled) return;
            Solstice.Update(__instance);
        }
    }

    [HarmonyPatch(typeof(UniStormWeatherSystem), nameof(UniStormWeatherSystem.UpdateSunTransform))]
    internal class UniStormWeatherSystem_UpdateSunTransform
    {
        internal static bool Prefix(UniStormWeatherSystem __instance)
        {
            if (!Solstice.Enabled) return true;

            //MelonLogger.Msg($"Latitude: {Solstice.Latitude}");
            //MelonLogger.Msg($"Sun Angle: {__instance.m_SunAngle}");

            float lat = Solstice.Latitude;

            //Decrease the sun angle by 10 because we can sometimes see it at night
            float sunAngle = __instance.m_SunAngle - 10;

            Transform transform = __instance.m_SunLight.transform;
            Vector3 suntoearth = new Vector3(0, 0, 1f);
            float zenith = sunAngle;
            suntoearth = Quaternion.Euler(zenith, 0, 0) * suntoearth;

            transform.forward = suntoearth;

            Vector3 earth_axis = new Vector3(0, 0, 1);

            earth_axis = Quaternion.Euler(-lat, 0, 0) * earth_axis;


            //MelonLogger.Msg($"Transform Forward: {transform.forward}");
            //MelonLogger.Msg($"Earth Axis: {earth_axis}");

            transform.Rotate(earth_axis, __instance.m_NormalizedTime * 360f - 180f, Space.World);
            //MelonLogger.Msg(__instance.m_NormalizedTime + " | angle : " + (__instance.m_NormalizedTime * 360f - 180f));
            //MelonLogger.Msg($"Final Transform Forward: {transform.forward}");

            return false;
        }
    }

    // THOSE 5 METHODS ARE NEVER CALLED FOR SOME REASONS....
    //[HarmonyPatch(typeof(Weather), nameof(Weather.GenerateTempHigh))]
    //[HarmonyPatch(typeof(Weather), nameof(Weather.GenerateTempLow))]
    //[HarmonyPatch(typeof(Weather), nameof(Weather.SetTempHigh))]
    //[HarmonyPatch(typeof(Weather), nameof(Weather.SetTempLow))]
    //[HarmonyPatch(typeof(Weather), nameof(Weather.UpdateUniStormTemperature))]

    [HarmonyPatch(typeof(Weather), nameof(Weather.Update))]
    internal class Weather_Update
    {
        internal static void Prefix(Weather __instance)
        {
            if (!Solstice.Enabled) return;

            if (__instance.m_TempLow != Solstice.TempLow)
            {
                GameManager.GetWeatherComponent().GenerateTempLow();
                __instance.m_TempLow += Solstice.TemperatureOffset;
                Solstice.TempLow = __instance.m_TempLow;
            }
            if (__instance.m_TempHigh != Solstice.TempHigh)
            {
                GameManager.GetWeatherComponent().GenerateTempHigh();
                __instance.m_TempHigh += Solstice.TemperatureOffset;
                Solstice.TempHigh = __instance.m_TempHigh;
            }
            //MelonLogger.Msg($"Weather.Update() m_TempLow : {__instance.m_TempLow} --- m_TempHigh : {__instance.m_TempHigh} --- TemperatureOffset : {Solstice.TemperatureOffset}");
        }
    }

    [HarmonyPatch(typeof(PlayerVoice), nameof(PlayerVoice.Queue), new Type[] { typeof(Il2CppAK.Wwise.Event), typeof(Il2CppVoice.Priority), typeof(Il2CppSystem.Action) })]
    internal class PlayerVoice_PlayDelayedVoiceOver_patch
    {
        internal static bool Prefix(PlayerVoice __instance, Il2CppAK.Wwise.Event audioEvent, Il2CppVoice.Priority priority, Il2CppSystem.Action completionCallback)
        {
            //MelonLogger.Msg($"Audio Event: {audioEvent.Name}, Priority: {priority}, Completion Callback: {completionCallback}");
            float sunrise = GameManager.GetUniStorm().m_TODKeyframeTimes[0];
            float sunset = GameManager.GetUniStorm().m_TODKeyframeTimes[6];

            if (audioEvent.Name == "Play_TODDawn" && (sunrise == 0 || sunrise == 12)) return false;
            if (audioEvent.Name == "Play_TODNight" && (sunset == 24 || sunset == 12)) return false;
            return true;

        }
    }
}