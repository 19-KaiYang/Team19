using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;

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
    public bool generateNavMesh = true;

    [Header("Enemy Spawning")]
    public GameObject[] enemyPrefabs;  // Array of different enemy types
    public float enemySpawnChance = 0.7f;  // 70% chance for a room to have enemies
    public int minEnemiesPerRoom = 0;
    public int maxEnemiesPerRoom = 3;

    private RoomController[,] grid;
    private int totalGridSize;
    private (int x, int y, string direction) shuttleConnection;
    private List<GameObject> allPaths = new List<GameObject>();
    public static GameObject ShuttlePlatform { get; private set; }

    void Start()
    {
        GenerateGrid();
    }

    private GameObject CreatePath(Vector3 position, Vector3 scale, Quaternion rotation)
    {
        GameObject path = Instantiate(pathPrefab, position, rotation);
        path.transform.localScale = scale;

        // Ensure path has required components for NavMesh
        if (!path.GetComponent<MeshRenderer>())
        {
            path.AddComponent<MeshRenderer>();
        }
        if (!path.GetComponent<MeshFilter>())
        {
            path.AddComponent<MeshFilter>();
        }

        allPaths.Add(path);
        return path;
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

                SpawnEnemiesInRoom(spawnedRoom);
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

            shuttleConnection = (selectedPosition.connectedX, selectedPosition.connectedY, selectedPosition.direction);

            Vector3 shuttlePosition = new Vector3(selectedPosition.x * roomSpacing, 0, selectedPosition.y * roomSpacing);

            int correctedRotation = selectedPosition.rotation;
            if (selectedPosition.direction == "left") correctedRotation = 90;
            if (selectedPosition.direction == "right") correctedRotation = 270;
            if (selectedPosition.direction == "bottom") correctedRotation = 0;
            if (selectedPosition.direction == "top") correctedRotation = 180;

            GameObject shuttlePlatform = Instantiate(shuttlePlatformPrefab, shuttlePosition,
                Quaternion.Euler(0, correctedRotation, 0));

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

            Vector3 pathPosition = shuttlePosition;
            float pathCenterOffset = 17.5f;
            float pathScaleLength = 15f;

            if (selectedPosition.rotation == 90) // Facing right
                pathPosition += new Vector3(pathCenterOffset, pathHeight / 2, 0);
            else if (selectedPosition.rotation == -90) // Facing left
                pathPosition += new Vector3(-pathCenterOffset, pathHeight / 2, 0);
            else if (selectedPosition.rotation == 180) // Facing up
                pathPosition += new Vector3(0, pathHeight / 2, pathCenterOffset);
            else // Facing down
                pathPosition += new Vector3(0, pathHeight / 2, -pathCenterOffset);

            GameObject connectingPath = CreatePath(
                pathPosition,
                new Vector3(pathScaleLength, pathHeight, pathWidth),
                Quaternion.Euler(0, (selectedPosition.rotation == 0 || selectedPosition.rotation == 180) ? 90 : 0, 0)
            );
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
                    CreatePath(
                        pathPos,
                        new Vector3(pathLength, pathHeight, pathWidth),
                        Quaternion.identity
                    );
                }

                if (y < totalGridSize - 2)
                {
                    Vector3 pathPos = new Vector3(
                        x * roomSpacing,
                        pathHeight / 2,
                        y * roomSpacing + roomSpacing / 2
                    );
                    CreatePath(
                        pathPos,
                        new Vector3(pathLength, pathHeight, pathWidth),
                        Quaternion.Euler(0, 90, 0)
                    );
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

                    if (x == shuttleConnection.x && y == shuttleConnection.y)
                    {
                        switch (shuttleConnection.direction)
                        {
                            case "top":
                                top = true;
                                break;
                            case "bottom":
                                bottom = true;
                                break;
                            case "left":
                                left = true;
                                break;
                            case "right":
                                right = true;
                                break;
                        }
                    }

                    room.SetDoors(top, bottom, left, right);
                }
            }
        }

        // Build NavMesh if needed
        if (generateNavMesh)
        {
            BuildNavMesh();
        }
    }

    private void SpawnEnemiesInRoom(GameObject room)
    {
        // First check if this room should have enemies
        if (Random.value > enemySpawnChance)
        {
            return; // Skip enemy spawning for this room
        }

        BoxCollider roomCollider = room.GetComponent<BoxCollider>();
        if (roomCollider == null)
        {
            Debug.LogError("Room prefab is missing a BoxCollider component!");
            return;
        }

        Bounds roomBounds = roomCollider.bounds;

        // Random number of enemies within min and max range
        int numberOfEnemies = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);

        for (int i = 0; i < numberOfEnemies; i++)
        {
            // Select random enemy type from prefabs array
            GameObject selectedEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            Vector3 randomPosition = new Vector3(
                Random.Range(roomBounds.min.x + 2f, roomBounds.max.x - 2f),
                roomBounds.min.y,
                Random.Range(roomBounds.min.z + 2f, roomBounds.max.z - 2f)
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, 2.0f, NavMesh.AllAreas))
            {
                GameObject enemy = Instantiate(selectedEnemyPrefab, hit.position + Vector3.up, Quaternion.identity);
                enemy.transform.SetParent(room.transform);
            }
        }
    }

    private void BuildNavMesh()
    {
        NavMeshSurface surface = FindObjectOfType<NavMeshSurface>();
        if (surface == null)
        {
            surface = gameObject.AddComponent<NavMeshSurface>();
        }

        surface.collectObjects = CollectObjects.All;
        surface.layerMask = -1; // All layers
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        surface.BuildNavMesh();
    }
}