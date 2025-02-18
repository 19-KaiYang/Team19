
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Amount of slots you want to assign to the inventory
    private static int slotAmount = 10;
    // array of item slots
    private GameObject[] itemSlots = new GameObject[slotAmount];

    // Acts to set the item amount for each item
    private int[] itemAmount = new int[slotAmount];

    // Acts to set the item amount for each item
    private float[] itemCost = new float[slotAmount];

    // Acts to set the item amount for each item
    private float[] itemWeight = new float[slotAmount];

    private GameObject[] itemVariables = new GameObject[4];

    // An item slot to instantiate into the inventory everytime a new item is added into inventory
    [SerializeField] private GameObject itemPrefab;
    

    // Acts as a bag to store all inventory items
    [SerializeField] private GameObject InventoryBag;
    [SerializeField] private GameObject InventoryDisplay;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i] = null;
            itemAmount[i] = 0;
            itemCost[i] = 0;
            itemWeight[i] = 0;
        }

        for (int i = 0; i < itemVariables.Length; i++)
        {
            itemVariables[i] = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            AddItem("Cheeseburger", 2.5f, 10);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddItem("Hotdog", 1, 8);
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            UseItem("Cheeseburger");           
        }

        if(Input.GetKeyDown (KeyCode.X))
        {
            UseItem("Hotdog");
        }


        // Inventory Open and close Logic
        if(Input.GetKeyDown(KeyCode.E))
        {
            InventoryDisplay.SetActive(!InventoryDisplay.activeSelf);
        }

    }

    public void AddItem(string name, float cost, float weight)
    {
   
        for(int i = 0;i < itemSlots.Length;i++)
        {
            if (itemSlots[i] == null)
            {
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
                    else if(text.gameObject.CompareTag("itemAmount"))
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


    public void UseItem(string name)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].tag == name)
            {
                if (itemAmount[i] > 0)
                {
                    // Increase the item amount
                    itemAmount[i]--;

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
                   
                }
                if (itemAmount[i] <= 0)
                {
                    itemSlots[i] = null;
                    itemAmount[i] = 0;
                    itemCost[i] = 0;
                    itemWeight[i] = 0;

                    if (InventoryBag.transform.childCount > 0)
                    {
                        Destroy(InventoryBag.transform.GetChild(i).gameObject);
                        Debug.Log("Item Destroyed");
                    }

                    // Shift the slots
                    for (int j = i; j < itemSlots.Length - 1; j++)
                    {
                        if (itemSlots[j + 1] != null)
                        {
                            itemSlots[j] = itemSlots[j + 1];
                            itemAmount[j] = itemAmount[j + 1];
                            itemCost[j] = itemCost[j + 1];
                            itemWeight[j] = itemWeight[j + 1];

                            if (itemSlots[j] != null)
                            {
                                // The GameObject is also shifted in the hierarchy
                                if (InventoryBag.transform.childCount > j + 1)
                                {
                                    Transform child = InventoryBag.transform.GetChild(j + 1);
                                    child.SetSiblingIndex(j);
                                }
                            }
                        }

                    }
                    // clear the last slot 
                    itemSlots[itemSlots.Length - 1] = null;
                    itemAmount[itemSlots.Length - 1] = 0;
                    itemCost[itemSlots.Length - 1] = 0;
                    itemWeight[itemSlots.Length - 1] = 0;

                    
                }
                break;
            }
            else if (itemSlots[i] != null && itemSlots[i].tag != name)
            {
               // Do nothing
            }
            
        }     
    }





}
