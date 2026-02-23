using UnityEngine;
using System.Collections.Generic;

public enum ConnectionOrientation
{
    NorthSouth,
    SouthNorth,
    EastWest,
    WestEast
}

public enum RoomType
{
    Start,
    Normal,
    Treasure,
    Final
}

public class RoomSpawner : MonoBehaviour
{

    public Room startRoomPrefab;
    public List<Room> roomPrefabs;
    public int roomCount;

    private List<Room> spawnedRooms = new List<Room>();
    

    void Start()
    {
        GenerateRooms();
    }

    void GenerateRooms()
    {
        Room currentRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        spawnedRooms.Add(currentRoom);

        for (int i = 0; i < roomCount; i++)
        {
            // develops a single linear thread of length roomCount
            // unfortunately priorities vertical orientation.

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

                currentRoom = nextRoomInstance;
                spawnedRooms.Add(currentRoom);
                break;
            }
            
        }
    }

    bool IsRoomObstructed(Vector3 position) {
        foreach (Room r in spawnedRooms)
        {
            if (Vector3.Distance(r.transform.position, position) < 0.1f)
                return true;
        }
        return false;
    }

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
