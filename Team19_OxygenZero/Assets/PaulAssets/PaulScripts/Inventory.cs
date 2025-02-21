using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    // Amount of slots you want to assign to the inventory
    private static int slotAmount = 10;

    // array of item slots
    public GameObject[] itemSlots = new GameObject[slotAmount];

    // Acts to set the item amount for each item
    private int[] itemAmount = new int[slotAmount];

    // Acts to set the item amount for each item
    private float[] itemCost = new float[slotAmount];

    // Acts to set the item amount for each item
    private float[] itemWeight = new float[slotAmount];

    // Acts to set the button for each item
    [SerializeField] private Button[] itemButton = new Button[slotAmount];

    // Acts to set the highlight for each item
    public Image[] Highlight = new Image[slotAmount];

    // Acts to set the slot selection when clicked for each item
    [SerializeField] public bool[] SlotSelected = new bool[slotAmount];

    private GameObject[] itemVariables = new GameObject[4];

    // An item slot to instantiate into the inventory everytime a new item is added into inventory
    [SerializeField] private GameObject itemPrefab;

    // Acts as a bag to store all inventory items
    [SerializeField] private GameObject InventoryBag;
    [SerializeField] public GameObject InventoryDisplay;

    public List<GameObject> droppedPrefabs;  // Assign prefabs in the Inspector
    public Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();


    // Track button initialization
    private bool[] buttonInitialized = new bool[slotAmount];

    public bool[] itemEquipped = new bool[slotAmount];

    public Transform itemHolderPosition;
    [SerializeField] private GameObject itemHolder;
    public Transform dropArea;

    void Start()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i] = null;
            itemAmount[i] = 0;
            itemCost[i] = 0;
            itemWeight[i] = 0;
            buttonInitialized[i] = false;
            itemEquipped[i] = false;
        }

        for (int i = 0; i < itemVariables.Length; i++)
        {
            itemVariables[i] = null;
        }

        InventoryDisplay.SetActive(false);

        foreach (var prefab in droppedPrefabs)
        {
            prefabDictionary[prefab.tag] = prefab; // Store prefabs by tag
        }
    }

    private void Update()
    {
        if (InventoryDisplay.activeSelf)
        {
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (itemSlots[i] != null && !buttonInitialized[i])
                {
                    InitializeButtonForSlot(i);
                }
            }
        }

        // To uncheck slotselected if slot does not exist
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if(itemSlots[i] == null)
            {
                SlotSelected[i] = false;
            }
        }
    }

    private void InitializeButtonForSlot(int slotIndex)
    {
        if (itemSlots[slotIndex] == null) return;

        Button[] itemButtons = itemSlots[slotIndex].GetComponentsInChildren<Button>(true);

        foreach (Button button in itemButtons)
        {
            if (button.gameObject.CompareTag("itemButton"))
            {

                if (button != null)
                {
                    itemButton[slotIndex] = button;

                    // Remove any existing listeners to prevent duplicates
                    button.onClick.RemoveAllListeners();

                    // Add the new listener with the correct index
                    int index = slotIndex; // Create a local copy for the closure
                    button.onClick.AddListener(() => ButtonSelect(itemButton[index]));

                    Debug.Log($"Button initialized for slot {slotIndex}: {button.name}");
                }
                else
                {
                    Debug.LogError("Button not found inside item prefab.");
                }

                // Setup highlight
                if (button.transform.childCount > 0)
                {
                    Highlight[slotIndex] = button.transform.GetChild(0).GetComponent<Image>();
                    if (Highlight[slotIndex] != null)
                    {
                        Debug.Log($"Highlight Found: {Highlight[slotIndex].name}");
                        Highlight[slotIndex].gameObject.SetActive(false);
                    }
                }
            }
        }

        buttonInitialized[slotIndex] = true;
    }

    public void AddItem(string name, float cost, float weight)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null)
            {
                buttonInitialized[i] = false; // Reset the initialization flag

                // instantiated an item prefab in an empty item slot in inventory
                itemSlots[i] = Instantiate(itemPrefab);
                // Set item into the bag by making it a child of the bag
                itemSlots[i].transform.SetParent(InventoryBag.transform, false);
                // Set the name and tag of the item
                itemSlots[i].tag = name;
                itemSlots[i].name = name;

                // Set the item cost
                itemCost[i] = cost;
                // Set the item weight
                itemWeight[i] = weight;

                // Increase the item amount
                itemAmount[i]++;

                // To get the TMP texts that are parented to the item prefab
                TMP_Text[] itemTexts = itemSlots[i].GetComponentsInChildren<TMP_Text>(true);

                foreach (TMP_Text text in itemTexts)
                {
                    // Set the name of the item visually
                    if (text.gameObject.CompareTag("itemName"))
                    {
                        text.text = name;
                    }
                    // Set the item amount visually
                    else if (text.gameObject.CompareTag("itemAmount"))
                    {
                        text.text = itemAmount[i].ToString();
                    }
                    // Set the cost of the item visually
                    else if (text.gameObject.CompareTag("itemCost"))
                    {
                        text.text = "$" + itemCost[i].ToString("F2");
                    }
                    // Set the weight of the item visually
                    else if (text.gameObject.CompareTag("itemWeight"))
                    {
                        text.text = itemWeight[i].ToString("F2") + "kg";
                    }
                }
                break;
            }
            else if (itemSlots[i].tag == name)
            {
                // Increase the item amount
                itemAmount[i]++;

                // set total cost
                float totalCost = itemCost[i] * itemAmount[i];

                // set total weight
                float totalWeight = itemWeight[i] * itemAmount[i];

                // To get the TMP texts that are parented to the item prefab
                TMP_Text[] itemTexts = itemSlots[i].GetComponentsInChildren<TMP_Text>(true);

                foreach (TMP_Text text in itemTexts)
                {
                    // Set the item amount visually
                    if (text.gameObject.CompareTag("itemAmount"))
                    {
                        text.text = itemAmount[i].ToString();
                    }
                    // Set the cost of the item visually
                    else if (text.gameObject.CompareTag("itemCost"))
                    {
                        text.text = "$" + totalCost.ToString("F2");
                    }
                    // Set the weight of the item visually
                    else if (text.gameObject.CompareTag("itemWeight"))
                    {
                        text.text = totalWeight.ToString("F2") + "kg";
                    }
                }
                break;
            }
        }
    }

    public void RemoveItem(string name)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].tag == name)
            {
                if (itemAmount[i] > 0)
                {
                    // Decrease the item amount
                    itemAmount[i]--;

                    float totalCost = itemCost[i] * itemAmount[i];
                    float totalWeight = itemWeight[i] * itemAmount[i];

                    // To get the TMP texts that are parented to the item prefab
                    TMP_Text[] itemTexts = itemSlots[i].GetComponentsInChildren<TMP_Text>(true);

                    foreach (TMP_Text text in itemTexts)
                    {
                        // Set the item amount visually
                        if (text.gameObject.CompareTag("itemAmount"))
                        {
                            text.text = itemAmount[i].ToString();
                        }
                        // Set the cost of the item visually
                        else if (text.gameObject.CompareTag("itemCost"))
                        {
                            text.text = "$" + totalCost.ToString("F2");
                        }
                        // Set the weight of the item visually
                        else if (text.gameObject.CompareTag("itemWeight"))
                        {
                            text.text = totalWeight.ToString("F2") + "kg";
                        }
                    }
                }

                if (itemAmount[i] == 0)
                {
                    buttonInitialized[i] = false; // Reset the initialization flag

                    itemSlots[i] = null;
                    itemAmount[i] = 0;
                    itemCost[i] = 0;
                    itemWeight[i] = 0;
                    Highlight[i] = null;
                    SlotSelected[i] = false;
                    itemButton[i] = null;
                    itemEquipped[i] = false;

                    if (InventoryBag.transform.childCount > 0)
                    {
                        Destroy(InventoryBag.transform.GetChild(i).gameObject);
                        Debug.Log("Item Destroyed");
                    }

                    // Shift the slots
                    for (int j = i; j < itemSlots.Length - 1; j++)
                    {                                              
                        itemSlots[j] = itemSlots[j + 1];
                        itemAmount[j] = itemAmount[j + 1];
                        itemCost[j] = itemCost[j + 1];
                        itemWeight[j] = itemWeight[j + 1];
                        buttonInitialized[j] = buttonInitialized[j + 1];
                        Highlight[j] = Highlight[j + 1];
                        SlotSelected[j] = SlotSelected[j + 1];
                        itemButton[j] = itemButton[j + 1];
                        buttonInitialized[j] = false;
                        itemEquipped[j] = itemEquipped[j + 1];
                    }

                    // Clear the last slot 
                    itemSlots[itemSlots.Length - 1] = null;
                    itemAmount[itemSlots.Length - 1] = 0;
                    itemCost[itemSlots.Length - 1] = 0;
                    itemWeight[itemSlots.Length - 1] = 0;
                    buttonInitialized[itemSlots.Length - 1] = false;
                    Highlight[itemSlots.Length - 1] = null;
                    SlotSelected[itemSlots.Length - 1] = false;
                    itemButton[itemSlots.Length - 1] = null;
                    itemEquipped[itemSlots.Length - 1] = false;
                }
                break;
            }
        }
    }

    // Function for handling all button click events in the inventory except for the open and close inventory buttons
    public void ButtonSelect(Button button)
    {
        Debug.Log("Button Clicked: " + button.name);

        // Deactivate all existing highlights first
        for (int i = 0; i < Highlight.Length; i++)
        {
            if (Highlight[i] != null)
            {
                Highlight[i].gameObject.SetActive(false);
                SlotSelected[i] = false;
            }
        }

        // Afterwards, set highlight for most recently pressed button
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (button == itemButton[i])
            {
                if (Highlight[i] != null)
                {
                    Highlight[i].gameObject.SetActive(true);
                    SlotSelected[i] = true;
                }
            }
        }
    }

    public void SpawnObject()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null)
            {

            }
        }
    }

    public void SpawnByTag(string tag, Vector3 position, Transform parent = null)
    {
        if (prefabDictionary.TryGetValue(tag, out GameObject prefab))
        {
            GameObject spawnedItem = Instantiate(prefab, position, Quaternion.identity);
            if (parent != null)
            {
                spawnedItem.transform.SetParent(parent, false); // Set parent without changing local scale/position
            }
        }
        else
        {
            Debug.LogError("No prefab found with tag: " + tag);
        }
    }


    public void EquipItem()
    {
        for(int i=0;i <itemSlots.Length;i++)
        {
            if (itemSlots[i] != null && SlotSelected[i] == true)
            {
                // Check if an item is already equipped and unequip it first
                for (int j = 0; j < itemSlots.Length; j++)
                {
                    if (itemEquipped[j] == true && j != i)
                    {
                        // Destroy the currently equipped item
                        foreach (Transform child in itemHolder.transform)
                        {
                            Destroy(child.gameObject);
                        }
                        itemEquipped[j] = false;
                    }
                }

                // Toggle equipment status for the selected item
                itemEquipped[i] = !itemEquipped[i];

                if (itemEquipped[i] == true)
                {
                    // Spawn item and set its parent to itemHolder
                    SpawnByTag(itemSlots[i].tag, itemHolder.transform.position, itemHolder.transform);

                    foreach (Transform child in itemHolder.transform)
                    {
                        child.localPosition = Vector3.zero;
                        child.localRotation = Quaternion.identity;

                        foreach (Transform child2 in child)
                        {
                            if (child2.CompareTag("pickupPrompt"))
                            {
                                child2.gameObject.SetActive(false);
                            }
                        }
                    }
                }
                else
                {
                    // Unequip the item
                    foreach (Transform child in itemHolder.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
            
            }
        }
    }
}