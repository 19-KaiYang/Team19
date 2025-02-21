using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CraftingScript : MonoBehaviour
{
    public List<CraftingRecipe> recipes; // list of all available recipes

    public TextMeshProUGUI selectedItemText; // UI text to display selected item name
    public TextMeshProUGUI ingredient1Text; // first ingredient
    public TextMeshProUGUI ingredient2Text; // second ingredient

    private CraftingRecipe selectedRecipe; // the currently selected recipe

    public void SelectItem(string itemName)
    {
        // find the recipe for the selected item
        selectedRecipe = recipes.Find( r => r.result.itemName == itemName);

        if (selectedRecipe != null)
        {
            // update the selected item name
            selectedItemText.text = "<b>" + selectedRecipe.result.itemName + "</b>";

            // set ingredient texts
            if (selectedRecipe.ingredients.Length > 0)
            {
                ingredient1Text.text = selectedRecipe.ingredients[0].item.itemName + " x" + selectedRecipe.ingredients[0].amount;
            }
            else
            {
                ingredient1Text.text = "";
            }
            
            if (selectedRecipe.ingredients.Length > 1)
            {
                ingredient2Text.text = selectedRecipe.ingredients[1].item.itemName + " x" + selectedRecipe.ingredients[1].amount;
            }
            else
            {
                ingredient2Text.text = "";
            }
            
        }
        else
        {
            Debug.LogError("No recipe found for: " + itemName);
        }
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
