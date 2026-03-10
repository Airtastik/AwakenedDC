using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// named as from->to, so like north exit to south entrance for NorthSouth
public enum orientation
{
    NorthSouth,
    SouthNorth,
    EastWest,
    WestEast,
    NULL
}

/// potential artifact of design, I'm not quite sure if this
/// is ever going to get implemented, but the interface is cool
public enum RoomType
{
    Start,
    Normal,
    Treasure,
    Final
}

public struct RoomDecision
{
    public bool isStaircase;
    public bool isTreasure;
    public int enemyCount;
    public int pickupCount;
}

public class RoomSpawner : MonoBehaviour
{
    /// chosen room to start
    public Room startRoomPrefab;

    /// list of all chosen rooms to choose from
    /// should have at least 1 for each door structure
    /// (so 24 rooms -1 for no doors)
    public List<Room> roomPrefabs;

    /// list of all enemies willing to spawn
    public List<DungeonEnemyAI> enemies;

    /// list of all pickups willing to spawn
    public List<WorldItem> pickups;

    /// the treasure prefab being instantiated
    public GameObject treasurePrefab;

    /// the staircase prefab being instantiated
    public GameObject staircasePrefab;

    /// amount of rooms on the floor (does not count starting room)
    /// may go over if too many paths are initially generated. Think of it as a soft cap
    /// roomCount = alpha <= beta | beta exists in the range (alpha, infinity)
    /// beta would be spawnedRooms.Count
    public int roomCount; 

    /// double representation of the weight we want the linear hallway room type to have
    /// should be between 0 and 1, and is really just an aesthetical decision, except it will 
    /// affect gamma (number of dead end rooms) by some summative polynomial.
    public double hallwayWeighting;


    /// !!!
    /// all randomization code is based on a repeated chance for multiple occurences replicating
    /// a binomial expansion. So the inputted value is the chance one instance will spawn in that 
    /// room, but the standardization of that probability for multiple possibilities occurs within
    /// the code itself
    /// !!!


    /// a hardcap on the maximum amount of enemies spawned, awaiting should be significantly
    /// greater than wandering
    public int totalEnemyMax;

    /// chance an enemy will spawn in a non dead-end room
    public double wanderingEnemySpawnChance;

    /// chance an enemy will spawn in a dead-end room
    public double awaitingEnemySpawnChance;

    /// a hardcap on the maximum amount of pickups spawned, awaiting should be significantly
    /// greater than wandering
    public int totalPickupMax;

    /// chance a pickup will spawn in a non dead-end room
    public double wanderingPickupSpawnChance;

    /// chance a pickup will spawn in a dead-end room
    public double awaitingPickupSpawnChance;

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
    private const int ROOM_SIZE_SCALAR = 10;
    private const int CENTRAL_ROOM_POSITION = 12 * ROOM_SIZE_SCALAR;
    private const int ROOM_OFFSET_RANGE = ROOM_SIZE_SCALAR / 2 - 1;

    void Start() {
        populateRoomBorderLists(); 
        Room[,] rooms = populateRoomMatrix();
        RoomDecision[,] decisions = makeRoomDecisions(rooms);
        buildEnviornment(rooms, decisions);
    }

    /// realistically should be done at compile time, but I don't know how to do that
    /// just fills all of the various sets we have with rooms that match so we can 
    /// use set algebra on them during the generation process
    private void populateRoomBorderLists() {
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

    /// relevant to the !!! header above, just recalculates the given probability, omega, into one that 
    /// allows for multiple possibilities without being aggressive
    private double effectiveProbability(double cumulativeProbability) {
        return (double) (2 * Mathf.Pow((float) cumulativeProbability, 2) * (1 - (float) cumulativeProbability));
            
    }


    /// uses the room matrix to place all the rooms and their contents down in the enviornment
    private void buildEnviornment(Room[,] populatedMatrix, RoomDecision[,] decisionMatrix)
    {
        for (int x = 0; x < 25; x++)
        {
            for (int y = 0; y < 25; y++)
            {
                if (populatedMatrix[x, y] != null)
                {
                    // should spawn the center of the prefab room so offsets will be abs() <= ROOM_SIZE_SCALAR / 2
                    Room room = populatedMatrix[x, y];
                    RoomDecision roomDecision = decisionMatrix[x, y];

                    Instantiate(room, new Vector3((ROOM_SIZE_SCALAR * x) - CENTRAL_ROOM_POSITION, 0,
                        (ROOM_SIZE_SCALAR * y) - CENTRAL_ROOM_POSITION), Quaternion.identity);
                    // ROOM_OFFSET_RANGE
                    if (roomDecision.isStaircase)
                    {
                        // Debug.LogError($"staircase chosen at {x}, {y}");
                        // spawn staircase
                        Instantiate(staircasePrefab, new Vector3((ROOM_SIZE_SCALAR * x) - CENTRAL_ROOM_POSITION, 0,
                            (ROOM_SIZE_SCALAR * y) - CENTRAL_ROOM_POSITION), Quaternion.identity);
                    }
                    else if (roomDecision.isTreasure)
                    {
                        // Debug.LogError($"treasure chosen at {x}, {y}");
                        // spawn treasure
                        Instantiate(treasurePrefab, new Vector3((ROOM_SIZE_SCALAR * x) - CENTRAL_ROOM_POSITION, 0,
                            (ROOM_SIZE_SCALAR * y) - CENTRAL_ROOM_POSITION), Quaternion.identity);
                    }
                }

            }

        }

        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
        }
        else
        {
            Debug.LogWarning("No NavMeshSurface found on RoomSpawner.");
        }

        for (int x = 0; x < 25; x++)
        {
            for (int y = 0; y < 25; y++)
            {
                if (populatedMatrix[x, y] != null)
                {
                    RoomDecision roomDecision = decisionMatrix[x, y];
                    if (!roomDecision.isStaircase && !roomDecision.isTreasure)
                    {
                        for (int i = 0; i < decisionMatrix[x, y].enemyCount; i++)
                        {
                            float spawnX = (ROOM_SIZE_SCALAR * x) - CENTRAL_ROOM_POSITION + UnityEngine.Random.Range(-ROOM_OFFSET_RANGE, ROOM_OFFSET_RANGE);
                            float spawnZ = (ROOM_SIZE_SCALAR * y) - CENTRAL_ROOM_POSITION + UnityEngine.Random.Range(-ROOM_OFFSET_RANGE, ROOM_OFFSET_RANGE);

                            Vector3 spawnPos = new Vector3(spawnX, 5.0f, spawnZ);
                            DungeonEnemyAI newEnemy = Instantiate(enemies[UnityEngine.Random.Range(0, enemies.Count)], spawnPos, Quaternion.identity);

                            NavMeshAgent agent = newEnemy.GetComponent<NavMeshAgent>();

                            if (agent != null)
                            {
                                NavMeshHit hit;
                                if (NavMesh.SamplePosition(spawnPos, out hit, 10.0f, NavMesh.AllAreas))
                                {
                                    newEnemy.transform.position = hit.position;
                                    agent.enabled = true;
                                    agent.Warp(hit.position);
                                    newEnemy.currentState = DungeonEnemyAI.EnemyState.Patrol;
                                }
                            }
                        }
                        // Debug.LogError($"{decisionMatrix[x, y].enemyCount} enemies chosen at {x}, {y}");

                        for (int i = 0; i < decisionMatrix[x, y].pickupCount; i++)
                        {
                            Instantiate(pickups[UnityEngine.Random.Range(0, pickups.Count)], new Vector3((ROOM_SIZE_SCALAR * x) - CENTRAL_ROOM_POSITION + UnityEngine.Random.Range(-ROOM_OFFSET_RANGE, ROOM_OFFSET_RANGE),
                                0.5f, (ROOM_SIZE_SCALAR * y) - CENTRAL_ROOM_POSITION + UnityEngine.Random.Range(-ROOM_OFFSET_RANGE, ROOM_OFFSET_RANGE)), Quaternion.identity);
                        }
                        // Debug.LogError($"{decisionMatrix[x, y].pickupCount} pickups chosen at {x}, {y}");
                    }
                }
            }
        }
    }

    /// just a generic shuffle method to effectively randomize and weight
    /// dead ends with more precedence over non-dead ends
    private void randomizeList(List<Vector2Int> list) {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1); 
            Vector2Int value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// further populates the room matrix by deciding on conditions and modifiers to the rooms
    /// such as enemies, pickups, and fianl room type
    private RoomDecision[,] makeRoomDecisions(Room[,] populatedMatrix) {
        int width = populatedMatrix.GetLength(0);
        int height = populatedMatrix.GetLength(1);

        RoomDecision[,] decisionMatrix = new RoomDecision[width, height];

        List<Vector2Int> allDeadEnds = new List<Vector2Int>();
        List<Vector2Int> allElse = new List<Vector2Int>();
        int spawnedEnemies = 0;
        int spawnedPickups = 0;

        for (int x = 0; x < 25; x++) {
            for (int y = 0; y < 25; y++) {
                if (x == 12 && y == 12)
                    continue;
                if (populatedMatrix[x, y] != null) {
                    if (populatedMatrix[x, y].DoorCount == 1) {
                        allDeadEnds.Add(new Vector2Int(x, y));
                    }
                    else
                        allElse.Add(new Vector2Int(x, y));
                } 

            }

        }

        randomizeList(allDeadEnds);
        randomizeList(allElse);

        Vector2Int pos = allDeadEnds[0];
        decisionMatrix[pos.x, pos.y].isStaircase = true;

        pos = allDeadEnds[1];
        decisionMatrix[pos.x, pos.y].isTreasure = true;

        while (spawnedEnemies < (totalEnemyMax / 2) || spawnedPickups < (totalPickupMax / 2)) {

            for (int i = 2; i < allDeadEnds.Count; i++) {
                pos = allDeadEnds[i];

                // generate possible enemy and treasure based on dead end
                while (spawnedEnemies <= totalEnemyMax && UnityEngine.Random.Range(0, 100) < 100 * effectiveProbability(awaitingEnemySpawnChance)) {
                    decisionMatrix[pos.x, pos.y].enemyCount++;
                    spawnedEnemies++;
                }
                    
                while (spawnedPickups <= totalPickupMax && UnityEngine.Random.Range(0, 100) < 100 * effectiveProbability(awaitingPickupSpawnChance)) {
                    decisionMatrix[pos.x, pos.y].pickupCount++;
                    spawnedPickups++;
                }
                
            }

            for (int i = 0; i < allElse.Count; i++) {
                pos = allElse[i];

                // generate possible enemy and treasure based on non dead-end
                while (spawnedEnemies <= totalEnemyMax && UnityEngine.Random.Range(0, 100) < 100 * effectiveProbability(wanderingEnemySpawnChance)) {
                    decisionMatrix[pos.x, pos.y].enemyCount++;
                    spawnedEnemies++;
                }
                    
                while (spawnedPickups <= totalPickupMax && UnityEngine.Random.Range(0, 100) < 100 * effectiveProbability(wanderingPickupSpawnChance)) {
                    decisionMatrix[pos.x, pos.y].pickupCount++;
                    spawnedPickups++;
                }
            }
        }

        return decisionMatrix;

    }

    /// the main algorithm for producing the randomly generated room layout
    /// all this handles is the actual room generation itself---not the enemies 
    /// or the items inside of the rooms
    private Room[,] populateRoomMatrix() {
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
                if (UnityEngine.Random.Range(0, 100) < 100 * hallwayWeighting)
                    validRooms = validHallways;
            }

            if (validRooms.Count == 0) {
                Debug.LogError($"No valid rooms at {x},{y}");
                continue;
            }

            Room randomRoom = validRooms.ElementAt(UnityEngine.Random.Range(0, validRooms.Count));

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
