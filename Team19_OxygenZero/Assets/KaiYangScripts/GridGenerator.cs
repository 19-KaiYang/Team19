using UnityEngine;
using System.Collections.Generic;

public class GridGenerator : MonoBehaviour
{
    public int innerGridSize = 3;
    public GameObject[] roomPrefabs;
    public GameObject pathPrefab;
    public GameObject shuttlePlatformPrefab;
    public float roomSpacing = 40f;
    public float pathWidth = 4f;
    public float pathLength = 10f;
    public float pathHeight = 0.01f;

    private RoomController[,] grid;
    private int totalGridSize;
    private (int x, int y) connectedRoomPosition; // Store the position of room connected to shuttle

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        totalGridSize = innerGridSize + 2;
        grid = new RoomController[totalGridSize, totalGridSize];
        connectedRoomPosition = (-1, -1); // Initialize to invalid position

        // Generate inner grid of rooms (3x3)
        for (int x = 1; x < totalGridSize - 1; x++)
        {
            for (int y = 1; y < totalGridSize - 1; y++)
            {
                GameObject selectedPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
                Vector3 worldPosition = new Vector3(x * roomSpacing, 0, y * roomSpacing);
                GameObject spawnedRoom = Instantiate(selectedPrefab, worldPosition, Quaternion.identity);
                grid[x, y] = spawnedRoom.GetComponent<RoomController>();
            }
        }

        // Find all possible shuttle positions
        List<(int x, int y, int rotation, int connectedX, int connectedY)> possibleShuttlePositions =
            new List<(int x, int y, int rotation, int connectedX, int connectedY)>();

        // Check all outer grid positions
        for (int x = 0; x < totalGridSize; x++)
        {
            for (int y = 0; y < totalGridSize; y++)
            {
                // Skip if not in outer ring
                if (x > 0 && x < totalGridSize - 1 && y > 0 && y < totalGridSize - 1) continue;

                // Check if position is adjacent to an existing room
                bool hasAdjacentRoom = false;
                int rotationAngle = 0;
                int connectedX = -1;
                int connectedY = -1;

                // Check right neighbor
                if (x < totalGridSize - 1 && grid[x + 1, y] != null)
                {
                    hasAdjacentRoom = true;
                    rotationAngle = 90;
                    connectedX = x + 1;
                    connectedY = y;
                }
                // Check left neighbor
                else if (x > 0 && grid[x - 1, y] != null)
                {
                    hasAdjacentRoom = true;
                    rotationAngle = -90;
                    connectedX = x - 1;
                    connectedY = y;
                }
                // Check top neighbor
                else if (y < totalGridSize - 1 && grid[x, y + 1] != null)
                {
                    hasAdjacentRoom = true;
                    rotationAngle = 180;
                    connectedX = x;
                    connectedY = y + 1;
                }
                // Check bottom neighbor
                else if (y > 0 && grid[x, y - 1] != null)
                {
                    hasAdjacentRoom = true;
                    rotationAngle = 0;
                    connectedX = x;
                    connectedY = y - 1;
                }

                if (hasAdjacentRoom)
                {
                    possibleShuttlePositions.Add((x, y, rotationAngle, connectedX, connectedY));
                }
            }
        }

        // Randomly select one of the possible positions and place the shuttle
        if (possibleShuttlePositions.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleShuttlePositions.Count);
            var selectedPosition = possibleShuttlePositions[randomIndex];

            // Store the connected room position
            connectedRoomPosition = (selectedPosition.connectedX, selectedPosition.connectedY);

            // Spawn shuttle platform
            Vector3 shuttlePosition = new Vector3(selectedPosition.x * roomSpacing, 0, selectedPosition.y * roomSpacing);
            GameObject shuttlePlatform = Instantiate(shuttlePlatformPrefab, shuttlePosition,
                Quaternion.Euler(0, selectedPosition.rotation, 0));

            // Create connecting path
            Vector3 pathPosition = shuttlePosition;
            if (selectedPosition.rotation == 90) // Facing right
                pathPosition += new Vector3(roomSpacing / 2, pathHeight / 2, 0);
            else if (selectedPosition.rotation == -90) // Facing left
                pathPosition += new Vector3(-roomSpacing / 2, pathHeight / 2, 0);
            else if (selectedPosition.rotation == 180) // Facing up
                pathPosition += new Vector3(0, pathHeight / 2, roomSpacing / 2);
            else // Facing down
                pathPosition += new Vector3(0, pathHeight / 2, -roomSpacing / 2);

            GameObject connectingPath = Instantiate(pathPrefab, pathPosition,
                Quaternion.Euler(0, selectedPosition.rotation == 0 || selectedPosition.rotation == 180 ? 90 : 0, 0));
            connectingPath.transform.localScale = new Vector3(pathLength, pathHeight, pathWidth);
        }

        // Generate paths between inner grid rooms
        for (int x = 1; x < totalGridSize - 1; x++)
        {
            for (int y = 1; y < totalGridSize - 1; y++)
            {
                if (x < totalGridSize - 2)
                {
                    Vector3 pathPos = new Vector3(
                        x * roomSpacing + roomSpacing / 2,
                        pathHeight / 2,
                        y * roomSpacing
                    );
                    GameObject horizontalPath = Instantiate(pathPrefab, pathPos, Quaternion.identity);
                    horizontalPath.transform.localScale = new Vector3(pathLength, pathHeight, pathWidth);
                }

                if (y < totalGridSize - 2)
                {
                    Vector3 pathPos = new Vector3(
                        x * roomSpacing,
                        pathHeight / 2,
                        y * roomSpacing + roomSpacing / 2
                    );
                    GameObject verticalPath = Instantiate(pathPrefab, pathPos, Quaternion.Euler(0, 90, 0));
                    verticalPath.transform.localScale = new Vector3(pathLength, pathHeight, pathWidth);
                }
            }
        }

        // Set doors for inner grid rooms
        for (int x = 1; x < totalGridSize - 1; x++)
        {
            for (int y = 1; y < totalGridSize - 1; y++)
            {
                RoomController room = grid[x, y];
                if (room != null)
                {
                    bool top = (y < totalGridSize - 2) && (grid[x, y + 1] != null);
                    bool bottom = (y > 1) && (grid[x, y - 1] != null);
                    bool left = (x > 1) && (grid[x - 1, y] != null);
                    bool right = (x < totalGridSize - 2) && (grid[x + 1, y] != null);

                    // If this is the room connected to the shuttle, open the appropriate door
                    if (x == connectedRoomPosition.x && y == connectedRoomPosition.y)
                    {
                        // Check which side the shuttle is on and open that door
                        if (grid[x + 1, y] == null && x < totalGridSize - 2) right = false; // Shuttle is on right
                        if (grid[x - 1, y] == null && x > 1) left = false; // Shuttle is on left
                        if (grid[x, y + 1] == null && y < totalGridSize - 2) top = false; // Shuttle is on top
                        if (grid[x, y - 1] == null && y > 1) bottom = false; // Shuttle is on bottom
                    }

                    room.SetDoors(!top, !bottom, !left, !right);
                }
            }
        }
    }
}