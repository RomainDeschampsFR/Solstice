using Il2Cpp;
using MelonLoader;
using ModData;
using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;

namespace Solstice
{
    internal class SaveDataManager
    {
        public static ModDataManager? dataManager;
        public static bool reloadPending = true;

        public static IEnumerator LoadSolsticeParameters()
        {
            if (reloadPending)
            {
                float waitSeconds = 0.2f;
                for (float t = 0f; t < waitSeconds; t += Time.deltaTime) yield return null;

                dataManager = new ModDataManager("Solstice", false);
                string serializedData = dataManager.Load();
                
                if (serializedData != null)
                {
                    string[] deserializedData = serializedData.Split(";");
                    Solstice.Enabled = (bool.TryParse(deserializedData[0],out bool result0)) ? result0 : false;
                    Solstice.CycleLength = (int.TryParse(deserializedData[1], out int result1)) ? result1 : 365;
                    Solstice.CycleOffset = (int.TryParse(deserializedData[2], out int result2)) ? result2 : 0;
                    Solstice.Latitude = (int.TryParse(deserializedData[3], out int result3)) ? result3 : 68;
                    Solstice.LatitudeTemp = (float.TryParse(deserializedData[4], out float result4)) ? result4 : 0;
                    Solstice.SeasonalTempGap = (float.TryParse(deserializedData[5], out float result5)) ? result5 : 0;
                    Solstice.MaxDrop = (float.TryParse(deserializedData[6], out float result6)) ? result6 : 0;
                    Solstice.DeclineStartDay = (int.TryParse(deserializedData[7], out int result7)) ? result7 : 0;
                    Solstice.DeclineEndDay = (int.TryParse(deserializedData[8], out int result8)) ? result8 : 0;

                    Settings.settings.enabled = Solstice.Enabled;
                    Settings.settings.cycleLength = Solstice.CycleLength;
                    Settings.settings.startDay = Solstice.CycleOffset + 1;
                    Settings.settings.latitude = Solstice.Latitude;
                    Settings.settings.latitudeTemp = Solstice.LatitudeTemp;
                    Settings.settings.seasonalTempGap = Solstice.SeasonalTempGap;
                    Settings.settings.maxDrop = Solstice.MaxDrop;
                    Settings.settings.declineStartDay = Solstice.DeclineStartDay;
                    Settings.settings.declineEndDay = Solstice.DeclineEndDay;
                    Settings.settings.RefreshGUI();
                    Settings.settings.SetFieldVisible(nameof(Settings.settings.cycleLength), Settings.settings.enabled == true);
                    Settings.settings.SetFieldVisible(nameof(Settings.settings.startDay), Settings.settings.enabled == true);
                    Settings.settings.SetFieldVisible(nameof(Settings.settings.latitude), Settings.settings.enabled == true);
                    Settings.settings.SetFieldVisible(nameof(Settings.settings.latitudeTemp), Settings.settings.enabled == true);
                    Settings.settings.SetFieldVisible(nameof(Settings.settings.seasonalTempGap), Settings.settings.enabled == true);
                    Settings.settings.SetFieldVisible(nameof(Settings.settings.maxDrop), Settings.settings.enabled == true);
                    Settings.settings.SetFieldVisible(nameof(Settings.settings.declineStartDay), Settings.settings.enabled == true);
                    Settings.settings.SetFieldVisible(nameof(Settings.settings.declineEndDay), Settings.settings.enabled == true);

                    MelonLogger.Msg($"[SOLSTICE] Parameters Loaded :" +
                                    $"\nEnabled: {Solstice.Enabled}" +
                                    $"\nCycleLength: {Solstice.CycleLength}" +
                                    $"\nCycleOffset: {Solstice.CycleOffset}" +
                                    $"\nLatitude: {Solstice.Latitude}" +
                                    $"\nLatitudeTemp: {Solstice.LatitudeTemp}" +
                                    $"\nSeasonalTempGap: {Solstice.SeasonalTempGap}" +
                                    $"\nMaxDrop: {Solstice.MaxDrop}" +
                                    $"\nDeclineStartDay: {Solstice.DeclineStartDay}" +
                                    $"\nDeclineEndDay: {Solstice.DeclineEndDay}");
                }
                reloadPending = false;

                Solstice.Update(GameManager.GetUniStorm());
            }
        }
    }
}


