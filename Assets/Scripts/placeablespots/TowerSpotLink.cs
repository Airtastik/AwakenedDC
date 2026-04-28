using UnityEngine;

/// <summary>
/// Added automatically to a tower when it is placed.
/// Vacates the PlacementSpot when the tower is destroyed (e.g. sold).
/// </summary>
public class TowerSpotLink : MonoBehaviour
{
    public PlacementSpot spot;

    void OnDestroy()
    {
        spot?.Vacate();
    }
}
