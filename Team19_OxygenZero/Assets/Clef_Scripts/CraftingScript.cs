using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEditor.Progress;

public class CraftingScript : MonoBehaviour
{
    public List<CraftingRecipe> recipes; // list of all available recipes

    public TextMeshProUGUI selectedItemText; // UI text to display selected item name

    private CraftingRecipe selectedRecipe; // the currently selected recipe

    [SerializeField]private Inventory inventory;

    // the place in where the text should spawn in
    [SerializeField] private Transform ingredientContents;

    [SerializeField] private GameObject textPrefab;

    private int[] requiredIngredients;

    
   

    public void SelectItem(string itemName)
    {
        // find the recipe for the selected item
        selectedRecipe = recipes.Find( r => r.result.itemName == itemName);

        bool[] obtainedIngredients = new bool[selectedRecipe.ingredients.Length];

        foreach(Transform child in ingredientContents)
        {
            Destroy(child.gameObject);
        }

        if (selectedRecipe != null)
        {
            // update the selected item name
            selectedItemText.text = "<b>" + selectedRecipe.result.itemName + "</b>";


            for (int i = 0; i < selectedRecipe.ingredients.Length; i++)
            {
                // set ingredient texts
                if (selectedRecipe.ingredients.Length > 0)
                {
                    Debug.Log("Instantiate Text Prefab");
                    GameObject TextPrefab = Instantiate(textPrefab);
                    TextPrefab.transform.SetParent(ingredientContents);
                    TextPrefab.transform.localPosition = Vector3.zero;
                    TextPrefab.transform.localRotation = Quaternion.identity;
                    TextPrefab.transform.localScale = Vector3.one;

                    TMP_Text ingredientText = TextPrefab.GetComponent<TMP_Text>();

                    for (int j = 0; j < inventory.itemSlots.Length; j++)
                    {
                        // check if the item slot is not empty
                        if (inventory.itemSlots[j] != null)
                        {
                            ObjectData objectData = inventory.itemSlots[j].GetComponent<ObjectData>();

                            // Check if the item has an objectdata
                            if (objectData != null)
                            {
                                // Check if there is an item in inventory that is the required ingredient
                                if (objectData.item.itemName == selectedRecipe.ingredients[i].item.itemName)
                                {
                                    ingredientText.text = selectedRecipe.ingredients[i].item.itemName + " x" + inventory.itemAmount[j] + "/" + selectedRecipe.ingredients[i].amount;
                                    Debug.Log("Ingredient text written");
                                    obtainedIngredients[i] = true;
                                }                                                                                                                                                
                            }                          
                        }                      
                    }
                    if (obtainedIngredients[i] == false)
                    {
                        ingredientText.text = selectedRecipe.ingredients[i].item.itemName + " x" + selectedRecipe.ingredients[i].amount;
                    }


                }
                else
                {

                }
            }


            
        }
        else
        {
            Debug.LogError("No recipe found for: " + itemName);
        }
    }

    public void CraftItem()
    {
        if (selectedRecipe == null)
        {
            Debug.LogError("No reicpe selected");
            return;
        }

        // check if player has all required ingredients
        for (int i = 0; i < selectedRecipe.ingredients.Length; i++)
        {
            if (!inventory.HasItem(selectedRecipe.ingredients[i].item.itemName, selectedRecipe.ingredients[i].amount))
            {
                Debug.LogWarning("Not enough material to craft " + selectedRecipe.result.itemName);
                return;
            }
        }

        // remove the required items from inventory
        for (int i = 0; i < selectedRecipe.ingredients.Length; i++)
        {
            inventory.RemoveItem(selectedRecipe.ingredients[i].item.itemName, selectedRecipe.ingredients[i].amount);
        }

        // add the crafted item to the inventory
        inventory.AddItem(selectedRecipe.result.itemName, selectedRecipe.result.price, selectedRecipe.result.weight);
        Debug.Log("Crafted: " + selectedRecipe.result.itemName);

        // refresh UI after crafting
        SelectItem(selectedRecipe.result.itemName);
    }

    public void onClickOxygenTank()
    {
        SelectItem("Oxygen Tank");
    }

    public void OnClickBatteryPack()
    {
        SelectItem("Battery Pack");
    }

    // Start is called beforre the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
