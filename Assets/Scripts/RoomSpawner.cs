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
    /// chosen room to start
    public Room startRoomPrefab;

    /// list of all chosen rooms to choose from
    /// should have at least 1 for each door structure
    /// (so 24 rooms -1 for no doors)
    public List<Room> roomPrefabs;

    /// amount of rooms on the floor (does not count starting room)
    /// may go over if too many paths are initially generated. Think of it as a soft cap
    /// roomCount = alpha <= beta | beta exists in the range (alpha, infinity)
    /// beta would be spawnedRooms.Count
    public int roomCount; 
    public double hallwayWeighting;

    private Dictionary<Vector2, Room> roomLookup = new Dictionary<Vector2, Room>();

    private List<Room> spawnedRooms = new List<Room>();

    /// maintains sets for rooms to safely select border room instances
    private HashSet<Room> noNorth = new HashSet<Room>();
    private HashSet<Room> noSouth = new HashSet<Room>();
    private HashSet<Room> noEast = new HashSet<Room>();
    private HashSet<Room> noWest = new HashSet<Room>();

    private HashSet<Room> hasNorth = new HashSet<Room>();
    private HashSet<Room> hasSouth = new HashSet<Room>();
    private HashSet<Room> hasEast = new HashSet<Room>();
    private HashSet<Room> hasWest = new HashSet<Room>();

    private HashSet<Room> noDeadEnds = new HashSet<Room>();
    private HashSet<Room> hallways = new HashSet<Room>();

    /// the width/height of each of the rooms for placement purposes
    private float ROOM_SIZE_SCALAR = 10;

    void Start() {
        populateRoomBorderLists(); 
        buildEnviornment(populateRoomMatrix());
    }

    /// realistically should be done at compile time, but I don't know how to do that
    /// just fills all of the various sets we have with rooms that match so we can 
    /// use set algebra on them during the generation process
    void populateRoomBorderLists() {
        for (int i = 0; i < roomPrefabs.Count; i++) {
            if (roomPrefabs[i].north == false) {
                noNorth.Add(roomPrefabs[i]);
            } else 
                hasNorth.Add(roomPrefabs[i]);
            if (roomPrefabs[i].south == false) {
                noSouth.Add(roomPrefabs[i]);
            } else 
                hasSouth.Add(roomPrefabs[i]);
            if (roomPrefabs[i].east == false) {
                noEast.Add(roomPrefabs[i]);
            } else 
                hasEast.Add(roomPrefabs[i]);
            if (roomPrefabs[i].west == false) {
                noWest.Add(roomPrefabs[i]);
            } else 
                hasWest.Add(roomPrefabs[i]);
            if (roomPrefabs[i].DoorCount >= 2) {
                noDeadEnds.Add(roomPrefabs[i]);
                if (roomPrefabs[i].DoorCount == 2) {
                    if ((roomPrefabs[i].north && roomPrefabs[i].south) || roomPrefabs[i].east && roomPrefabs[i].west)
                        hallways.Add(roomPrefabs[i]);
                }
            } 

        }
    }


    /// uses the room matrix to place all the rooms down in the enviornment
    void buildEnviornment(Room[,] populatedMatrix) {
        for (int x = 0; x < 25; x++) {
            for (int y = 0; y < 25; y++) {
                if (populatedMatrix[x, y] != null) {
                    Instantiate(populatedMatrix[x, y], new Vector3((ROOM_SIZE_SCALAR * x) - 12 * ROOM_SIZE_SCALAR, 0, (ROOM_SIZE_SCALAR * y) - 12 * ROOM_SIZE_SCALAR), Quaternion.identity);
                    
                }

            }

        }

    }

    /// the main algorithm for producing the randomly generated room layout
    /// all this handles is the actual room generation itself---not the enemies 
    /// or the items inside of the rooms
    Room[,] populateRoomMatrix() {
        Queue<int> xQueue = new Queue<int>();
        Queue<int> yQueue = new Queue<int>();

        Room[,] roomGrid = new Room[25, 25];
        addRoomToGrid(startRoomPrefab, 12, 12, xQueue, yQueue, roomGrid);

        int spawnedRoomsCount = 0;
        while (xQueue.Count > 0) {
            int x = xQueue.Dequeue();
            int y = yQueue.Dequeue();

            if (roomGrid[x, y] != null)
                continue;

            HashSet<Room> validRooms = new HashSet<Room>();

            if (x == 0) validRooms.IntersectWith(noWest);
            if (x == 24) validRooms.IntersectWith(noEast);
            if (y == 0) validRooms.IntersectWith(noSouth);
            if (y == 24) validRooms.IntersectWith(noNorth);

            bool onBorder = (x == 0 || x == 24 || y == 0 || y == 24);
            bool useDeadEnd = (spawnedRoomsCount >= roomCount || onBorder);

            if (useDeadEnd) 
                validRooms.UnionWith(roomPrefabs);
            else
                validRooms.UnionWith(noDeadEnds);

            if (y + 1 <= 24 && roomGrid[x, y + 1] != null) {
                if (roomGrid[x, y + 1].south)
                    validRooms.IntersectWith(hasNorth);
                else
                    validRooms.IntersectWith(noNorth);
            } else if (useDeadEnd) validRooms.IntersectWith(noNorth);
            if (y - 1 >= 0 && roomGrid[x, y - 1] != null) {
                if (roomGrid[x, y - 1].north)
                    validRooms.IntersectWith(hasSouth);
                else
                    validRooms.IntersectWith(noSouth);
            } else if (useDeadEnd) validRooms.IntersectWith(noSouth);
            if (x + 1 <= 24 && roomGrid[x + 1, y] != null) {
                if (roomGrid[x + 1, y].west)
                    validRooms.IntersectWith(hasEast);
                else
                    validRooms.IntersectWith(noEast);
            } else if (useDeadEnd) validRooms.IntersectWith(noEast);
            if (x - 1 >= 0 && roomGrid[x - 1, y] != null) {
                if (roomGrid[x - 1, y].east)
                    validRooms.IntersectWith(hasWest);
                else
                    validRooms.IntersectWith(noWest);
            } else if (useDeadEnd) validRooms.IntersectWith(noWest);

            HashSet<Room> validHallways = new HashSet<Room>();
            validHallways.UnionWith(hallways);
            validHallways.IntersectWith(validRooms);
            if (validHallways.Count > 0) {
                if (Random.Range(0, 100) < 100 * hallwayWeighting)
                    validRooms = validHallways;
            }

            if (validRooms.Count == 0) {
                Debug.LogError($"No valid rooms at {x},{y}");
                continue;
            }

            Room randomRoom = validRooms.ElementAt(Random.Range(0, validRooms.Count));
            addRoomToGrid(randomRoom, x, y, xQueue, yQueue, roomGrid);
            spawnedRoomsCount++;
            
            
        }

        return roomGrid;
        
    }

    /// private helper method for adding a room to the grid and then queuing all the new positions into it
    private void addRoomToGrid(Room room, int x, int y, Queue<int> xQueue, Queue<int> yQueue, Room[,] roomGrid) {
        if (roomGrid[x, y] != null)
            return;

        roomGrid[x, y] = room;

        if (room.north && y + 1 <= 24 && roomGrid[x, y + 1] == null) {
            xQueue.Enqueue(x);
            yQueue.Enqueue(y + 1);
        }
        if (room.south && y - 1 >= 0 && roomGrid[x, y - 1] == null) {
            xQueue.Enqueue(x);
            yQueue.Enqueue(y - 1);
        }
        if (room.east && x + 1 <= 24 && roomGrid[x + 1, y] == null) {
            xQueue.Enqueue(x + 1);
            yQueue.Enqueue(y);
        }
        if (room.west && x - 1 >= 0 && roomGrid[x - 1, y] == null) {
            xQueue.Enqueue(x - 1);
            yQueue.Enqueue(y);
        }

    }

}
