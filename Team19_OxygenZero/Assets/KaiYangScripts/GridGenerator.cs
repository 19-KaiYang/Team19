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
    private (int x, int y, string direction) shuttleConnection;
    public static GameObject ShuttlePlatform { get; private set; }

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        totalGridSize = innerGridSize + 2;
        grid = new RoomController[totalGridSize, totalGridSize];
        shuttleConnection = (-1, -1, "none");

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
        List<(int x, int y, int rotation, int connectedX, int connectedY, string direction)> possibleShuttlePositions =
            new List<(int x, int y, int rotation, int connectedX, int connectedY, string direction)>();

        // Check all outer grid positions
        for (int x = 0; x < totalGridSize; x++)
        {
            for (int y = 0; y < totalGridSize; y++)
            {
                // Skip if not in outer ring
                if (x > 0 && x < totalGridSize - 1 && y > 0 && y < totalGridSize - 1) continue;

                // Check right neighbor
                if (x < totalGridSize - 1 && grid[x + 1, y] != null)
                {
                    possibleShuttlePositions.Add((x, y, 90, x + 1, y, "left"));
                }
                // Check left neighbor
                else if (x > 0 && grid[x - 1, y] != null)
                {
                    possibleShuttlePositions.Add((x, y, -90, x - 1, y, "right"));
                }
                // Check top neighbor
                else if (y < totalGridSize - 1 && grid[x, y + 1] != null)
                {
                    possibleShuttlePositions.Add((x, y, 180, x, y + 1, "bottom"));
                }
                // Check bottom neighbor
                else if (y > 0 && grid[x, y - 1] != null)
                {
                    possibleShuttlePositions.Add((x, y, 0, x, y - 1, "top"));
                }
            }
        }

        // Place shuttle and store connection info
        if (possibleShuttlePositions.Count > 0)
        {
            var selectedPosition = possibleShuttlePositions[Random.Range(0, possibleShuttlePositions.Count)];

            // Store connection information
            shuttleConnection = (selectedPosition.connectedX, selectedPosition.connectedY, selectedPosition.direction);

            // Spawn shuttle platform
            Vector3 shuttlePosition = new Vector3(selectedPosition.x * roomSpacing, 0, selectedPosition.y * roomSpacing);
            GameObject shuttlePlatform = Instantiate(shuttlePlatformPrefab, shuttlePosition,
                Quaternion.Euler(0, selectedPosition.rotation, 0));

            shuttlePlatform.tag = "ShuttlePlatform";
            ShuttlePlatform = shuttlePlatform;

            // Spawn player on shuttle
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = shuttlePlatform.transform.position + Vector3.up * 1f;
                player.transform.rotation = shuttlePlatform.transform.rotation;
            }
            else
            {
                Debug.LogError("Player not found in scene! Make sure it has the 'Player' tag.");
            }

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
                    // Default door states based on neighboring rooms
                    bool top = (y < totalGridSize - 2) && (grid[x, y + 1] != null);
                    bool bottom = (y > 1) && (grid[x, y - 1] != null);
                    bool left = (x > 1) && (grid[x - 1, y] != null);
                    bool right = (x < totalGridSize - 2) && (grid[x + 1, y] != null);

                    // If this is the room connected to the shuttle, modify its doors
                    if (x == shuttleConnection.x && y == shuttleConnection.y)
                    {
                        Debug.Log($"Setting doors for shuttle-connected room at ({x}, {y}), Direction: {shuttleConnection.direction}");

                        // Explicitly set the door state based on the shuttle connection direction
                        switch (shuttleConnection.direction)
                        {
                            case "top":
                                top = true;
                                Debug.Log("Opening top door for shuttle connection");
                                break;
                            case "bottom":
                                bottom = true;
                                Debug.Log("Opening bottom door for shuttle connection");
                                break;
                            case "left":
                                left = true;
                                Debug.Log("Opening left door for shuttle connection");
                                break;
                            case "right":
                                right = true;
                                Debug.Log("Opening right door for shuttle connection");
                                break;
                        }
                    }

                    // SetDoors expects: true to close, false to open
                    room.SetDoors(!top, !bottom, !left, !right);
                }
            }
        }
    }
}