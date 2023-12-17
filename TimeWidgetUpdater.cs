using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace Solstice
{
    internal class TimeWidgetUpdater
    {
        private static readonly Interpolator moonInterpolator = new Interpolator();
        private static readonly Interpolator sunInterpolator = new Interpolator();
        private static float moonRadius;
        private static float sunRadius;

        internal static void Initialize(TimeWidget timeWidget)
        {
            sunRadius = timeWidget.m_SunRadius; 
            moonRadius = timeWidget.m_MoonRadius;
        }

        internal static void SetTimes(float sunrise, float noon, float sunset)
        {
            float epsilonSunrise = (sunrise == 12) ? -0.0000001f : 0.0000001f;
            float epsilonSunset = (sunset == 12) ? 0.0000001f : -0.0000001f;

            sunInterpolator.Clear();
            sunInterpolator.Set(0, -180);
            sunInterpolator.Set((sunrise / 24f) + epsilonSunrise, -119);
            sunInterpolator.Set(noon / 24f, 0);
            sunInterpolator.Set((sunset / 24f) + epsilonSunset, 119);
            sunInterpolator.Set(1, 180);

            moonInterpolator.Clear();
            moonInterpolator.Set(0, 0);
            moonInterpolator.Set((sunrise / 24f) + epsilonSunrise, 114);
            moonInterpolator.Set(noon / 24f, 180);
            moonInterpolator.Set((sunset / 24f) + epsilonSunset, 246);
            moonInterpolator.Set(1, 360);
        }

        internal static void Update(TimeWidget timeWidget, float angleDegrees)
        {
            float normalizedTime = GameManager.GetUniStorm().m_NormalizedTime;

            float sunAngle = sunInterpolator.GetValue(normalizedTime);
            Vector3 sunPosition = GetPositionOnCircle(sunRadius, sunAngle);
            //MelonLogger.Msg($"Sun Angle : {sunAngle} --- Sun Position : {sunPosition}");
            timeWidget.m_SunSprite.transform.position = timeWidget.m_ArrowSprite.transform.position + sunPosition;
            
            float moonAngle = moonInterpolator.GetValue(normalizedTime);
            Vector3 moonPosition = GetPositionOnCircle(moonRadius, moonAngle);
            //MelonLogger.Msg($"Moon Angle : {moonAngle} --- Moon Position : {moonPosition}");
            timeWidget.m_MoonSprite.transform.position = timeWidget.m_ArrowSprite.transform.position + moonPosition;
        }

        private static Vector3 GetPositionOnCircle(float radius, float angleDegrees)
        {
            float x = radius * Mathf.Sin(angleDegrees * Mathf.Deg2Rad);
            float y = radius * Mathf.Cos(angleDegrees * Mathf.Deg2Rad);

            Vector3 position = new Vector3(x, y, 0.0f);

            return position;
        }
    }
}