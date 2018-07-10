using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject {

    public string itemName;
    public int sellPrice;
    public int buyPrice;
    public string description;
    public bool isStackable;
    public int maxStackSize;
    public int currentCount; // Amount in the stack
    public bool hasDurability;
    public int durability;
    public bool isUnique; // Only one copy of this item should exist in a game instance
    public bool destroyOnUse;
    public int hungerModifier;
    public int thirstModifier;
    public int warmthModifier;
    public int healthModifier;
    public int weaponDamage; // Amount of damage this weapon deals
    public float attackSpeed; // Speed this weapon attacks at
    public int damageVariation; // Amount of damage variation
    public int damageAbsorbtion; // Amount of damage this item mitigates
    public bool equippable; // Can this item be equipped
    public bool sellable; // Can this item be sold
    public ItemTypes itemType;
    public Sprite itemSprite;

    public enum ItemTypes
    {
        Weapon,
        Consumable,
        Misc,
        Headgear,
        Chestgear,
        Pants,
        Gloves,
        Shoes,
        Earrings,
        Ring,
        Necklace,
        Quest,
        Material,
        Structure
    }

    public void Use()
    {
        switch (itemType)
        {
            case (ItemTypes.Weapon):
                return;

            case (ItemTypes.Consumable):

                if (destroyOnUse)
                {
                    currentCount -= 1;
                }

                PlayerController.Instance.health.currentValue += healthModifier;
                PlayerController.Instance.hunger.currentValue += hungerModifier;
                PlayerController.Instance.thirst.currentValue += thirstModifier;
                return;

            case (ItemTypes.Misc):
                return;

            case (ItemTypes.Headgear):
                return;

            case (ItemTypes.Chestgear):
                return;

            case (ItemTypes.Gloves):
                return;

            case (ItemTypes.Shoes):
                return;

            case (ItemTypes.Earrings):
                return;

            case (ItemTypes.Ring):
                return;

            case (ItemTypes.Necklace):
                return;

            case (ItemTypes.Quest):
                return;

            case (ItemTypes.Material):
                return;
        }
    }
}
