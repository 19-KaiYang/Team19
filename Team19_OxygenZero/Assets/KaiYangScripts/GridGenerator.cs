using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public int gridSize = 3;
    public GameObject[] roomPrefabs; // Array of different room prefabs
    public GameObject pathPrefab;
    public float roomSpacing = 40f;
    public float pathWidth = 4f;
    public float pathLength = 10f;
    public float pathHeight = 0.01f;

    private RoomController[,] grid;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            Debug.LogError("No room prefabs assigned!");
            return;
        }

        grid = new RoomController[gridSize, gridSize];

        // First generate all rooms
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Randomly select a room prefab
                GameObject selectedPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
                Vector3 worldPosition = new Vector3(x * roomSpacing, 0, y * roomSpacing);
                GameObject spawnedRoom = Instantiate(selectedPrefab, worldPosition, Quaternion.identity);
                grid[x, y] = spawnedRoom.GetComponent<RoomController>();
            }
        }

        // Then add paths between connected rooms
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Add horizontal paths (connecting to right neighbor)
                if (x < gridSize - 1)
                {
                    Vector3 pathPos = new Vector3(
                        x * roomSpacing + roomSpacing / 2,
                        pathHeight / 2,
                        y * roomSpacing
                    );
                    GameObject horizontalPath = Instantiate(pathPrefab, pathPos, Quaternion.identity);
                    horizontalPath.transform.localScale = new Vector3(
                        pathLength,
                        pathHeight,
                        pathWidth
                    );
                }

                if (y < gridSize - 1)
                {
                    Vector3 pathPos = new Vector3(
                        x * roomSpacing,
                        pathHeight / 2,
                        y * roomSpacing + roomSpacing / 2
                    );
                    GameObject verticalPath = Instantiate(pathPrefab, pathPos, Quaternion.Euler(0, 90, 0));
                    verticalPath.transform.localScale = new Vector3(
                        pathLength,
                        pathHeight,
                        pathWidth
                    );
                }
            }
        }

        // Set doors
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                RoomController room = grid[x, y];
                if (room != null)
                {
                    bool top = (y < gridSize - 1) && (grid[x, y + 1] != null);
                    bool bottom = (y > 0) && (grid[x, y - 1] != null);
                    bool left = (x > 0) && (grid[x - 1, y] != null);
                    bool right = (x < gridSize - 1) && (grid[x + 1, y] != null);

                    room.SetDoors(!top, !bottom, !left, !right);
                }
            }
        }
    }
}