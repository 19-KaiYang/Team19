using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEditor.Progress;

public class CraftingScript : MonoBehaviour
{
    public List<CraftingRecipe> recipes; // list of all available recipes

    public TextMeshProUGUI selectedItemText; // UI text to display selected item name

    private CraftingRecipe selectedRecipe; // the currently selected recipe

    private GameObject FindInventory;
    [SerializeField]private Inventory inventory;

    // the place in where the text should spawn in
    [SerializeField] private Transform ingredientContents;

    [SerializeField] private GameObject textPrefab;

    bool RequirementMet = false;

    bool RecipeSelected = false;


    void Start()
    {
        FindInventory = GameObject.FindWithTag("Inventory");
        inventory= FindInventory.GetComponent<Inventory>();
    }

    
   

    public void SelectItem(string itemName)
    {
        RecipeSelected = true;
        // find the recipe for the selected item
        selectedRecipe = recipes.Find( r => r.result.itemName == itemName);

        bool[] obtainedIngredients = new bool[selectedRecipe.ingredients.Length];

        // Add this debug line
        Debug.Log("Looking for recipe: " + itemName + ", Found: " + (selectedRecipe != null));

        foreach (Transform child in ingredientContents)
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

                    // Search inventory for this ingredient
                    int foundAmount = 0;
                    for (int j = 0; j < inventory.itemSlots.Length; j++)
                    {
                        Debug.Log($"Checking inventory slot {j}");
                        if (inventory.itemSlots[j] != null)
                        {
                            Debug.Log($"Comparing {inventory.itemSlots[j].name} with {selectedRecipe.ingredients[i].item.itemName}");
                            if (inventory.itemSlots[j].name == selectedRecipe.ingredients[i].item.itemName)
                            {
                                
                                foundAmount = inventory.itemAmount[j];
                                if (foundAmount >= selectedRecipe.ingredients[i].item.amount)
                                {
                                    obtainedIngredients[i] = true;
                                }
                                    Debug.Log($"Found {foundAmount} of {selectedRecipe.ingredients[i].item.itemName}");
                                break;  // Exit the j loop once we find the ingredient
                            }
                        }
                    }        
                        // Display the ingredients you have and in the ingredients needed
                        ingredientText.text = $"{selectedRecipe.ingredients[i].item.itemName} x{foundAmount}/{selectedRecipe.ingredients[i].amount}";                  
                }
                
            }

            for (int i = 0; i < obtainedIngredients.Length; i++)
            {
                RequirementMet = true;
                if (obtainedIngredients[i] == false)
                {
                    RequirementMet = false;
                    break;
                }
            }


            
        }
        else
        {
            Debug.LogError("No recipe found for: " + itemName);
        }
    }

    private void Update()
    {
        if (RecipeSelected == true)
        {
            bool[] obtainedIngredients = new bool[selectedRecipe.ingredients.Length];


            if (selectedRecipe != null)
            {
                // update the selected item name
                selectedItemText.text = "<b>" + selectedRecipe.result.itemName + "</b>";





                for (int i = 0; i < selectedRecipe.ingredients.Length; i++)
                {
                    // set ingredient texts
                    if (selectedRecipe.ingredients.Length > 0)
                    {
                        GameObject Text = ingredientContents.GetChild(i).gameObject;
                        TMP_Text ingredientText = Text.GetComponent<TMP_Text>();

                        // Search inventory for this ingredient
                        int foundAmount = 0;
                        for (int j = 0; j < inventory.itemSlots.Length; j++)
                        {
                            Debug.Log($"Checking inventory slot {j}");
                            if (inventory.itemSlots[j] != null)
                            {
                                Debug.Log($"Comparing {inventory.itemSlots[j].name} with {selectedRecipe.ingredients[i].item.itemName}");
                                if (inventory.itemSlots[j].name == selectedRecipe.ingredients[i].item.itemName)
                                {

                                    foundAmount = inventory.itemAmount[j];
                                    if (foundAmount >= selectedRecipe.ingredients[i].item.amount)
                                    {
                                        obtainedIngredients[i] = true;
                                    }
                                    else
                                    {
                                        obtainedIngredients[i] = false;
                                    }
                                    Debug.Log($"Found {foundAmount} of {selectedRecipe.ingredients[i].item.itemName}");
                                    break;  // Exit the j loop once we find the ingredient
                                }
                            }
                        }
                        // Display the ingredients you have and in the ingredients needed
                        ingredientText.text = $"{selectedRecipe.ingredients[i].item.itemName} x{foundAmount}/{selectedRecipe.ingredients[i].amount}";
                    }

                }

                for (int i = 0; i < obtainedIngredients.Length; i++)
                {
                    RequirementMet = true;
                    if (obtainedIngredients[i] == false)
                    {
                        RequirementMet = false;
                        break;
                    }
                }
            }



        }
    }

    public void CraftItem()
    {
        if (selectedRecipe == null)
        {
            Debug.LogError("No recipe selected");
            return;
        }
     
        if (RequirementMet == false)
        {
             Debug.LogWarning("Not enough material to craft " + selectedRecipe.result.itemName);
             return;
        }

        if (RequirementMet == true)
        {
            // remove the required items from inventory
            for (int i = 0; i < selectedRecipe.ingredients.Length; i++)
            {
                inventory.RemoveItem(selectedRecipe.ingredients[i].item.itemName, selectedRecipe.ingredients[i].amount);
            }
            // add the crafted item to the inventory
            inventory.AddItem(selectedRecipe.result.itemName, "Item", selectedRecipe.result.cost, selectedRecipe.result.weight, selectedRecipe.result.usable);
            Debug.Log("Crafted: " + selectedRecipe.result.itemName);
        }
        // refresh UI after crafting
        SelectItem(selectedRecipe.result.itemName);
    }

    public void ChooseRecipe(string Recipe)
    {
        SelectItem(Recipe);
    }

   
}
