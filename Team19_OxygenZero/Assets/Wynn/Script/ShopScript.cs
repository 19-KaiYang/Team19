using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopScript : MonoBehaviour
{

    private GameObject FindInventory;
    [SerializeField] private Inventory inventory;

    private GameObject FindItemManager;
    [SerializeField] private ItemManager itemManager;

    [SerializeField] private GameObject dropper;

    [SerializeField] private TMP_Text shopItemNameTMP, shopItemDescTMP, shopItemCostTMP;


    // Start is called before the first frame update
    void Start()
    {
        FindInventory = GameObject.FindWithTag("Inventory");
        inventory = FindInventory.GetComponent<Inventory>();

        FindItemManager = GameObject.FindWithTag("GameItemManager");
        itemManager = FindItemManager.GetComponent<ItemManager>();
    }

    public void CheckItemName(string name)
    {
        for (int i = 0; i < itemManager.GameItemPrefabs.Count; i++)
        {
            GameObject prefab = itemManager.GameItemPrefabs[i]; // Get the prefab

            if (prefab != null) // Null check
            {
                ObjectData objectData = prefab.GetComponent<ObjectData>();

                if (objectData != null && objectData.item.itemName == name)
                {
                    shopItemNameTMP.text = objectData.item.itemName;
                    shopItemDescTMP.text = objectData.item.description;
                    shopItemCostTMP.text = "$" + objectData.item.cost.ToString();

                    Debug.Log($"Found ObjectData in {prefab.name}");
                }
            }
            else
            {
                Debug.LogWarning("Null prefab found in GameItemPrefabs list");
            }
        }
    }

    public void BuyButton()
    {

        for (int i = 0; i < itemManager.GameItemPrefabs.Count; i++)
        {
            GameObject prefab = itemManager.GameItemPrefabs[i]; // Get the prefab

            if (prefab != null)
            {
                ObjectData objectData = prefab.GetComponent<ObjectData>();

                if (objectData != null)
                {
                    if (shopItemNameTMP.text == objectData.item.itemName)
                    {
                        inventory.UpdateMoney(objectData.item.cost, false);
                        itemManager.SpawnByItemName(objectData.item.itemName, dropper.transform.position);
                    }
                }
            }
        }
    }
}
