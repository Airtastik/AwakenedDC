using UnityEngine;
using System.Collections.Generic;

// named as from->to, so like north exit to south entrance for NorthSouth
public const enum ConnectionOrientation
{
    NorthSouth,
    SouthNorth,
    EastWest,
    WestEast
}

// potential artifact of design, I'm not quite sure if this
// is ever going to get implemented, but the interface is cool
public const enum RoomType
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
    public int roomCount;

    // starts with a linear thread with no restrictions and then builds from there
    // needs to be less than or equal to roomCount
    // shouldn't be more than like 4 because you don't want the thread to get stuck on itself
    public int linearThreadLength;

    private Dictionary<Vector2Int, Room> roomLookup = new Dictionary<Vector2Int, Room>();

    private List<Room> spawnedRooms = new List<Room>();

    // maintains sets for rooms to safely select border room instances
    private HashSet<Room> noNorth = new HashSet<Room>();
    private HashSet<Room> noSouth = new HashSet<Room>();
    private HashSet<Room> noEast = new HashSet<Room>();
    private HashSet<Room> noWest = new HashSet<Room>();

    // don't really know how rooms are going to scale with vectors, so there needs to be a scalar multiple
    private const ROOM_SIZE_SCALAR = 1;

    void Start()
    {
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
        }
    }

    void GenerateRooms()
    {
        // parallel queues to maintain where rooms need to be added
        // check length of queue compared to length of spawned rooms and roomCount
        Queue<Vector3> positionsToAdd = new Queue<Vector3>();
        Queue<orientation> orientationsToAdd = new Queue<orientation>();

        Vector3 worldPosition = new Vector3(0, 0, 0);
        Room currentRoom = Instantiate(startRoomPrefab, worldPosition, Quaternion.identity);
        spawnedRooms.Add(currentRoom);

        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(worldPosition.x),
            Mathf.RoundToInt(worldPosition.z)
        );

        roomLookup[gridPos] = newRoom;
        spawnedRooms.Add(newRoom);
        populateQueues(currentRoom, positionsToAdd, orientationsToAdd);

        // for (int i = 0; i < linearThreadLength; i++) {
        //     currentRoom = linearThreadAlgorithm(currentRoom);
        //     populateQueues(currentRoom, positionsToAdd, orientationsToAdd);
        // }

        // when choosing rooms to add make sure there would be no obstruction a step in advance
        // the actual queue management of roomms to add
        while (positionsToAdd.Count > 0) {
            // decides what rooms to add based on presence in queue and amount of rooms to add
            // 1. cannot obstruct another room, but that shouldn't be possible
            // 2. if the generated room has border rooms. the borders need to align with the border rooms
            // 3. if we are nearing room "capacity," start adding rooms that don't add any new doorways
            // 4. one thread ends with a staircase to another floor, another ends with a treasure room

            

        }
            



    }

    Vector2Int getProspectivePivotPosition(Vector3 entranceDoorVector, orientation orientation) {
        Vector2Int pivotGrid = new Vector2Int(
            Mathf.RoundToInt(entranceDoorVector.x),
            Mathf.RoundToInt(entranceDoorVector.z)
        );

        Vector2Int prospectiveGrid;

        switch (orientation)
        {
            case ConnectionOrientation.NorthSouth:
                prospectiveGrid = pivotGrid + Vector2Int.up * ROOM_SIZE_SCALAR;
                break;

            case ConnectionOrientation.SouthNorth:
                prospectiveGrid = pivotGrid + Vector2Int.down * ROOM_SIZE_SCALAR;
                break;

            case ConnectionOrientation.EastWest:
                prospectiveGrid = pivotGrid + Vector2Int.right * ROOM_SIZE_SCALAR;
                break;

            case ConnectionOrientation.WestEast:
                prospectiveGrid = pivotGrid + Vector2Int.left * ROOM_SIZE_SCALAR;
                break;

            default:
                prospectiveGrid = pivotGrid;
                break;
        }

        return prospectiveGrid;
    }

    bool isWallNorth(Vector3 entranceDoorVector, orientation orientation) {
        Vector2Int northNeighborGrid = getProspectivePivotPosition + Vector2Int.up * ROOM_SIZE_SCALAR;
        if (roomLookup.TryGetValue(northNeighborGrid, out Room northRoom))
        {
            if (northRoom.south == null)
                return true;
        }

        return false;
    }

    bool isWallSouth(Vector3 entranceDoorVector, orientation orientation) {
        Vector2Int southNeighborGrid = getProspectivePivotPosition + Vector2Int.down * ROOM_SIZE_SCALAR;
        if (roomLookup.TryGetValue(southNeighborGrid, out Room northRoom))
        {
            if (northRoom.north == null)
                return true;
        }

        return false;
    } 

    bool isWallEast(Vector3 entranceDoorVector, orientation orientation) {
        Vector2Int eastNeighborGrid = getProspectivePivotPosition + Vector2Int.right * ROOM_SIZE_SCALAR;
        if (roomLookup.TryGetValue(eastNeighborGrid, out Room northRoom))
        {
            if (northRoom.west == null)
                return true;
        }

        return false;
    } 

    bool isWallWest(Vector3 entranceDoorVector, orientation orientation) {
        Vector2Int westNeighborGrid = getProspectivePivotPosition + Vector2Int.left * ROOM_SIZE_SCALAR;
        if (roomLookup.TryGetValue(westNeighborGrid, out Room northRoom))
        {
            if (northRoom.east == null)
                return true;
        }

        return false;
    } 

    void populateQueues(Room currentRoom, Queue<Vector3> positionsToAdd, Queue<orientation> orientationsToAdd) {
        if (currentRoom.north != null) {
            positionsToAdd.Enqueue(currentRoom.north);
            orientationsToAdd.Enqueue(ConnectionOrientation.NorthSouth);
        }
        if (currentRoom.south != null) {
            positionsToAdd.Enqueue(currentRoom.south);
            orientationsToAdd.Enqueue(ConnectionOrientation.SouthNorth);
        }
        if (currentRoom.east != null) {
            positionsToAdd.Enqueue(currentRoom.east);
            orientationsToAdd.Enqueue(ConnectionOrientation.EastWest);
        }
        if (currentRoom.west != null) {
            positionsToAdd.Enqueue(currentRoom.west);
            orientationsToAdd.Enqueue(ConnectionOrientation.WestEast);
        }
    }

    // develops a single linear thread as recurrently called
    // unfortunately priorities vertical orientation.
    Room linearThreadAlgorithm(Room currentRoom) {
        Room nextRoomPrefab;
        while (true) {

            nextRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            Room nextRoomInstance = Instantiate(nextRoomPrefab);

            List<ConnectionOrientation> orientations = new List<ConnectionOrientation>();
            Vector3 nextPosition;

            if (currentRoom.north != null && nextRoomPrefab.south != null) {
                orientations.Add(ConnectionOrientation.NorthSouth);
            }
            if (currentRoom.south != null && nextRoomPrefab.north != null) {
                orientations.Add(ConnectionOrientation.SouthNorth);
            }
            if (currentRoom.east != null && nextRoomPrefab.west != null) {
                orientations.Add(ConnectionOrientation.EastWest);
            }
            if (currentRoom.west != null && nextRoomPrefab.east != null) {
                orientations.Add(ConnectionOrientation.WestEast);
            }
            if (orientations.Count == 0) {
                Destroy(nextRoomInstance);
                continue;
            }

            ConnectionOrientation orientation = orientations[Random.Range(0, orientations.Count)];

            nextPosition = GetNextRoomPosition(currentRoom, nextRoomInstance, orientation);

            if (IsRoomObstructed(nextPosition))
            {
                Destroy(nextRoomInstance);
                continue; 
            }

            ConnectRooms(currentRoom, nextRoomInstance, orientation);

            spawnedRooms.Add(nextRoomInstance);
            return nextRoomInstance;
        }
    }

    // takes a vector and checks all spawned rooms for an obstruction at the vector
    bool IsRoomObstructed(Vector3 position) {
        foreach (Room r in spawnedRooms)
        {
            if (Vector3.Distance(r.transform.position, position) < 0.1f)
                return true;
        }
        return false;
    }

    // orients the rooms where they need to be
    void ConnectRooms(Room fromRoom, Room toRoom, ConnectionOrientation orientation) {
        Vector3 offset;
        
        if (orientation == ConnectionOrientation.NorthSouth) {
            offset = fromRoom.north.position - toRoom.south.position;
        } else if (orientation == ConnectionOrientation.SouthNorth) {
            offset = fromRoom.south.position - toRoom.north.position;
        } else if (orientation == ConnectionOrientation.EastWest) {
            offset = fromRoom.east.position - toRoom.west.position;
        } else {
            offset = fromRoom.west.position - toRoom.east.position;
        }

        toRoom.transform.position += offset;

    }
    
    // returns the connection point of where the toRoom is going to go
    Vector3 GetNextRoomPosition(Room fromRoom, Room toRoom, ConnectionOrientation orientation) {
        if (orientation == ConnectionOrientation.NorthSouth) {
            return fromRoom.north.position - (toRoom.south.position - toRoom.transform.position);
        } else if (orientation == ConnectionOrientation.SouthNorth) {
            return fromRoom.south.position - (toRoom.north.position - toRoom.transform.position);
        } else if (orientation == ConnectionOrientation.EastWest) {
            return fromRoom.east.position - (toRoom.west.position - toRoom.transform.position);
        } else {
            return fromRoom.west.position - (toRoom.east.position - toRoom.transform.position);
        }
    }
}
