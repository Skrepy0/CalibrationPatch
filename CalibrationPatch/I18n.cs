using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CalibrationPatch
{
  public static class I18n
  {
    private static readonly Dictionary<string, string> en = new()
    {
      ["language"] = "Language",
      ["mpb"] = "ms/beat",
      ["calibration_bpm"] = "BPM = {0}",
      ["offset_beat"] = "Beat Offset",
      ["mod_incompatibility_warning"] = "BetterCalibration mod detected, which may interfere with CalibrationPatch."
    };

    private static readonly Dictionary<string, string> zh = new()
    {
      ["language"] = "语言",
      ["mpb"] = "每拍毫秒数",
      ["calibration_bpm"] = "BPM = {0}",
      ["offset_beat"] = "偏移拍数",
      ["mod_incompatibility_warning"] = "检测到 BetterCalibration 模组，可能影响 CalibrationPatch 的正常功能。"
    };

    private static readonly Dictionary<string, string> ko = new()
    {
      ["language"] = "언어",
      ["mpb"] = "비트당 밀리초",
      ["calibration_bpm"] = "BPM = {0}",
      ["offset_beat"] = "비트 오프셋",
      ["mod_incompatibility_warning"] = "BetterCalibration 모드가 감지되었습니다. CalibrationPatch 기능에 영향을 줄 수 있습니다."
    };

    private static string _lang = "en";

    public static string Lang
    {
      get => _lang;
      set
      {
        if (value == "zh" || value == "ko" || value == "en")
          _lang = value;
        else
          _lang = "en";
      }
    }

    private static Dictionary<string, string> CurrentDict => Lang switch
    {
      "zh" => zh,
      "ko" => ko,
      _ => en
    };

    public static void Load(string modPath)
    {
      string path = Path.Combine(modPath, "lang", "lang.json");
      if (!File.Exists(path))
      {
        Debug.Log($"[{Main.ModId}::I18n] Language file not found at: " + path);
        return;
      }

      try
      {
        string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
        Debug.Log($"[{Main.ModId}::I18n] Loaded language file, size: {json.Length} bytes");

        var root = JObject.Parse(json);
        var entriesToken = root["entries"];
        if (entriesToken == null || !entriesToken.HasValues)
        {
          Debug.LogWarning($"[{Main.ModId}::I18n] 'entries' field is missing or empty.");
          Debug.LogWarning(
            $"[{Main.ModId}::I18n] JSON content preview: {json.Substring(0, Math.Min(200, json.Length))}...");
          return;
        }

        int count = 0;
        foreach (var entryToken in entriesToken)
        {
          string key = entryToken["key"]?.Value<string>();
          if (string.IsNullOrEmpty(key)) continue;

          string enVal = entryToken["en"]?.Value<string>();
          string zhVal = entryToken["zh"]?.Value<string>();
          string koVal = entryToken["ko"]?.Value<string>();

          if (!string.IsNullOrEmpty(enVal))
          {
            en[key] = enVal;
            count++;
          }

          if (!string.IsNullOrEmpty(zhVal))
          {
            zh[key] = zhVal;
            count++;
          }

          if (!string.IsNullOrEmpty(koVal))
          {
            ko[key] = koVal;
            count++;
          }
        }

        Debug.Log($"[{Main.ModId}::I18n] Successfully applied {count} translation entries from external file.");
      }
      catch (Exception ex)
      {
        Debug.LogError($"[{Main.ModId}::I18n] Failed to load language file: {ex.Message}");
        Debug.LogError($"[{Main.ModId}::I18n] Stack trace: {ex.StackTrace}");
        try
        {
          string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
          Debug.LogError($"[{Main.ModId}::I18n] Full JSON content:\n{json}");
        }
        catch
        {
        }
      }
    }

    public static string Tr(string key)
    {
      return CurrentDict.TryGetValue(key, out var val) ? val : key;
    }

    [Serializable]
    private class LangFile
    {
      public LangEntry[] entries;
    }

    [Serializable]
    private class LangEntry
    {
      public string key;
      public string en;
      public string zh;
      public string ko;
    }
  }
}