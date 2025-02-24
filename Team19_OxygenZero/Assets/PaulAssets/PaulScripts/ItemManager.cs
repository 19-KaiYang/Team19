using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public List<GameObject> GameItemPrefabs;  // Assign prefabs in the Inspector
    public Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    public static ItemManager Instance { get; private set; }
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            /*DontDestroyOnLoad(gameObject);*/ // Optional: keeps it across scenes
        }
        else
        {
            Destroy(gameObject);
        }
        foreach (var prefab in GameItemPrefabs)
        {
            prefabDictionary[prefab.name] = prefab; // Store prefabs by tag
        }
    }

    public void SpawnByTag(string name, Vector3 position, Transform parent = null)
    {
        if (prefabDictionary.TryGetValue(name, out GameObject prefab))
        {
            GameObject spawnedItem = Instantiate(prefab, position, Quaternion.identity);
            if (parent != null)
            {
                spawnedItem.transform.SetParent(parent, false); // Set parent without changing local scale/position
            }
        }
        else
        {
            Debug.LogError("No prefab found with name: " + tag);
        }
    }
}
