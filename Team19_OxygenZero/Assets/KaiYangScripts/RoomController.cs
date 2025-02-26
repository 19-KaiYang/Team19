using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class SpawnPointData
{
    public Transform spawnPoint; // The position where objects can spawn
    public GameObject[] possibleObjects; // Unique objects that can spawn here
}

[System.Serializable]
public class ScatteredObjectData
{
    public GameObject objectPrefab;
    public float spawnProbability = 0.5f; // Chance of this object spawning
    [Range(0, 10)]
    public int maxCount = 3; // Maximum number of this object type to spawn
}

public class RoomController : MonoBehaviour
{
    [Header("Doors")]
    public GameObject topDoor, bottomDoor, leftDoor, rightDoor;
    public GameObject doorPrefab;

    [Header("Room Oxygen Settings")]
    public float oxygenConsumptionRate = 1f;

    [SerializeField] private List<GameObject> spawnedDoors = new List<GameObject>(); // List to store spawned doors

    [Header("Original Object Spawning")]
    public List<SpawnPointData> spawnPoints = new List<SpawnPointData>(); // Each spawn point has its own object list

    [Header("Scattered Object Spawning")]
    public bool useScatteredObjects = true; // Toggle for scattered spawning
    public Transform roomCenter; // Central reference point
    public float spawnRadius = 5f; // How far from center objects can spawn
    public List<ScatteredObjectData> scatteredObjects = new List<ScatteredObjectData>();
    public float spawnHeightOffset = 0.5f; // Default height offset for spawned objects
    public int maxSpawnAttempts = 30; // Prevent infinite loops
    public LayerMask obstacleLayer; // Layer for collision checking
    public float minDistanceBetweenObjects = 1.5f; // Minimum distance between spawned objects

    private List<GameObject> spawnedObjects = new List<GameObject>();

    public void Awake()
    {
        Debug.Log("Starting door spawning...");
        spawnedDoors.Add(SpawnDoor(topDoor, Quaternion.Euler(0, -90, 0)));
        Debug.Log($"Top door added, count: {spawnedDoors.Count}");
        spawnedDoors.Add(SpawnDoor(bottomDoor, Quaternion.Euler(0, 90, 0)));
        Debug.Log($"Bottom door added, count: {spawnedDoors.Count}");
        spawnedDoors.Add(SpawnDoor(leftDoor, Quaternion.Euler(0, 0, 0)));
        Debug.Log($"Left door added, count: {spawnedDoors.Count}");
        spawnedDoors.Add(SpawnDoor(rightDoor, Quaternion.Euler(0, 0, 0)));
        Debug.Log($"Right door added, final count: {spawnedDoors.Count}");
    }

    private void Start()
    {
        // Use both spawning methods
        SpawnObjects(); // Original method

        if (useScatteredObjects)
        {
            SpawnScatteredObjects(); // New scattered method
        }
    }

    public void SetDoors(bool top, bool bottom, bool left, bool right)
    {
        if (doorPrefab != null)
        {
            Debug.Log($"Current spawnedDoors count: {spawnedDoors.Count}");

            if (spawnedDoors.Count < 4)
            {
                Debug.LogError("Not all doors were properly spawned!");
                return;
            }

            // Get the DoorScript for each door and set its status
            if (spawnedDoors[0] != null)
            {
                DoorScript doorScript = spawnedDoors[0].GetComponent<DoorScript>();
                if (doorScript != null) doorScript.SetDoorStatus(top);
            }

            if (spawnedDoors[1] != null)
            {
                DoorScript doorScript = spawnedDoors[1].GetComponent<DoorScript>();
                if (doorScript != null) doorScript.SetDoorStatus(bottom);
            }

            if (spawnedDoors[2] != null)
            {
                DoorScript doorScript = spawnedDoors[2].GetComponent<DoorScript>();
                if (doorScript != null) doorScript.SetDoorStatus(left);
            }

            if (spawnedDoors[3] != null)
            {
                DoorScript doorScript = spawnedDoors[3].GetComponent<DoorScript>();
                if (doorScript != null) doorScript.SetDoorStatus(right);
            }
        }
    }

    private GameObject SpawnDoor(GameObject doorPosition, Quaternion rotation)
    {
        if (doorPosition != null)
        {
            return Instantiate(doorPrefab, doorPosition.transform.position, rotation, doorPosition.transform);
        }
        Debug.LogWarning($"Door position was null when trying to spawn door");
        return null;
    }

    private void SpawnObjects()
    {
        foreach (SpawnPointData spawnData in spawnPoints)
        {
            if (spawnData.spawnPoint == null || spawnData.possibleObjects.Length == 0)
                continue;

            float spawnChance = Random.value;

            if (spawnChance < 0.33f)
            {
                continue;
            }
            else
            {
                GameObject selectedObject = spawnData.possibleObjects[Random.Range(0, spawnData.possibleObjects.Length)];

                // Calculate spawn position with height offset
                Vector3 spawnPosition = spawnData.spawnPoint.position;

                // Try to get renderer from the prefab to calculate proper height
                Renderer prefabRenderer = selectedObject.GetComponent<Renderer>();
                if (prefabRenderer != null)
                {
                    // Use half the height of the renderer bounds as offset
                    float objectHeight = prefabRenderer.bounds.size.y;
                    spawnPosition.y += objectHeight / 2;
                }
                else
                {
                    // Fall back to default offset if no renderer found
                    spawnPosition.y += spawnHeightOffset;
                }

                // Instantiate the object at the adjusted position
                GameObject spawnedObject = Instantiate(selectedObject, spawnPosition, Quaternion.identity, transform);
                spawnedObjects.Add(spawnedObject);

                // Add NavMeshObstacle component if it doesn't exist
                ConfigureNavMeshObstacle(spawnedObject);
            }
        }
    }

    private void SpawnScatteredObjects()
    {
        if (roomCenter == null)
        {
            Debug.LogError("Room center transform is not assigned!");
            return;
        }

        foreach (ScatteredObjectData objectData in scatteredObjects)
        {
            int objectsToSpawn = Random.Range(0, objectData.maxCount + 1);

            for (int i = 0; i < objectsToSpawn; i++)
            {
                // Check spawn probability
                if (Random.value > objectData.spawnProbability)
                    continue;

                // Try to find a valid spawn position
                Vector3 spawnPosition;
                bool validPositionFound = false;
                int attempts = 0;

                do
                {
                    // Generate random position within radius
                    Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                    spawnPosition = roomCenter.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                    // Check if position is valid (not overlapping other objects)
                    validPositionFound = IsValidSpawnPosition(spawnPosition, objectData.objectPrefab);
                    attempts++;

                } while (!validPositionFound && attempts < maxSpawnAttempts);

                if (validPositionFound)
                {
                    // Adjust height based on object
                    Renderer prefabRenderer = objectData.objectPrefab.GetComponent<Renderer>();
                    if (prefabRenderer != null)
                    {
                        float objectHeight = prefabRenderer.bounds.size.y;
                        spawnPosition.y += objectHeight / 2;
                    }
                    else
                    {
                        spawnPosition.y += spawnHeightOffset;
                    }

                    // Spawn object with random rotation around Y axis
                    Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    GameObject spawnedObject = Instantiate(objectData.objectPrefab, spawnPosition, randomRotation, transform);
                    spawnedObjects.Add(spawnedObject);

                    // Add NavMeshObstacle
                    ConfigureNavMeshObstacle(spawnedObject);
                }
            }
        }

        // Rebuild NavMesh once after all objects are spawned
        StartCoroutine(RebuildNavMeshDelayed());
    }

    private bool IsValidSpawnPosition(Vector3 position, GameObject objectToSpawn)
    {
        // Get approximate size of object
        float objectRadius = 0.5f;
        Renderer prefabRenderer = objectToSpawn.GetComponent<Renderer>();
        if (prefabRenderer != null)
        {
            // Use the largest dimension as a radius check
            Vector3 size = prefabRenderer.bounds.size;
            objectRadius = Mathf.Max(size.x, size.z) / 2;
        }

        // Check for obstacles and other spawned objects
        Collider[] hitColliders = Physics.OverlapSphere(position, objectRadius + minDistanceBetweenObjects, obstacleLayer);
        if (hitColliders.Length > 0)
            return false;

        // Check distance from doors
        foreach (GameObject door in spawnedDoors)
        {
            if (door != null)
            {
                float distanceToDoor = Vector3.Distance(position, door.transform.position);
                if (distanceToDoor < 2f) // Keep area around doors clear
                    return false;
            }
        }

        // Check distance from other spawned objects
        foreach (GameObject obj in spawnedObjects)
        {
            float distanceToObject = Vector3.Distance(position, obj.transform.position);
            if (distanceToObject < minDistanceBetweenObjects)
                return false;
        }

        return true;
    }

    private void ConfigureNavMeshObstacle(GameObject spawnedObject)
    {
        NavMeshObstacle obstacle = spawnedObject.GetComponent<NavMeshObstacle>();
        if (obstacle == null)
        {
            obstacle = spawnedObject.AddComponent<NavMeshObstacle>();
        }

        // Configure the obstacle
        Collider objectCollider = spawnedObject.GetComponent<Collider>();
        if (objectCollider != null)
        {
            // Match obstacle size to collider
            if (objectCollider is BoxCollider boxCollider)
            {
                obstacle.size = boxCollider.size;
                obstacle.center = boxCollider.center;
            }
            else
            {
                // Default size for other collider types
                obstacle.radius = 0.5f;
                obstacle.height = 2f;
            }
        }

        // Set obstacle properties
        obstacle.carving = true;
        obstacle.carveOnlyStationary = true;
    }

    private System.Collections.IEnumerator RebuildNavMeshDelayed()
    {
        // Wait for a frame to ensure all objects are properly placed
        yield return new WaitForEndOfFrame();

        // Find and rebuild the NavMeshSurface
        NavMeshSurface surface = GetComponentInParent<NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OxygenSystem playerOxygen = other.GetComponent<OxygenSystem>();
            if (playerOxygen != null)
            {
                playerOxygen.SetOxygenConsumptionRate(oxygenConsumptionRate);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OxygenSystem playerOxygen = other.GetComponent<OxygenSystem>();
            if (playerOxygen != null)
            {
                playerOxygen.SetOxygenConsumptionRate(playerOxygen.defaultConsumptionRate);
            }
        }
    }
}