﻿using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using ModSettings;
using AsmResolver.DotNet;
using HarmonyLib;
using Il2Cpp;
using Random = UnityEngine.Random;
using System.Runtime;
using Innovative.SolarCalculator;
using UnityEngine.Playables;
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;

namespace Solstice
{
    public class Solstice : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
            Settings.OnLoad();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (sceneName.Contains("MainMenu"))
            {
                Enabled = false;
                CycleLength = 365;
                CycleOffset = 0;
                Latitude = 45;
                LatitudeTemp = 0;
                SeasonalTempGap = 0;
                TemperatureOffset = 0;

                //UPDATE MOD SETTINGS UI TO AVOID CONFUSION (CHANGES ONLY ALLOWED IN SANDBOX)
                Settings.settings.enabled = false;
                Settings.settings.cycleLength = 365;
                Settings.settings.startDay = 1;
                Settings.settings.latitude = 68;
                Settings.settings.latitudeTemp = 0;
                Settings.settings.seasonalTempGap = 0;
                Settings.settings.RefreshGUI();
                Settings.settings.SetFieldVisible(nameof(Settings.settings.cycleLength), Settings.settings.enabled == true);
                Settings.settings.SetFieldVisible(nameof(Settings.settings.startDay), Settings.settings.enabled == true);
                Settings.settings.SetFieldVisible(nameof(Settings.settings.latitude), Settings.settings.enabled == true);
                Settings.settings.SetFieldVisible(nameof(Settings.settings.latitudeTemp), Settings.settings.enabled == true);
                Settings.settings.SetFieldVisible(nameof(Settings.settings.seasonalTempGap), Settings.settings.enabled == true);

                RestoreKeyframeTimes(GameManager.GetUniStorm());
                SaveDataManager.reloadPending = true;
            }
            if (sceneName.Contains("SANDBOX")) MelonCoroutines.Start(SaveDataManager.LoadSolsticeParameters());
        }

        internal static readonly Interpolator interpolator = new Interpolator();

        internal static TODStateData BlizzardDawnColors = new TODStateData();
        internal static TODStateData ClearDawnColors = new TODStateData();
        internal static TODStateData ClearAuroraDawnColors = new TODStateData();
        internal static TODStateData CloudyDawnColors = new TODStateData();
        internal static TODStateData DenseFogDawnColors = new TODStateData();
        internal static TODStateData HeavySnowDawnColors = new TODStateData();
        internal static TODStateData LightFogDawnColors = new TODStateData();
        internal static TODStateData LightSnowDawnColors = new TODStateData();
        internal static TODStateData PartlyCloudyDawnColors = new TODStateData();
        internal static TODStateData ToxicFogDawnColors = new TODStateData();
        internal static TODStateData ElectrostaticFogDawnColors = new TODStateData();

        internal static TODStateData BlizzardAfternoonColors = new TODStateData();
        internal static TODStateData ClearAfternoonColors = new TODStateData();
        internal static TODStateData ClearAuroraAfternoonColors = new TODStateData();
        internal static TODStateData CloudyAfternoonColors = new TODStateData();
        internal static TODStateData DenseFogAfternoonColors = new TODStateData();
        internal static TODStateData HeavySnowAfternoonColors = new TODStateData();
        internal static TODStateData LightFogAfternoonColors = new TODStateData();
        internal static TODStateData LightSnowAfternoonColors = new TODStateData();
        internal static TODStateData PartlyCloudyAfternoonColors = new TODStateData();
        internal static TODStateData ToxicFogAfternoonColors = new TODStateData();
        internal static TODStateData ElectrostaticFogAfternoonColors = new TODStateData();

        internal static bool Enabled;
        internal static int CycleLength;
        internal static int CycleOffset;
        internal static int Latitude;
        internal static float LatitudeTemp;
        internal static float SeasonalTempGap;
        internal static float TemperatureOffset; 
        internal static float TempHigh; 
        internal static float TempLow;

        internal static float[] originalKeyframeTimes = new float[7];
        internal static float originalMasterTimeKeyOffset;

        internal static void ApplySettings()
        {
            if (GameManager.IsMainMenuActive())
            {
                MelonLogger.Msg("Setting changes only allowed in a sandbox");
                return;
            }

            Enabled = Settings.settings.enabled;

            if (!Enabled)
            {
                RestoreKeyframeTimes(GameManager.GetUniStorm());
                return;
            }

            CycleLength = Settings.settings.cycleLength;
            CycleOffset = Settings.settings.startDay - 1;
            Latitude = Settings.settings.latitude;
            LatitudeTemp = Settings.settings.latitudeTemp;
            SeasonalTempGap = Settings.settings.seasonalTempGap;

            MelonLogger.Msg($"[SOLSTICE] Parameters saved :" +
                            $"\nEnabled : {Enabled}" +
                            $"\nCycleLength : {CycleLength}" +
                            $"\nCycleOffSet : {CycleOffset}" +
                            $"\nLatitude : {Latitude}" +
                            $"\nLatitudeTemp : {LatitudeTemp}" +
                            $"\nSeasonalGap : {SeasonalTempGap}");

            if (SaveDataManager.dataManager != null) SaveDataManager.dataManager.Save($"{Enabled};{CycleLength};{CycleOffset};{Latitude};{LatitudeTemp};{SeasonalTempGap}");

            Update(GameManager.GetUniStorm());
        }

        internal static void Init(UniStormWeatherSystem uniStormWeatherSystem)
        {
            if (uniStormWeatherSystem != null)
            {
                originalMasterTimeKeyOffset = uniStormWeatherSystem.m_MasterTimeKeyOffset;
                for (int i = 0; i < uniStormWeatherSystem.m_TODKeyframeTimes.Length; i++)
                {
                    originalKeyframeTimes[i] = uniStormWeatherSystem.m_TODKeyframeTimes[i];
                }
            }
        }

        internal static void Update(UniStormWeatherSystem uniStormWeatherSystem)
        {
            if (!Enabled) return;

            uniStormWeatherSystem.m_MasterTimeKeyOffset = 0;
            float[] keyframeTimes = new float[7];

            int dayOfYear = DayOfYear(uniStormWeatherSystem.m_DayCounter);
            int dayBefore = DayOfYear(uniStormWeatherSystem.m_DayCounter - 1);
            int dayAfter = DayOfYear(uniStormWeatherSystem.m_DayCounter + 1);

            DateTime referenceDate = DateTime.SpecifyKind(new DateTime(2022, 1, 1).AddDays(dayOfYear).AddHours(12), DateTimeKind.Utc);
            DateTime referenceDateBefore = DateTime.SpecifyKind(new DateTime(2022, 1, 1).AddDays(dayBefore).AddHours(12), DateTimeKind.Utc);
            DateTime referenceDateAfter = DateTime.SpecifyKind(new DateTime(2022, 1, 1).AddDays(dayAfter).AddHours(12), DateTimeKind.Utc);

            SolarTimes solartimes = new SolarTimes(referenceDate, (float)Latitude, 0f);
            SolarTimes solartimesDayBefore = new SolarTimes(referenceDateBefore, (float)Latitude, 0f);
            SolarTimes solartimesDayAfter = new SolarTimes(referenceDateAfter, (float)Latitude, 0f);

            //DISPLAY THE NUMBER OF POLAR DAYS/NIGHTS DEPENDING ON LATITUDE
            /*int polarday = 0;
            int polarnight = 0;
            for (int i = 0; i < 365; i++)
            {
                SolarTimes solartimestest = new SolarTimes(referenceDate.AddDays(i), Latitude, 0f);
                if (solartimestest.IsPolarDay) polarday += 1;
                if (solartimestest.IsPolarNight) polarnight += 1;
            }
            MelonLogger.Msg($"Latitude : {Latitude} --- DAY : {polarday} --- NIGHT : {polarnight}");*/

            //MelonLogger.Msg("DayOfYear : " + dayOfYear);
            //MelonLogger.Msg("Date : " + referenceDate);
            //MelonLogger.Msg("Date kind : " + referenceDate.Kind);
            //MelonLogger.Msg($"SunriseFull : {solartimes.Sunrise}");
            //MelonLogger.Msg($"SunsetFull : {solartimes.Sunset}");
            //MelonLogger.Msg($"DateBefore : {referenceDateBefore}");
            //MelonLogger.Msg($"DateAfter : {referenceDateAfter}");
            //MelonLogger.Msg($"Polar Night ? : {solartimes.IsPolarNight}");
            //MelonLogger.Msg($"Polar Night (day before) ? : {solartimesDayBefore.IsPolarNight}");
            //MelonLogger.Msg($"Polar Night (day after) ? : {solartimesDayAfter.IsPolarNight}");
            //MelonLogger.Msg($"Polar Day ? : {solartimes.IsPolarDay}");
            //MelonLogger.Msg($"Polar Day (day before) ? : {solartimesDayBefore.IsPolarDay}");
            //MelonLogger.Msg($"Polar Day (day after) ? : {solartimesDayAfter.IsPolarDay}");

            float csunrise = (float)solartimesDayBefore.Sunrise.TimeOfDay.TotalHours;
            float cnoon = 12f;
            float csunset = (float)solartimesDayBefore.Sunset.TimeOfDay.TotalHours;

            //Method is called at 12 oclock. The whole block below allow a smooth change regarding sky color.
            //Doesn't work well with short cycleLength, in other words when there's only one polar night/day
            if (solartimes.IsPolarNight)
            {
                if (!solartimesDayBefore.IsPolarNight)
                {
                    //MelonLogger.Msg("dayBeforeIsNOTPolarNight : " + !solartimesDayBefore.IsPolarNight);
                    csunrise = cnoon;
                }
                else if (!solartimesDayAfter.IsPolarNight)
                {
                    //MelonLogger.Msg("dayAfterIsNOTPolarNight : " + !solartimesDayAfter.IsPolarNight);
                    csunrise = (float)solartimesDayAfter.Sunrise.TimeOfDay.TotalHours;
                    csunset = cnoon;
                }
                else
                {
                    csunrise = cnoon;
                    csunset = cnoon;
                }
            }
            else if (solartimes.IsPolarDay)
            {
                csunrise = 0;
                csunset = 24;
            }
            else
            {
                csunrise = (float)solartimes.Sunrise.TimeOfDay.TotalHours;
                csunset = (float)solartimes.Sunset.TimeOfDay.TotalHours;
            }


            //MelonLogger.Msg($"Sunrise : {csunrise}");
            //MelonLogger.Msg($"Solar Noon : {cnoon}");
            //MelonLogger.Msg($"Sunset : {csunset}");


            //MelonLogger.Msg("SolarDeclinaison : " + solartimes.SolarDeclination.Degrees);
            uniStormWeatherSystem.m_SunAngle = 90 - Latitude + solartimes.SolarDeclination.Degrees;

            float sunAngle = uniStormWeatherSystem.m_SunAngle;

            //MelonLogger.Msg("SunAngle : " + uniStormWeatherSystem.m_SunAngle);
            //MelonLogger.Msg($"Elevation Angle: {solartimes.SolarElevation.Degrees}");

            //Methods allowing to know the time when the sun is low enough on the horizon to keep the sky with warm colors 
            float warmSkyMorningHour = GetTimeForSolarElevationBeforeNoon(referenceDate, 15, Latitude, csunrise);
            float warmSkyAfternoonHour = GetTimeForSolarElevationAfterNoon(referenceDate, 15, Latitude, csunset);

            //Set key frames in order to have smooth change in sky colors at particular time of the years
            if (cnoon - warmSkyMorningHour < 0.75 || warmSkyAfternoonHour - cnoon < 0.75)
            {
                if (uniStormWeatherSystem.m_TODKeyframeTimes[2] != 12)
                {
                    warmSkyMorningHour = cnoon;
                }
                else if (true)
                {
                    warmSkyMorningHour = cnoon;
                    warmSkyAfternoonHour = cnoon;
                }
            }
            else
            {
                if (uniStormWeatherSystem.m_TODKeyframeTimes[2] == 12) warmSkyAfternoonHour = cnoon;
            }

            float sunHorizonMorning = csunrise + (warmSkyMorningHour - csunrise) / 3;
            float sunHorizonAfternoon = csunset - (csunset - warmSkyAfternoonHour) / 3;
            sunHorizonMorning = (sunHorizonMorning - csunrise > 1) ? csunrise + 1 : sunHorizonMorning;
            sunHorizonAfternoon = (csunset - sunHorizonAfternoon > 1) ? csunset - 1 : sunHorizonAfternoon;


            keyframeTimes[0] = csunrise;
            keyframeTimes[1] = (solartimes.IsPolarDay) ? 0 : sunHorizonMorning;
            keyframeTimes[2] = warmSkyMorningHour;
            keyframeTimes[3] = cnoon;
            keyframeTimes[4] = warmSkyAfternoonHour;
            keyframeTimes[5] = (solartimes.IsPolarDay) ? 24 : sunHorizonAfternoon;
            keyframeTimes[6] = csunset;

            uniStormWeatherSystem.m_TODKeyframeTimes = keyframeTimes;
            for (int i = 0; i < uniStormWeatherSystem.m_TODKeyframeTimes.Length; i++)
            {
                uniStormWeatherSystem.m_TODKeyframeTimes[i] = keyframeTimes[i];
            }
            TimeWidgetUpdater.SetTimes(csunrise, cnoon, csunset);

            // UPDATE TODStateData FOR ALL WEATHER TYPES IN ORDER TO HAVE A SMOOTH CHANGE IN SKY COLOR
            if (uniStormWeatherSystem.m_TODKeyframeTimes[5] == 24)
            {
                uniStormWeatherSystem.m_WeatherStateConfigs[0].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[0].m_DuskColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[1].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[1].m_DuskColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[2].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[2].m_DuskColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[3].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[3].m_DuskColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[4].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[4].m_DuskColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[5].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[5].m_DuskColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[6].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[6].m_DuskColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[7].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[7].m_DuskColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[8].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[8].m_DuskColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[9].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[9].m_DuskColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[10].m_DawnColors = uniStormWeatherSystem.m_WeatherStateConfigs[10].m_DuskColors;
            }
            else if (uniStormWeatherSystem.m_TODKeyframeTimes[2] == 12 || uniStormWeatherSystem.m_TODKeyframeTimes[4] == 12)
            {
                uniStormWeatherSystem.m_WeatherStateConfigs[0].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[0].m_MorningColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[1].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[1].m_MorningColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[2].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[2].m_MorningColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[3].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[3].m_MorningColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[4].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[4].m_MorningColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[5].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[5].m_MorningColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[6].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[6].m_MorningColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[7].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[7].m_MorningColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[8].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[8].m_MorningColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[9].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[9].m_MorningColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[10].m_AfternoonColors = uniStormWeatherSystem.m_WeatherStateConfigs[10].m_MorningColors;
            }
            else
            {
                uniStormWeatherSystem.m_WeatherStateConfigs[0].m_DawnColors = BlizzardDawnColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[1].m_DawnColors = ClearDawnColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[2].m_DawnColors = ClearAuroraDawnColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[3].m_DawnColors = CloudyDawnColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[4].m_DawnColors = DenseFogDawnColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[5].m_DawnColors = HeavySnowDawnColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[6].m_DawnColors = LightFogDawnColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[7].m_DawnColors = LightSnowDawnColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[8].m_DawnColors = PartlyCloudyDawnColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[9].m_DawnColors = ToxicFogDawnColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[10].m_DawnColors = ElectrostaticFogDawnColors;

                uniStormWeatherSystem.m_WeatherStateConfigs[0].m_AfternoonColors = BlizzardAfternoonColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[1].m_AfternoonColors = ClearAfternoonColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[2].m_AfternoonColors = ClearAuroraAfternoonColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[3].m_AfternoonColors = CloudyAfternoonColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[4].m_AfternoonColors = DenseFogAfternoonColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[5].m_AfternoonColors = HeavySnowAfternoonColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[6].m_AfternoonColors = LightFogAfternoonColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[7].m_AfternoonColors = LightSnowAfternoonColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[8].m_AfternoonColors = PartlyCloudyAfternoonColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[9].m_AfternoonColors = ToxicFogAfternoonColors;
                uniStormWeatherSystem.m_WeatherStateConfigs[10].m_AfternoonColors = ElectrostaticFogAfternoonColors;
            }

            //AURORA & GLIMMER FOG
            GameManager.GetWeatherComponent().m_AuroraActivationWindowStart = (int)Math.Floor(keyframeTimes[4]);
            GameManager.GetWeatherComponent().m_AuroraActivationWindowEnd = (keyframeTimes[0] < 1) ? 0 : (int)Math.Floor(keyframeTimes[0] - 1);
            // The closer to the equinoxes the higher the chance to get an aurora
            GameManager.GetWeatherComponent().m_AuroraEarlyWindowProbability = 20 - (int)Math.Ceiling(Math.Abs(((solartimes.SolarDeclination.Degrees + 23) / 46) - 0.5f) * 20) ;
            GameManager.GetWeatherComponent().m_AuroraLateWindowProbability = 10 - (int)Math.Ceiling(Math.Abs(((solartimes.SolarDeclination.Degrees + 23) / 46) - 0.5f) * 10);

            GameManager.GetWeatherComponent().m_ElectrostaticFogActivationWindowStart = GameManager.GetWeatherComponent().m_AuroraActivationWindowEnd;
            GameManager.GetWeatherComponent().m_ElectrostaticFogActivationWindowEnd = GameManager.GetWeatherComponent().m_AuroraActivationWindowStart - 2;
            GameManager.GetWeatherComponent().m_ElectrostaticFogSelectionWindowStart = GameManager.GetWeatherComponent().m_AuroraActivationWindowEnd;
            GameManager.GetWeatherComponent().m_ElectrostaticFogSelectionWindowEnd = GameManager.GetWeatherComponent().m_AuroraActivationWindowStart;

            //TEMPERATURE
            GameManager.GetWeatherComponent().m_HourWarmingBegins = (int)Math.Floor(keyframeTimes[1]);
            GameManager.GetWeatherComponent().m_HourCoolingBegins = (int)Math.Floor(keyframeTimes[5]);

            float latitudeTemp = (65 - Latitude) * Settings.settings.latitudeTemp;
            float seasonalTempGap = (((solartimes.SolarDeclination.Degrees + 23) / 46) - 0.5f) * Settings.settings.seasonalTempGap;
            TemperatureOffset = latitudeTemp + seasonalTempGap;

        }

        public static void RestoreKeyframeTimes(UniStormWeatherSystem uniStormWeatherSystem)
        {

            uniStormWeatherSystem.m_MasterTimeKeyOffset = originalMasterTimeKeyOffset;
            for (int i = 0; i < uniStormWeatherSystem.m_TODKeyframeTimes.Length; i++)
            {
                uniStormWeatherSystem.m_TODKeyframeTimes[i] = originalKeyframeTimes[i];
            }


        }

        // -----------------------------------  UTILITIES  ----------------------------------- //

        internal static int DayOfYear(int dayCounter)
        {

            float ponderatedCycleOffset = ((CycleOffset) * CycleLength) / 365;
            //MelonLogger.Msg("ponderatedCycleOffset : " + ponderatedCycleOffset);
            int day = (int)Math.Round(((dayCounter + ponderatedCycleOffset) % CycleLength) * 365 / CycleLength);
            return day = (day == 0) ? 365 : day ;
        }

        public static float GetTimeForSolarElevationBeforeNoon(DateTime referenceDate, float targetSolarElevation, float latitude, float csunrise)
        {
            if (csunrise == 12) return 12;


            int increment = 0;
            float elevationAngle = -50;
            DateTime warmSkyMorningHour;

            do
            {
                warmSkyMorningHour = referenceDate.AddMinutes(increment);
                SolarTimes comparedSolarTimes = new SolarTimes(warmSkyMorningHour, latitude, 0f);
                elevationAngle = comparedSolarTimes.SolarElevation.Degrees;
                
                //MelonLogger.Msg($"Elevation Angle: {elevationAngle}, Increment: {increment}");

                increment -= 6;

            } while (elevationAngle > targetSolarElevation && (float)referenceDate.TimeOfDay.TotalHours > csunrise);

            //MelonLogger.Msg($"GetTimeForSolarElevationBeforeNoon : {(float)warmSkyMorningHour.TimeOfDay.TotalHours}");
            if ((float)warmSkyMorningHour.TimeOfDay.TotalHours > csunrise)
            {
                return (float)warmSkyMorningHour.TimeOfDay.TotalHours;
            }
            else
            {
                return csunrise;
            }
        }

        public static float GetTimeForSolarElevationAfterNoon(DateTime referenceDate, float targetSolarElevation, float latitude, float csunset)
        {
            if (csunset == 12) return 12;

            int increment = 0;
            float elevationAngle = -50;
            DateTime warmSkyAfternoonHour;

            do
            {
                warmSkyAfternoonHour = referenceDate.AddMinutes(increment);
                SolarTimes comparedSolarTimes = new SolarTimes(warmSkyAfternoonHour, latitude, 0f);
                elevationAngle = comparedSolarTimes.SolarElevation.Degrees;

                //MelonLogger.Msg($"Elevation Angle: {elevationAngle}, Increment: {increment}");

                increment += 6;

            } while (elevationAngle > targetSolarElevation && (float)warmSkyAfternoonHour.TimeOfDay.TotalHours < csunset);

            //MelonLogger.Msg($"GetTimeForSolarElevationAfterNoon : {(float)warmSkyAfternoonHour.TimeOfDay.TotalHours}");
            if ((float)warmSkyAfternoonHour.TimeOfDay.TotalHours < csunset)
            {
                return (float)warmSkyAfternoonHour.TimeOfDay.TotalHours;
            }
            else
            {
                return csunset;
            }
        }
    }
}
