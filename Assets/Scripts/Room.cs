using UnityEngine;

public class Room : MonoBehaviour
{
    public bool north;
    public bool south;
    public bool east;
    public bool west;

    public int DoorCount {
        get {
            int count = 0;

            if (north) count++;
            if (south) count++;
            if (east) count++;
            if (west) count++;

            return count;
        }
    }
}
