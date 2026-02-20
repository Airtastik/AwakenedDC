using UnityEngine;

public class Room : MonoBehaviour
{
    public Transform north;
    public Transform south;
    public Transform east;
    public Transform west;

    public int DoorCount {
        get {
            int count = 0;

            if (north != null) count++;
            if (south != null) count++;
            if (east != null) count++;
            if (west != null) count++;

            return count;
        }
    }
}
