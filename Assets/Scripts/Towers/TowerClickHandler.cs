using UnityEngine;

/// <summary>
/// Attach to every tower prefab alongside a Collider.
/// When the player clicks the tower in the world, the HUD upgrade panel opens.
/// </summary>
[RequireComponent(typeof(TowerParent))]
public class TowerClickHandler : MonoBehaviour
{
    private TowerParent tower;

    void Awake() => tower = GetComponent<TowerParent>();

    void OnMouseDown()
    {
        TowerDefenseHUD.Instance?.SelectPlacedTower(tower);
    }
}
