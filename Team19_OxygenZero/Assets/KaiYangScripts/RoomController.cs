using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Object Spawning")]
    public List<SpawnPointData> spawnPoints = new List<SpawnPointData>(); // Each spawn point has its own object list

    private void Start()
    {
        SpawnObjects();
    }

    public void SetDoors(bool top, bool bottom, bool left, bool right)
    {
        if (topDoor) topDoor.SetActive(top);
        if (bottomDoor) bottomDoor.SetActive(bottom);
        if (leftDoor) leftDoor.SetActive(left);
        if (rightDoor) rightDoor.SetActive(right);
    }

    private void SpawnObjects()
    {
        foreach (SpawnPointData spawnData in spawnPoints)
        {
            if (spawnData.spawnPoint == null || spawnData.possibleObjects.Length == 0)
                continue; // Skip if no spawn point or objects available

            float spawnChance = Random.value; // Generates a random number between 0 and 1

            if (spawnChance < 0.33f)
            {
                // 33% chance to spawn nothing (skip this spawn point)
                continue;
            }
            else
            {
                // 67% chance to spawn an object from this specific spawn point's list
                GameObject selectedObject = spawnData.possibleObjects[Random.Range(0, spawnData.possibleObjects.Length)];
                Instantiate(selectedObject, spawnData.spawnPoint.position, Quaternion.identity, transform);
            }
        }
    }
}
