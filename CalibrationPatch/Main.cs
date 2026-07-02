using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityModManagerNet;
using UnityEngine;

namespace CalibrationPatch
{
  public class Main
  {
    public const string ModId = "CalibrationPatch";
    public static UnityModManager.ModEntry ModEntry;
    private static readonly Harmony Harmony = new(ModId);
    private static string settingsPath;
    private static int userOffsetBeat;
    public static int CurrentMpb = 400;
    private static bool patched;
    private static GUIStyle _warningStyle;


    public static bool Load(UnityModManager.ModEntry entry)
    {
      ModEntry = entry;
      settingsPath = Path.Combine(entry.Path, "settings.json");
      I18n.Load(entry.Path);
      LoadSettings();
      entry.OnGUI = OnGUI;
      entry.OnToggle = OnToggle;
      entry.OnUnload = OnUnload;
      ApplyPatch();
      ModEntry.Logger.Log($"{ModId} loaded");
      return true;
    }

    private static bool OnToggle(UnityModManager.ModEntry entry, bool value)
    {
      if (value) ApplyPatch();
      else Unpatch();

      return true;
    }

    private static bool OnUnload(UnityModManager.ModEntry entry)
    {
      Unpatch();
      return true;
    }

    private static void ApplyPatch()
    {
      if (patched) return;
      Harmony.CreateClassProcessor(typeof(CalibrationPatch)).Patch();
      patched = true;
      ModEntry.Logger.Log("Patch applied");
    }

    private static void Unpatch()
    {
      if (!patched) return;
      Harmony.UnpatchAll(ModId);
      patched = false;
      ModEntry.Logger.Log("Patch removed");
    }

    private static bool IsBetterCalibrationActive()
    {
      try
      {
        Type floatOffsetType = Type.GetType("BetterCalibration.Features.FloatOffset, BetterCalibration");
        if (floatOffsetType == null)
          return false;

        var modEntry = UnityModManager.FindMod("BetterCalibration");
        if (modEntry == null)
        {
          PropertyInfo instanceProp = floatOffsetType.GetProperty("Instance",
            BindingFlags.Public | BindingFlags.Static);
          if (instanceProp != null)
          {
            object instance = instanceProp.GetValue(null);
            return instance != null;
          }

          return true;
        }

        if (!modEntry.Active)
          return false;

        PropertyInfo instanceProp2 = floatOffsetType.GetProperty("Instance",
          BindingFlags.Public | BindingFlags.Static);
        if (instanceProp2 != null)
        {
          object instance = instanceProp2.GetValue(null);
          return instance != null;
        }

        return true;
      }
      catch
      {
        return false;
      }
    }

    private static GUIStyle WarningStyle
    {
      get
      {
        if (_warningStyle == null)
        {
          _warningStyle = new GUIStyle(GUI.skin.label)
          {
            normal = { textColor = Color.red },
            wordWrap = true,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
          };
        }

        return _warningStyle;
      }
    }

    private static void OnGUI(UnityModManager.ModEntry entry)
    {
      if (!entry.Active) return;

      GUILayout.BeginHorizontal();
      GUILayout.Label(I18n.Tr("language"), GUILayout.Width(150));
      string[] langs = { "en", "zh", "ko" };
      string[] langNames = { "English", "中文", "한국어" };
      int idx = Array.IndexOf(langs, I18n.Lang);
      if (idx < 0) idx = 0;
      int newIdx = GUILayout.SelectionGrid(idx, langNames, 3, GUILayout.Width(600));
      if (newIdx != idx)
      {
        I18n.Lang = langs[newIdx];
        SaveSettings();
      }

      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      // --- 偏移拍数 ---
      GUILayout.BeginHorizontal();
      GUILayout.Label(I18n.Tr("offset_beat"), GUILayout.Width(150));
      string offsetBeat = GUILayout.TextField(userOffsetBeat.ToString(), GUILayout.Width(80));
      if (int.TryParse(offsetBeat, out int v) && v != userOffsetBeat)
      {
        userOffsetBeat = v;
        SaveSettings();
      }

      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      // --- 每拍毫秒数 & BPM 显示 ---
      GUILayout.BeginHorizontal();
      GUILayout.Label(I18n.Tr("mpb"), GUILayout.Width(150));
      string mpb = GUILayout.TextField(CurrentMpb.ToString(), GUILayout.Width(150));
      if (int.TryParse(mpb, out int m) && m != CurrentMpb)
      {
        CurrentMpb = m;
        SaveSettings();
      }

      GUILayout.Label(string.Format(I18n.Tr("calibration_bpm"), (60000.0d / CurrentMpb).ToString("F2")),
        GUILayout.Width(200));
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      // --- 不兼容警告 ---
      if (IsBetterCalibrationActive())
      {
        GUILayout.Space(8);
        GUILayout.BeginHorizontal();
        GUILayout.Label(I18n.Tr("mod_incompatibility_warning"), WarningStyle,
          GUILayout.ExpandWidth(true), GUILayout.MinHeight(30));
        GUILayout.EndHorizontal();
        GUILayout.Space(4);
      }
    }

    public static int UserOffsetBeat => userOffsetBeat;

    private static void LoadSettings()
    {
      if (!File.Exists(settingsPath)) return;
      try
      {
        JObject obj = JObject.Parse(File.ReadAllText(settingsPath));

        userOffsetBeat = obj.Value<int?>("offsetBeat") ?? 0;
        CurrentMpb = obj.Value<int?>("currentMpb") ?? 400;
        I18n.Lang = obj.Value<string?>("language") ?? "en";
      }
      catch (Exception ex)
      {
        ModEntry.Logger.Log("LoadSettings error: " + ex);
      }
    }

    private static void SaveSettings()
    {
      try
      {
        JObject obj = new JObject
        {
          ["language"] = I18n.Lang,
          ["offsetBeat"] = userOffsetBeat,
          ["currentMpb"] = CurrentMpb
        };
        File.WriteAllText(settingsPath, obj.ToString());
      }
      catch (Exception ex)
      {
        ModEntry.Logger.Log("SaveSettings error: " + ex);
      }
    }
  }
}