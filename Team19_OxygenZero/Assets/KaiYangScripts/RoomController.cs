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

public class RoomController : MonoBehaviour
{
    [Header("Doors")]
    public GameObject topDoor, bottomDoor, leftDoor, rightDoor;
    public GameObject doorPrefab;

    [Header("Room Oxygen Settings")]
    public float oxygenConsumptionRate = 1f;

    [SerializeField] private List<GameObject> spawnedDoors = new List<GameObject>(); // List to store spawned doors

   

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

    [Header("Object Spawning")]
    public List<SpawnPointData> spawnPoints = new List<SpawnPointData>(); // Each spawn point has its own object list

    private void Start()
    {
        SpawnObjects();
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
                GameObject spawnedObject = Instantiate(selectedObject, spawnData.spawnPoint.position, Quaternion.identity, transform);

                // Add NavMeshObstacle component if it doesn't exist
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
                        obstacle.size = Vector3.Scale(boxCollider.size, spawnedObject.transform.localScale);
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
                obstacle.carving = true; // This makes the agent avoid the obstacle
                obstacle.carveOnlyStationary = true; // Only carve if object doesn't move

                // Rebuild NavMesh to include new obstacles
                StartCoroutine(RebuildNavMeshDelayed());
            }
        }
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



