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
                    Solstice.Latitude = (float.TryParse(deserializedData[3], out float result3)) ? result3 : 45;
                    Solstice.LatitudeTemp = (float.TryParse(deserializedData[4], out float result4)) ? result4 : 0;
                    Solstice.SeasonalTempGap = (float.TryParse(deserializedData[5], out float result5)) ? result5 : 0;

                    MelonLogger.Msg($"[SOLSTICE] Parameters Loaded :" +
                                    $"\nEnabled: {Solstice.Enabled}" +
                                    $"\nCycleLength: {Solstice.CycleLength}" +
                                    $"\nCycleOffset: {Solstice.CycleOffset}" +
                                    $"\nLatitude: {Solstice.Latitude}" +
                                    $"\nLatitudeTemp: {Solstice.LatitudeTemp}" +
                                    $"\nSeasonalTempGap: {Solstice.SeasonalTempGap}");
                }
                reloadPending = false;

                Solstice.Update(GameManager.GetUniStorm());
            }
        }
    }
}


