using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// named as from->to, so like north exit to south entrance for NorthSouth
public enum orientation
{
    NorthSouth,
    SouthNorth,
    EastWest,
    WestEast,
    NULL
}

// potential artifact of design, I'm not quite sure if this
// is ever going to get implemented, but the interface is cool
public enum RoomType
{
    Start,
    Normal,
    Treasure,
    Final
}

public class RoomSpawner : MonoBehaviour
{
    // chosen room to start
    public Room startRoomPrefab;

    // list of all chosen rooms to choose from
    // should have at least 1 for each door structure
    // (so 24 rooms -1 for no doors)
    public List<Room> roomPrefabs;

    // amount of rooms on the floor (does not count starting room)
    // may go over if too many paths are initially generated. Think of it as a soft cap
    // roomCount = alpha <= beta | beta exists in the range (alpha, infinity)
    // beta would be spawnedRooms.Count
    public int roomCount; 

    private Dictionary<Vector2, Room> roomLookup = new Dictionary<Vector2, Room>();

    private List<Room> spawnedRooms = new List<Room>();

    // maintains sets for rooms to safely select border room instances
    private HashSet<Room> noNorth = new HashSet<Room>();
    private HashSet<Room> noSouth = new HashSet<Room>();
    private HashSet<Room> noEast = new HashSet<Room>();
    private HashSet<Room> noWest = new HashSet<Room>();
    private HashSet<Room> noDeadEnds = new HashSet<Room>();
    private HashSet<Room> yesDeadEnds = new HashSet<Room>();

    // don't really know how rooms are going to scale with vectors, so there needs to be a scalar multiple
    private float ROOM_SIZE_SCALAR;

    void Start() {
        ROOM_SIZE_SCALAR = ((startRoomPrefab.east.position - startRoomPrefab.west.position) / 2).magnitude;
        populateRoomBorderLists(); 
        GenerateRooms();
    }

    // realistically should be done at compile time, but I don't know how to do that
    void populateRoomBorderLists() {
        for (int i = 0; i < roomPrefabs.Count; i++) {
            if (roomPrefabs[i].north == null) {
                noNorth.Add(roomPrefabs[i]);
            }
            if (roomPrefabs[i].south == null) {
                noSouth.Add(roomPrefabs[i]);
            }
            if (roomPrefabs[i].east == null) {
                noEast.Add(roomPrefabs[i]);
            }
            if (roomPrefabs[i].west == null) {
                noWest.Add(roomPrefabs[i]);
            }
            if (roomPrefabs[i].DoorCount >= 2) {
                noDeadEnds.Add(roomPrefabs[i]);
            } else {
                yesDeadEnds.Add(roomPrefabs[i]);
            }
        }
    }

    void GenerateRooms() {
        // parallel queues to maintain where rooms need to be added
        // check length of queue compared to length of spawned rooms and roomCount
        Queue<Vector3> positionsToAdd = new Queue<Vector3>();
        Queue<orientation> orientationsToAdd = new Queue<orientation>();

        Vector3 worldPosition = new Vector3(0, 0, 0);
        Room firstRoom = Instantiate(startRoomPrefab, worldPosition, Quaternion.identity);
        RegisterRoom(firstRoom);
        populateQueues(firstRoom, positionsToAdd, orientationsToAdd, orientation.NULL);

        // when choosing rooms to add make sure there would be no obstruction a step in advance
        // the actual queue management of roomms to add
        // int deadEndCount = 0;
        while (positionsToAdd.Count > 0) {
            // decides what rooms to add based on presence in queue and amount of rooms to add
            // 1. cannot obstruct another room, but that shouldn't be possible
            // 2. if the generated room has border rooms. the borders need to align with the border rooms
            // 3. if we are nearing room "capacity," start adding rooms that don't add any new doorways
            // 4. one thread ends with a staircase to another floor, another ends with a treasure room

            Vector3 positionToAdd = positionsToAdd.Dequeue();
            orientation orientationToAdd = orientationsToAdd.Dequeue();

            HashSet<Room> validRooms = new HashSet<Room>();
            if (spawnedRooms.Count >= roomCount) {
                // start exlusively generating dead ends UNLESS there would be a connecting room
                validRooms.UnionWith(yesDeadEnds);
            } else {
                // require two or more doors for all generated rooms
                validRooms.UnionWith(noDeadEnds);
            }

            Vector2 prospectivePosition = getProspectivePivotPosition(positionToAdd, orientationToAdd);
            if (isWallNorth(prospectivePosition)) 
                validRooms.IntersectWith(noNorth);
            if (isWallSouth(prospectivePosition)) 
                validRooms.IntersectWith(noSouth);
            if (isWallEast(prospectivePosition)) 
                validRooms.IntersectWith(noEast);
            if (isWallWest(prospectivePosition)) 
                validRooms.IntersectWith(noWest);
            

            Room randomRoom = validRooms.ElementAt(Random.Range(0, validRooms.Count));
            Vector2 center = PivotToCenter(positionToAdd, orientationToAdd);

            // just an extra layer I guess
            if (roomLookup.ContainsKey(center))
                continue;

            Vector3 worldPos = CenterToWorld(center);
            Room newRoom = Instantiate(randomRoom, worldPos, Quaternion.identity);
            RegisterRoom(newRoom);
            populateQueues(newRoom, positionsToAdd, orientationsToAdd, orientationToAdd);
        }
            



    }

    Vector2 getProspectivePivotPosition(Vector3 entranceDoorVector, orientation orientation) {
        Vector2 pivotGrid = new Vector2(
            Mathf.RoundToInt(entranceDoorVector.x),
            Mathf.RoundToInt(entranceDoorVector.z)
        );

        Vector2 prospectiveGrid;

        switch (orientation) {
            case orientation.NorthSouth:
                prospectiveGrid = pivotGrid + Vector2.up * ROOM_SIZE_SCALAR;
                break;

            case orientation.SouthNorth:
                prospectiveGrid = pivotGrid + Vector2.down * ROOM_SIZE_SCALAR;
                break;

            case orientation.EastWest:
                prospectiveGrid = pivotGrid + Vector2.right * ROOM_SIZE_SCALAR;
                break;

            case orientation.WestEast:
                prospectiveGrid = pivotGrid + Vector2.left * ROOM_SIZE_SCALAR;
                break;

            default:
                prospectiveGrid = pivotGrid;
                break;
        }

        return prospectiveGrid;
    }

    bool isWallNorth(Vector3 pivot, orientation orientation) {
        return isWallNorth(PivotToCenter(pivot, orientation));
    }

    bool isWallSouth(Vector3 pivot, orientation orientation) {
        return isWallSouth(PivotToCenter(pivot, orientation));
    }

    bool isWallEast(Vector3 pivot, orientation orientation) {
        return isWallEast(PivotToCenter(pivot, orientation));
    }

    bool isWallWest(Vector3 pivot, orientation orientation) {
        return isWallWest(PivotToCenter(pivot, orientation));
    }


    bool isWallNorth(Vector2 center) {
        Vector2 neighbor = center + Vector2.up * ROOM_SIZE_SCALAR;

        if (roomLookup.TryGetValue(neighbor, out Room room))
            return room.south == null;

        return false;
    }

    bool isWallSouth(Vector2 center) {
        Vector2 neighbor = center + Vector2.down * ROOM_SIZE_SCALAR;

        if (roomLookup.TryGetValue(neighbor, out Room room))
            return room.north == null;

        return false;
    }

    bool isWallEast(Vector2 center) {
        Vector2 neighbor = center + Vector2.right * ROOM_SIZE_SCALAR;

        if (roomLookup.TryGetValue(neighbor, out Room room))
            return room.west == null;

        return false;
    }

    bool isWallWest(Vector2 center) {
        Vector2 neighbor = center + Vector2.left * ROOM_SIZE_SCALAR;

        if (roomLookup.TryGetValue(neighbor, out Room room))
            return room.east == null;

        return false;
    }

    void populateQueues(Room currentRoom, Queue<Vector3> positionsToAdd, Queue<orientation> orientationsToAdd, orientation omissionOrientation) {
        if (currentRoom.north != null && omissionOrientation != orientation.NorthSouth) {
            positionsToAdd.Enqueue(currentRoom.north.position);
            orientationsToAdd.Enqueue(orientation.NorthSouth);
        }
        if (currentRoom.south != null && omissionOrientation != orientation.SouthNorth) {
            positionsToAdd.Enqueue(currentRoom.south.position);
            orientationsToAdd.Enqueue(orientation.SouthNorth);
        }
        if (currentRoom.east != null && omissionOrientation != orientation.EastWest) {
            positionsToAdd.Enqueue(currentRoom.east.position);
            orientationsToAdd.Enqueue(orientation.EastWest);
        }
        if (currentRoom.west != null && omissionOrientation != orientation.WestEast) {
            positionsToAdd.Enqueue(currentRoom.west.position);
            orientationsToAdd.Enqueue(orientation.WestEast);
        }
    }

    // takes a vector and checks all spawned rooms for an obstruction at the vector
    bool IsRoomObstructed(Vector3 position) {
        foreach (Room r in spawnedRooms) {
            if (Vector3.Distance(r.transform.position, position) < 0.1f)
                return true;
        }
        return false;
    }

    // orients the rooms where they need to be
    void ConnectRooms(Room fromRoom, Room toRoom, orientation orientation) {
        Vector3 offset;
        
        if (orientation == orientation.NorthSouth) {
            offset = fromRoom.north.position - toRoom.south.position;
        } else if (orientation == orientation.SouthNorth) {
            offset = fromRoom.south.position - toRoom.north.position;
        } else if (orientation == orientation.EastWest) {
            offset = fromRoom.east.position - toRoom.west.position;
        } else {
            offset = fromRoom.west.position - toRoom.east.position;
        }

        toRoom.transform.position += offset;
    }
    
    // returns the connection point of where the toRoom is going to go
    Vector3 GetNextRoomPosition(Room fromRoom, Room toRoom, orientation orientation) {
        if (orientation == orientation.NorthSouth) {
            return fromRoom.north.position - (toRoom.south.position - toRoom.transform.position);
        } else if (orientation == orientation.SouthNorth) {
            return fromRoom.south.position - (toRoom.north.position - toRoom.transform.position);
        } else if (orientation == orientation.EastWest) {
            return fromRoom.east.position - (toRoom.west.position - toRoom.transform.position);
        } else {
            return fromRoom.west.position - (toRoom.east.position - toRoom.transform.position);
        }
    }

    // world pivot to grid pivot
    Vector2 PivotToGrid(Vector3 pivot) {
        return new Vector2(
            Mathf.RoundToInt(pivot.x),
            Mathf.RoundToInt(pivot.z)
        );
    }

    // pivot + orientation to CENTER grid position
    Vector2 PivotToCenter(Vector3 pivot, orientation orientation) {
        Vector2 pivotGrid = PivotToGrid(pivot);

        switch (orientation) {
            case orientation.NorthSouth:
                return pivotGrid + Vector2.up * ROOM_SIZE_SCALAR;

            case orientation.SouthNorth:
                return pivotGrid + Vector2.down * ROOM_SIZE_SCALAR;

            case orientation.EastWest:
                return pivotGrid + Vector2.right * ROOM_SIZE_SCALAR;

            case orientation.WestEast:
                return pivotGrid + Vector2.left * ROOM_SIZE_SCALAR;
        }

        return pivotGrid;
    }

    // center grid to world position
    Vector3 CenterToWorld(Vector2 center) {
        return new Vector3(center.x, 0, center.y);
    }

    void RegisterRoom(Room room) {
        Vector2 center = PivotToGrid(room.transform.position);

        roomLookup[center] = room;
        spawnedRooms.Add(room);
    }


}
