using AsmResolver.DotNet;
using Il2Cpp;
using ModSettings;
using System.Reflection;

namespace Solstice
{
    internal static class Settings
    {
        public static SolsticeSettings settings = new();

        public static void OnLoad()
        {
            settings.AddToModSettings("Solstice");
        }
    }

    internal class SolsticeSettings : JsonModSettings
    {
        [Name("Sunshine Warmth Bonus : Enabled ")]
        [Description("When enabled the player will benefit from a temperature bonus when in direct sunlight.")]
        public bool enabledSunBuff = true;

        [Name("Sun Strength")]
        [Description("With a strength of 10 you can expect a maximum bonus of 6°C (At noon, in summer, at the lowest latitude and with a clear weather)\n" +
            "Clear => strength\n" +
            "Partly Cloudy => strength / 2\n" +
            "Light Fog=> strength / 20\n" +
            "Cloudy => strength / 60\n" +
            "Other weather type => 0" )]
        [Slider(0, 20, 21)]
        public int sunStrength = 6;

        [Name("Solstice : Enabled")]
        [Description("If enabled the length of the day will cycle from short to long and back to short.\n\nSOLSTICE SETTINGS CAN ONLY BE CHANGED IN A SANDBOX!")]
        public bool enabled = true;

        [Section("SOLSTICE : SEASONAL PARAMETERS")]

        [Name("Cycle Length")]
        [Description("The length, in days, of a full cycle in in-game time.")]
        [Slider(60, 365)]
        public int cycleLength = 365;

        [Name("Start Day")]
        [Description("Where in the cycle the game starts.\n 79 = Spring Equinox\n 172 : Summer Solstice\n 265 : Autumn Equinox\n 355 : Winter Solstice")]
        [Slider(1, 365)]
        public int startDay = 1;

        [Name("Latitude")]
        [Description("The greater the latitude, the longer the days in summer and the shorter in winter.\n" +
                    "There are no polar days/nights up to 65° latitude (value displayed below using a 365-day cycle as a reference).\n" +
                    "Latitude\t --- \tPolar days\t --- \tPolar nights\t\t\t\t\t\t\r\n" + 
                    "65°\t --- \t0\t --- \t0\t   |   \t73°\t --- \t96\t --- \t78\r\n" +
                    "66°\t --- \t18\t --- \t0\t   |   \t74°\t --- \t102\t --- \t85\r\n" +
                    "67°\t --- \t38\t --- \t0\t   |   \t75°\t --- \t108\t --- \t92\r\n" +
                    "68°\t --- \t52\t --- \t25\t   |   \t76°\t --- \t115\t --- \t98\r\n" +
                    "69°\t --- \t62\t --- \t41\t   |   \t77°\t --- \t121\t --- \t104\r\n" +
                    "70°\t --- \t72\t --- \t52\t   |   \t78°\t --- \t127\t --- \t110\r\n" +
                    "71°\t --- \t80\t --- \t62\t   |   \t79°\t --- \t132\t --- \t116\r\n" +
                    "72°\t --- \t88\t --- \t70\t   |   \t80°\t --- \t138\t --- \t122\r\n")]
        [Slider(45, 80, NumberFormat = "{0:0}°")]
        public int latitude = 68;

        [Name("Temperature offset (°C)")]
        [Description("Flat temperature offset (Guide values below for realistic settings.\n" +
                    "Latitude 65° as a reference \n(Vanilla = 0 / Realistic = ±0.7°C/°Lat)\n" +
                    "Lat. 50° & Temp. : +10.5°C\n" +
                    "Lat. 60° & Temp. : +3.5°C\n" +
                    "Lat. 65° & Temp. : 0°C\n" +
                    "Lat. 70° & Temp. : -3.5°C\n" +
                    "Lat. 80° & Temp. : -10.5°C\n")]
        [Slider(-20f, 20f, NumberFormat = "{0:0.#}°C")]
        public float latitudeTemp = 0f;

        [Name("Seasonal temperature gap (°C)")]
        [Description("Average seasonal gap between the 2 solstices \n(Vanilla = 0)\n" +
                    "Lat. 50° : Seasonal gap : 20°C \n(+10°C in summer & -10°C in winter)\n" +
                    "Lat. 60° : Seasonal gap : 25°C \n(+12,5°C in summer & -12,5°C in winter)\n" +
                    "Lat. 70° : Seasonal gap : 30°C \n(+15°C in summer & -15°C in winter)\n" +
                    "Lat. 80° : Seasonal gap : 35°C \n(+17,5°C in summer & -17,5°C in winter)\n")]
        [Slider(0f, 50f, NumberFormat = "{0:0.#}°C")]
        public float seasonalTempGap = 28f;

        [Section("TEMPERATURE DROP OVERTIME")]

        [Name("Maximum Temperature Drop")]
        [Description("The maximum decrease in global temperatures.")]
        [Slider(0, 100, 101)]
        public float maxDrop = 0f;

        [Name("Decline Starting Day")]
        [Description("The day at which global temperatures start dropping.")]
        [Slider(0, 200, 201)]
        public int declineStartDay = 0;

        [Name("Decline Ending Day")]
        [Description("The day at which global temperatures do not drop any further.")]
        [Slider(0, 1000, 1001)]
        public int declineEndDay = 730;


        protected override void OnChange(FieldInfo field, object? oldValue, object? newValue)
        {
            if (GameManager.IsMainMenuActive())
            {
                enabled = false;
                RefreshGUI();
            }
            SetFieldVisible(nameof(sunStrength), Settings.settings.enabledSunBuff == true);
            SetFieldVisible(nameof(cycleLength), Settings.settings.enabled == true);
            SetFieldVisible(nameof(startDay), Settings.settings.enabled == true);
            SetFieldVisible(nameof(latitude), Settings.settings.enabled == true);
            SetFieldVisible(nameof(latitudeTemp), Settings.settings.enabled == true);
            SetFieldVisible(nameof(seasonalTempGap), Settings.settings.enabled == true);
            SetFieldVisible(nameof(maxDrop), Settings.settings.enabled == true);
            SetFieldVisible(nameof(declineStartDay), Settings.settings.enabled == true);
            SetFieldVisible(nameof(declineEndDay), Settings.settings.enabled == true);


            if (field.Name == nameof(declineStartDay)) declineEndDay = Math.Max((int)newValue, declineEndDay);
            else if (field.Name == nameof(declineEndDay)) declineStartDay = Math.Min((int)newValue, declineStartDay);

            RefreshGUI();

        }

        protected override void OnConfirm()
        {
            Solstice.ApplySettings();
            base.OnConfirm();
        }

    }
}