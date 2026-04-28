#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Right-click "Placeable Spots" in the Hierarchy →
/// "Setup → Add PlacementSpot to Children"
/// to add PlacementSpot to every child cube in one click.
/// </summary>
public static class PlacementSpotSetup
{
    [MenuItem("GameObject/Setup/Add PlacementSpot to Children", false, 0)]
    static void AddToChildren()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null) { Debug.LogWarning("No GameObject selected."); return; }

        int count = 0;
        foreach (Transform child in selected.transform)
        {
            if (child.GetComponent<PlacementSpot>() == null)
            {
                child.gameObject.AddComponent<PlacementSpot>();
                count++;
            }
        }

        Debug.Log($"[PlacementSpotSetup] Added PlacementSpot to {count} children of '{selected.name}'.");
    }
}
#endif
