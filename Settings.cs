using AsmResolver.DotNet;
using Il2Cpp;
using ModSettings;
using System.Reflection;

namespace Solstice
{
    internal class SolsticeSettings : JsonModSettings
    {

        [Section("Solstice")]

        [Name("Enabled")]
        [Description("If enabled the length of the day will cycle from short to long and back to short.\n\nSETTINGS CAN ONLY BE CHANGED IN A SANDBOX!")]
        public bool enabled = true;

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

        [Name("Latitude : Temperature (°C)")]
        [Description("Latitude 65° as a reference \n(Vanilla = 0 / Realistic = ±0.7°C/°Lat)\n" +
                    "Lat. 50° & Temp. : 0.5 => 15 * 0.5°C = +7.5°C\n" +
                    "Lat. 60° & Temp. : 0.2 => 5 * 0.2°C = +1°C\n" +
                    "Lat. 70° & Temp. : 0.5 => -5 * 0.5°C = -2.5°C\n" +
                    "Lat. 80° & Temp. : 0.7 => -15 * 0.7°C = -10.5°C\n")]
        [Slider(0f, 1f, NumberFormat = "{0:0.#}°C")]
        public float latitudeTemp = 0.7f;

        [Name("Latitude : Seasonal temperature gap (°C)")]
        [Description("Average seasonal gap between the 2 solstices \n(Vanilla = 0)\n" +
                    "Lat. 50° : Seasonal gap : 20°C \n(+10°C in summer & -10°C in winter)\n" +
                    "Lat. 60° : Seasonal gap : 25°C \n(+12,5°C in summer & -12,5°C in winter)\n" +
                    "Lat. 70° : Seasonal gap : 30°C \n(+15°C in summer & -15°C in winter)\n" +
                    "Lat. 80° : Seasonal gap : 35°C \n(+17,5°C in summer & -17,5°C in winter)\n")]
        [Slider(0f, 50f, NumberFormat = "{0:0.#}°C")]
        public float seasonalTempGap = 28f;

        protected override void OnChange(FieldInfo field, object? oldValue, object? newValue)
        {
            if (GameManager.IsMainMenuActive())
            {
                enabled = false;
                RefreshGUI();
            }
            SetFieldVisible(nameof(cycleLength), Settings.settings.enabled == true);
            SetFieldVisible(nameof(startDay), Settings.settings.enabled == true);
            SetFieldVisible(nameof(latitude), Settings.settings.enabled == true);
            SetFieldVisible(nameof(latitudeTemp), Settings.settings.enabled == true);
            SetFieldVisible(nameof(seasonalTempGap), Settings.settings.enabled == true);
        }

        protected override void OnConfirm()
        {
            base.OnConfirm();
            Solstice.ApplySettings();
        }

    }

    internal static class Settings
    {
        public static SolsticeSettings settings = new();

        public static void OnLoad()
        {
            settings.AddToModSettings("Solstice");
        }
    }


}