using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [System.Serializable]
    public struct Ingredient
    {
        public ItemData item; // the required item
        public int amount; // how many of this item is needed
    }

    public Ingredient[] ingredients; // list of required items
    public ItemData result; // the crafted item

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
