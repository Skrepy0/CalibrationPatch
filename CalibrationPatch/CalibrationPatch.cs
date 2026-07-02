using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CalibrationPatch
{
  [HarmonyPatch]
  public static class CalibrationPatch
  {
    private static MethodBase targetMethod;
    private static FieldInfo fieldAvgTimeOffset;
    private static FieldInfo fieldConductorCurrentPreset;
    private static FieldInfo fieldPresetInputOffset;

    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
      if (targetMethod != null) return targetMethod;

      Type typeCalibration = AccessTools.TypeByName("scnCalibration");
      if (typeCalibration != null)
        targetMethod = AccessTools.Method(typeCalibration, "Calibrated");

      if (targetMethod == null)
      {
        Type oldType = AccessTools.TypeByName("scrCalibrationPlanet");
        if (oldType != null)
          targetMethod = AccessTools.Method(oldType, "PostSong");
      }

      if (targetMethod != null)
      {
        Type declaringType = targetMethod.DeclaringType;
        fieldAvgTimeOffset = AccessTools.Field(declaringType, "averageTimeOffset");

        Type conductorType = AccessTools.TypeByName("scrConductor");
        if (conductorType != null)
        {
          fieldConductorCurrentPreset = AccessTools.Field(conductorType, "currentPreset");
          if (fieldConductorCurrentPreset != null)
          {
            object preset = fieldConductorCurrentPreset.GetValue(null);
            if (preset != null)
            {
              fieldPresetInputOffset = AccessTools.Field(preset.GetType(), "inputOffset");
            }
          }
        }
      }

      return targetMethod;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    static void Postfix(object __instance)
    {
      try
      {
        if (fieldAvgTimeOffset == null || fieldConductorCurrentPreset == null || fieldPresetInputOffset == null)
        {
          Main.ModEntry?.Logger.Log($"[{{Main.ModId}}] Reflection fields not initialized, skipping.");
          return;
        }

        float avgSec = Convert.ToSingle(fieldAvgTimeOffset.GetValue(__instance));
        int deltaMs = Mathf.RoundToInt(avgSec * 1000f);
        int beatOffset = Main.UserOffsetBeat * Main.CurrentMpb;
        int finalDelta = deltaMs + beatOffset;

        object preset = fieldConductorCurrentPreset.GetValue(null);
        if (preset == null) return;

        int newOffset = finalDelta;

        scrConductor.currentPreset.inputOffset = newOffset;
        scrConductor.SaveCurrentPreset();
        Persistence.WriteSaveToDisk();

        Main.ModEntry?.Logger.Log($"[{Main.ModId}] Offset applied:{deltaMs} → {newOffset}");
      }
      catch (Exception ex)
      {
        Main.ModEntry?.Logger.Log($"[{Main.ModId}] Error: {ex.Message}");
      }
    }
  }
}