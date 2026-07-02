using UnityModManagerNet;
using UnityEngine;

namespace CalibrationPatch
{
  public class Main
  {
    public const string ModId = "CalibrationPatch";

    private static UnityModManager.ModEntry modEntry;

    public static bool Load(UnityModManager.ModEntry entry)
    {
      modEntry = entry;
      entry.OnGUI = OnGUI;
      modEntry.Logger.Log($"{ModId} loaded.");
      return true;
    }

    private static void OnGUI(UnityModManager.ModEntry entry)
    {
      if (!modEntry.Active) return;
      GUILayout.BeginHorizontal();
      GUILayout.Label("test", GUILayout.Width(100));
      GUILayout.EndHorizontal();
    }
  }
}