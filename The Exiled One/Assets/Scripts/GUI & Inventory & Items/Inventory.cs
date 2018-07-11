using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {

    #region Singleton
    // Singleton pattern
    private static Inventory inventoryControllerInstance;

    public static Inventory Instance { get { return inventoryControllerInstance; } }

    private void Awake()
    {
        if (inventoryControllerInstance == null)
        {
            inventoryControllerInstance = this;
        }
    }
    #endregion

    public int quickSlotSize = 10; // Quick slots + inventory space
    public int currentInventorySize = 24;

    // References
    private ItemDescriptionController itemDescriptionContainerScript;
    public GameObject itemDescriptionContainer;
    public GameObject quickSlotContainer;
    public GameObject inventoryContainer;
    public GameObject inventorySlotContainer;
    public GameObject equipmentSlotContainer;

    [HideInInspector]
    // Inventory, first ten slots are quick slots
    public InventorySlot[] inventorySlots;

    [HideInInspector]
    public Toggle[] quickSlotToggles;

    // Selections & Variable checks
    [HideInInspector]
    public bool isDragging = false; // Is player currently dragging an object
    [HideInInspector]
    public int currentlyOverSlotIndex = -1; // Index of the slot mouse is currently over (-1 = Inventory panel, -2 = Outside (drop item))
    [HideInInspector]
    public int draggedFromSlotIndex = -1; // Index of slot dragging started from
    [HideInInspector]
    public int currentlySelectedIndex = 0; // Index of quick slot selected
    [HideInInspector]
    public Item currentlySelectedItem;
    public int numberOfFilledSlots; // Index that the inventroy array is filled to

    public bool hasInventory = false; // Has player crafted an inventory

    // Start function
    #region Generate slots and set references
    private void Start()
    {
        // Set references
        itemDescriptionContainerScript = itemDescriptionContainer.GetComponent<ItemDescriptionController>();

        // Add event listeners
        EventManager.Instance.e_saveGame.AddListener(Save);
        EventManager.Instance.e_loadGame.AddListener(Load);

        // Generate arrays
        inventorySlots = new InventorySlot[1000];
        quickSlotToggles = new Toggle[quickSlotSize];

        // Generate inventory and quick slots UI  

        // Generate quick slots
        Vector3[] quickSlotPositions = new[] { new Vector3(-605, -605, 0), new Vector3(-500, -605, 0), new Vector3(-395, -605, 0), new Vector3(-290, -605, 0), new Vector3(-185, -605, 0), new Vector3(185, -605, 0), new Vector3(290, -605, 0), new Vector3(395, -605, 0), new Vector3(500, -605, 0), new Vector3(605, -605, 0) };
        var toggleGroup = quickSlotContainer.GetComponent<ToggleGroup>();

        for (int i = 0; i < quickSlotSize; i++)
        {
            var newQuickSlot = Instantiate(PrefabManager.Instance.prefabDatabase["Quick Slot"], quickSlotContainer.transform);
            newQuickSlot.transform.localPosition = quickSlotPositions[i];
            quickSlotToggles[i] = newQuickSlot.GetComponent<Toggle>();
            quickSlotToggles[i].group = toggleGroup; // Set toggle group
            newQuickSlot.name = "Quick Slot " + i;
            inventorySlots[i] = newQuickSlot.GetComponent<InventorySlot>();
            inventorySlots[i].slotNumber = i;
            

            if (i == 0) // First toggle quick slot, we turn it on
            {
                quickSlotToggles[i].isOn = true;
                currentlySelectedIndex = 0;
                currentlySelectedItem = inventorySlots[0].item;
            }
        }

        // Generate inventory slots
        for (int i = quickSlotSize; i < currentInventorySize + quickSlotSize; i++)
        {
            var newInventorySlot = Instantiate(PrefabManager.Instance.prefabDatabase["Inventory Slot"], inventorySlotContainer.transform);
            newInventorySlot.name = "Inventory Slot " + i;
            inventorySlots[i] = newInventorySlot.GetComponent<InventorySlot>();
            inventorySlots[i].slotNumber = i;
        }

        // Generate equipment slots
        int num = currentInventorySize + quickSlotSize;
        string[] equipmentSlotNames = new string[] {"Helmet", "Chestgear", "Pants", "Shoes", "Gloves"};

        for (int i = 0; i < 5; i++)
        {
            var newEquipmentSlot = Instantiate(PrefabManager.Instance.prefabDatabase["Equipment " + equipmentSlotNames[i] + " Slot"], equipmentSlotContainer.transform);
            newEquipmentSlot.name = "Equipment " + equipmentSlotNames[i] + " Slot";
            inventorySlots[num + i] = newEquipmentSlot.GetComponent<InventorySlot>();
            inventorySlots[num + i].slotNumber = num + i;
        }

        // Generate crafting slots

        // Set default variables
        currentlyOverSlotIndex = -1;
        draggedFromSlotIndex = -1;
        inventoryContainer.SetActive(false);
        equipmentSlotContainer.SetActive(false);
        numberOfFilledSlots = quickSlotSize + currentInventorySize + 5;

        // TESTING
        hasInventory = true;
        AddItem(Instantiate(ItemManager.Instance.itemDatabase["Stone Axe"]), "full inventory");
        for (int i = 0; i < 100; i++)
        {
            AddItem(Instantiate(ItemManager.Instance.itemDatabase["Wood"]), "full inventory");
        }

        AddItem(Instantiate(ItemManager.Instance.itemDatabase["Stone Axe"]), "full inventory");
        AddItem(Instantiate(ItemManager.Instance.itemDatabase["Stone Axe"]), "full inventory");
        AddItem(Instantiate(ItemManager.Instance.itemDatabase["Stone Axe"]), "full inventory");
        AddItem(Instantiate(ItemManager.Instance.itemDatabase["Stone Axe"]), "full inventory");

    }
    #endregion

    private void OnDestroy()
    {
        EventManager.Instance.e_saveGame.RemoveListener(Save);
        EventManager.Instance.e_loadGame.RemoveListener(Load);
    }

    private void Update()
    {
        GetInput();
        UpdateGUI();
    }

    // Get input for inventory and quickslot selection (Expand region)
    #region GetInput
    // Inputs relating to inventory
    private void GetInput()
    {
        // Open inventory
        if (Input.GetButtonDown("Inventory"))
        {
            if (hasInventory)
            {
                // If inventory open, we close it
                if (inventoryContainer.activeSelf)
                {
                    inventoryContainer.SetActive(false);
                    equipmentSlotContainer.SetActive(false);
                }
                // If inventory closed, we open it and pause the game
                else
                {
                    inventoryContainer.SetActive(true);
                    equipmentSlotContainer.SetActive(true);
                }
            }           
        }

        // On click and dragging item
        if (Input.GetButtonDown("Left Click"))
        {
            if (Input.GetButton("Shift")) // If shift is held down, we either move item to inventory or away
            {
                CheckForShiftClick();
            }
            else if (Input.GetButton("Control"))
            {
                CheckForDrag(true);
            }
            else
            {
                CheckForDrag(false);
            }
        }

        // If right click, we use item
        if (Input.GetButtonDown("Right Click"))
        {
            if (currentlyOverSlotIndex >= 0)
            {
                // If there's an item we try to use it
                if (inventorySlots[currentlyOverSlotIndex].item)
                {
                    inventorySlots[currentlyOverSlotIndex].item.Use();
                }
            } 
        }

        // Quick Slot Controls (Scroll wheel or number keys)
        if (Input.GetButtonDown("Number One"))
        {
            quickSlotToggles[0].isOn = true;
            currentlySelectedIndex = 0;
        }
        else if (Input.GetButtonDown("Number Two"))
        {
            quickSlotToggles[1].isOn = true;
            currentlySelectedIndex = 1;
        }
        else if (Input.GetButtonDown("Number Three"))
        {
            quickSlotToggles[2].isOn = true;
            currentlySelectedIndex = 2;
        }
        else if (Input.GetButtonDown("Number Four"))
        {
            quickSlotToggles[3].isOn = true;
            currentlySelectedIndex = 3;
        }
        else if (Input.GetButtonDown("Number Five"))
        {
            quickSlotToggles[4].isOn = true;
            currentlySelectedIndex = 4;
        }
        else if (Input.GetButtonDown("Number Six"))
        {
            quickSlotToggles[5].isOn = true;
            currentlySelectedIndex = 5;
        }
        else if (Input.GetButtonDown("Number Seven"))
        {
            quickSlotToggles[6].isOn = true;
            currentlySelectedIndex = 6;
        }
        else if (Input.GetButtonDown("Number Eight"))
        {
            quickSlotToggles[7].isOn = true;
            currentlySelectedIndex = 7;
        }
        else if (Input.GetButtonDown("Number Nine"))
        {
            quickSlotToggles[8].isOn = true;
            currentlySelectedIndex = 8;
        }
        else if (Input.GetButtonDown("Number Zero"))
        {
            quickSlotToggles[9].isOn = true;
            currentlySelectedIndex = 9;
        }

        // Scroll wheel for quick slots
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Scroll downwards on quick slot
        {
            if (currentlySelectedIndex >= 9)
            {
                quickSlotToggles[0].isOn = true;
                currentlySelectedIndex = 0;
            }
            else
            {
                quickSlotToggles[currentlySelectedIndex + 1].isOn = true;
                currentlySelectedIndex++;
            } 
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0f) // Scroll upwards on quick slot
        {
            if (currentlySelectedIndex <= 0)
            {
                quickSlotToggles[9].isOn = true;
                currentlySelectedIndex = 9;
            }
            else
            {
                quickSlotToggles[currentlySelectedIndex - 1].isOn = true;
                currentlySelectedIndex--;
            }
        }
    }
    #endregion

    // Update GUI
    private void UpdateGUI()
    {
        currentlySelectedItem = inventorySlots[currentlySelectedIndex].item;

        // If dragging an item, make dragging object follow mouse
        if (isDragging)
        {
            DraggedItemController.Instance.transform.position = Input.mousePosition;
        }
    }

    // Inventory methods

    public bool AddItem(Item itemToAdd, string addType)
    {
        int lowerBound = 0;
        int upperBound = 0;

        switch (addType)
        {
            case ("inventory only"): // Add to top inventory only (not quick slot)
                lowerBound = quickSlotSize;
                upperBound = quickSlotSize + currentInventorySize;
                break;
            case ("full inventory"): // Add to full inventory
                lowerBound = 0;
                upperBound = quickSlotSize + currentInventorySize;
                break;
            case ("quick slots only"): // Add to quick slots only
                lowerBound = 0;
                upperBound = quickSlotSize;
                break;
            case ("chest"): // Add to chest only
                // Do nothing till chests are added
                break;
            default:
                lowerBound = 0;
                upperBound = quickSlotSize + currentInventorySize;
                break;
        }

        if (itemToAdd.isStackable)
        {
            for (int i = lowerBound; i < upperBound; i++)
            {
                // If empty slot we skip it
                if (inventorySlots[i].item == null)
                {
                    break;
                }

                // If item is stackable then we try to stack it to the max
                if (inventorySlots[i].item.itemName == itemToAdd.itemName && itemToAdd.isStackable && inventorySlots[i].item.currentCount < itemToAdd.maxStackSize)
                {
                    // We just stack the item
                    if (itemToAdd.currentCount + inventorySlots[i].item.currentCount <= itemToAdd.maxStackSize)
                    {
                        inventorySlots[i].item.currentCount += itemToAdd.currentCount;
                        inventorySlots[i].DisplayItem();
                        return true;
                    }
                    // We fill up to the max stack size and iterate on
                    else if (itemToAdd.currentCount + inventorySlots[i].item.currentCount > itemToAdd.maxStackSize)
                    {
                        itemToAdd.currentCount -= (itemToAdd.maxStackSize - inventorySlots[i].item.currentCount);
                        inventorySlots[i].item.currentCount = itemToAdd.maxStackSize;
                        inventorySlots[i].DisplayItem();
                    }
                }
            }
        } 

        // If item is not stackable or no remaining stacks, we put the item in a new slot if possible
        for (int i = lowerBound; i < upperBound; i++)
        {
            if (inventorySlots[i].item == null)
            {
                inventorySlots[i].item = itemToAdd;
                inventorySlots[i].DisplayItem();
                return true;
            }
        }

        // Else inventory is full and we let the player know that.
        PlayerText.Instance.UIPrint(LocalizationManager.Instance.LocalizeText("Player Dialog Inventory Full"));
        return false;
    }

    public void SwapItem()
    {

        inventorySlots[draggedFromSlotIndex].SetItem(inventorySlots[currentlyOverSlotIndex].item);
        inventorySlots[currentlyOverSlotIndex].SetItem(DraggedItemController.Instance.itemBeingDragged);

        // Show item description of new item
        ShowItemDescription(inventorySlots[currentlyOverSlotIndex].item);
    }

    public void AddItemToSlot(int slotIndex, Item itemToAdd)
    {
        if (inventorySlots[slotIndex].item)
        {
            if (inventorySlots[slotIndex].item.isStackable)
            {
                // If item is stackable then we try to stack it to the max
                if (inventorySlots[slotIndex].item.itemName == itemToAdd.itemName && itemToAdd.isStackable && inventorySlots[slotIndex].item.currentCount < itemToAdd.maxStackSize)
                {
                    // We just stack the item
                    if (itemToAdd.currentCount + inventorySlots[slotIndex].item.currentCount <= itemToAdd.maxStackSize)
                    {
                        inventorySlots[slotIndex].item.currentCount += itemToAdd.currentCount;
                        inventorySlots[slotIndex].DisplayItem();
                    }
                    // We fill up to the max stack size and iterate on
                    else if (itemToAdd.currentCount + inventorySlots[slotIndex].item.currentCount > itemToAdd.maxStackSize)
                    {
                        itemToAdd.currentCount -= (itemToAdd.maxStackSize - inventorySlots[slotIndex].item.currentCount);
                        inventorySlots[slotIndex].item.currentCount = itemToAdd.maxStackSize;
                    }
                }
            }
        }
        else
        {
            inventorySlots[slotIndex].SetItem(itemToAdd);
        }      
    }

    public void DropItem(Item item)
    {
        // Instantiate loot container and assign its item
        var lootContainerScript = Instantiate(PrefabManager.Instance.prefabDatabase["Loot Container"]).GetComponent<LootContainerController>();
        lootContainerScript.transform.position = (PlayerController.Instance.transform.position + new Vector3(0, 0.4f, 0));
        lootContainerScript.SetItem(item);
        lootContainerScript.isDroppedByPlayer = true;
        lootContainerScript.rb.AddForce(new Vector2(Random.Range(-3f, 3f), Random.Range(6f, 6f)), ForceMode2D.Impulse);
    }

    public void ShowItemDescription(Item item)
    {
        itemDescriptionContainer.SetActive(true);
        itemDescriptionContainerScript.SetItem(item);
        itemDescriptionContainer.transform.position = new Vector3(inventorySlots[currentlyOverSlotIndex].transform.position.x, inventorySlots[currentlyOverSlotIndex].transform.position.y + Screen.height/9, inventorySlots[currentlyOverSlotIndex].transform.position.z);
    }

    public void HideItemDescription()
    {
        itemDescriptionContainer.SetActive(false);
    }

    void ResetDrag()
    {
        isDragging = false;
        draggedFromSlotIndex = -1;
        DraggedItemController.Instance.ResetItem();
    }

    void CheckForDrag(bool splitStack)
    {
        // Not dragging an item, we start dragging
        if (!isDragging)
        {
            if (currentlyOverSlotIndex >= 0 && inventorySlots[currentlyOverSlotIndex].item)
            {
                isDragging = true;
                draggedFromSlotIndex = currentlyOverSlotIndex;

                // We split the stack of items if possible to drag
                if (splitStack && inventorySlots[currentlyOverSlotIndex].item.isStackable && inventorySlots[currentlyOverSlotIndex].item.currentCount > 1)
                {
                    // Make a clone of existing object
                    var newObject = Instantiate(inventorySlots[currentlyOverSlotIndex].item);

                    // Split the stack in half, assign it to new object
                    int splitCount = Mathf.CeilToInt(inventorySlots[currentlyOverSlotIndex].item.currentCount / 2);
                    inventorySlots[currentlyOverSlotIndex].item.currentCount -= splitCount;
                    inventorySlots[currentlyOverSlotIndex].DisplayItem();
                    newObject.currentCount = splitCount;

                    DraggedItemController.Instance.SetItem(newObject);
                }
                // Else just drag the item straight
                else
                {
                    DraggedItemController.Instance.SetItem(inventorySlots[currentlyOverSlotIndex].item);

                    // Remove item from slot
                    inventorySlots[currentlyOverSlotIndex].SetItem(null);
                }

                HideItemDescription();
            }
        }
        // Already dragging an item, we either swap it or place it in the empty slot and stop dragging
        else
        {
            // Drop item
            if (currentlyOverSlotIndex <= -2)
            {
                // We drop one by one
                if (splitStack && DraggedItemController.Instance.itemBeingDragged.isStackable && DraggedItemController.Instance.itemBeingDragged.currentCount > 1)
                {
                    var itemToDrop = Instantiate(DraggedItemController.Instance.itemBeingDragged);
                    itemToDrop.currentCount = 1;
                    DropItem(itemToDrop);
                    DraggedItemController.Instance.itemBeingDragged.currentCount -= 1;
                    DraggedItemController.Instance.DisplayItem();
                }
                // Just drop the item
                else
                {
                    DropItem(DraggedItemController.Instance.itemBeingDragged);
                    ResetDrag();
                } 
            }

            // Return object to original position
            else if (currentlyOverSlotIndex == -1)
            {
                if (inventorySlots[draggedFromSlotIndex].item)
                {
                    // If same item, we try to stack back
                    if (inventorySlots[draggedFromSlotIndex].item.itemName == DraggedItemController.Instance.itemBeingDragged.itemName && inventorySlots[draggedFromSlotIndex].item.isStackable && inventorySlots[draggedFromSlotIndex].item.currentCount < inventorySlots[draggedFromSlotIndex].item.maxStackSize)
                    {
                        // We just stack the item and reset drag
                        if (DraggedItemController.Instance.itemBeingDragged.currentCount + inventorySlots[draggedFromSlotIndex].item.currentCount <= DraggedItemController.Instance.itemBeingDragged.maxStackSize)
                        {
                            inventorySlots[draggedFromSlotIndex].item.currentCount += DraggedItemController.Instance.itemBeingDragged.currentCount;
                            inventorySlots[draggedFromSlotIndex].DisplayItem();
                            ResetDrag();
                        }
                        // We fill up to the max stack size and keep dragging
                        else if (DraggedItemController.Instance.itemBeingDragged.currentCount + inventorySlots[draggedFromSlotIndex].item.currentCount > DraggedItemController.Instance.itemBeingDragged.maxStackSize)
                        {
                            DraggedItemController.Instance.itemBeingDragged.currentCount -= (DraggedItemController.Instance.itemBeingDragged.maxStackSize - inventorySlots[draggedFromSlotIndex].item.currentCount);
                            inventorySlots[draggedFromSlotIndex].item.currentCount = DraggedItemController.Instance.itemBeingDragged.maxStackSize;
                            inventorySlots[draggedFromSlotIndex].DisplayItem();
                            DraggedItemController.Instance.DisplayItem();
                        }
                    }
                    // Else if different item, we keep dragging
                }
                else
                {
                    AddItemToSlot(draggedFromSlotIndex, DraggedItemController.Instance.itemBeingDragged);
                    ResetDrag();
                }
            }

            // Swap item
            else if (currentlyOverSlotIndex >= 0 && inventorySlots[currentlyOverSlotIndex].item)
            {
                // If item is stackable then we try to stack it to the max
                if (inventorySlots[currentlyOverSlotIndex].item.itemName == DraggedItemController.Instance.itemBeingDragged.itemName && DraggedItemController.Instance.itemBeingDragged.isStackable)
                {
                    // If ctrl held down, we add one by one
                    if (splitStack)
                    {
                        // We can still add one more count of the item
                        if (inventorySlots[currentlyOverSlotIndex].item.currentCount < DraggedItemController.Instance.itemBeingDragged.maxStackSize)
                        {
                            // We add one by one
                            if (DraggedItemController.Instance.itemBeingDragged.currentCount > 1)
                            {
                                inventorySlots[currentlyOverSlotIndex].item.currentCount++;                         
                                DraggedItemController.Instance.itemBeingDragged.currentCount--;
                                inventorySlots[currentlyOverSlotIndex].DisplayItem();
                                DraggedItemController.Instance.DisplayItem();
                            }
                            // We just add the last count and reset drag container
                            else
                            {
                                inventorySlots[currentlyOverSlotIndex].item.currentCount += DraggedItemController.Instance.itemBeingDragged.currentCount;
                                inventorySlots[currentlyOverSlotIndex].DisplayItem();
                                ResetDrag();
                            }               
                        }
                        // Else the stack is full, we do nothing
                    }
                    // If ctrl isnt held we just add
                    else
                    {
                        // We just stack the item
                        if (DraggedItemController.Instance.itemBeingDragged.currentCount + inventorySlots[currentlyOverSlotIndex].item.currentCount <= DraggedItemController.Instance.itemBeingDragged.maxStackSize)
                        {
                            inventorySlots[currentlyOverSlotIndex].item.currentCount += DraggedItemController.Instance.itemBeingDragged.currentCount;
                            inventorySlots[currentlyOverSlotIndex].DisplayItem();
                            ResetDrag();
                        }
                        // We fill up to the max stack size and keep dragging the remainder
                        else if (DraggedItemController.Instance.itemBeingDragged.currentCount + inventorySlots[currentlyOverSlotIndex].item.currentCount > DraggedItemController.Instance.itemBeingDragged.maxStackSize)
                        {
                            DraggedItemController.Instance.itemBeingDragged.currentCount -= (DraggedItemController.Instance.itemBeingDragged.maxStackSize - inventorySlots[currentlyOverSlotIndex].item.currentCount);
                            inventorySlots[currentlyOverSlotIndex].item.currentCount = DraggedItemController.Instance.itemBeingDragged.maxStackSize;
                            DraggedItemController.Instance.DisplayItem();
                            inventorySlots[currentlyOverSlotIndex].DisplayItem();
                        }
                    }
                }

                // If not we just swap the item
                else
                {
                    // If equipment slot, only allow equip items
                    if (inventorySlots[currentlyOverSlotIndex].isEquipmentSlot)
                    {
                        // Only allow swap if equippable
                        if (DraggedItemController.Instance.itemBeingDragged.equippable)
                        {
                            inventorySlots[draggedFromSlotIndex].SetItem(inventorySlots[currentlyOverSlotIndex].item);
                            inventorySlots[currentlyOverSlotIndex].SetItem(DraggedItemController.Instance.itemBeingDragged);
                        }
                    }

                    // If dragged from slot still has stacks of the dragged item, we swap the items being dragged with the new item
                    else if (inventorySlots[draggedFromSlotIndex].item)
                    {
                        var itemToSwap = inventorySlots[currentlyOverSlotIndex].item;
                        inventorySlots[currentlyOverSlotIndex].SetItem(DraggedItemController.Instance.itemBeingDragged);
                        DraggedItemController.Instance.SetItem(itemToSwap);
                    }
                    // We just swap the item
                    else
                    {
                        SwapItem();
                        ResetDrag();
                    }
                }               
            }

            // We just put item in the empty slot
            else if (currentlyOverSlotIndex >= 0 && !inventorySlots[currentlyOverSlotIndex].item)
            {
                if (inventorySlots[currentlyOverSlotIndex].isEquipmentSlot)
                {
                    // Only allow swap if equippable
                    if (DraggedItemController.Instance.itemBeingDragged.equippable)
                    {
                        AddItemToSlot(currentlyOverSlotIndex, DraggedItemController.Instance.itemBeingDragged);
                        ResetDrag();
                    }
                }

                else if (splitStack && DraggedItemController.Instance.itemBeingDragged.currentCount > 1)
                {
                    // Add one count of item
                    var itemToSplitAdd = Instantiate(DraggedItemController.Instance.itemBeingDragged);
                    itemToSplitAdd.currentCount = 1;
                    AddItemToSlot(currentlyOverSlotIndex, itemToSplitAdd);

                    DraggedItemController.Instance.itemBeingDragged.currentCount -= 1;
                    DraggedItemController.Instance.DisplayItem();
                } 
                // Else we just add
                else
                {
                    AddItemToSlot(currentlyOverSlotIndex, DraggedItemController.Instance.itemBeingDragged);
                    ResetDrag();
                }  
            }

            // Else we return it to original position
            else
            {
                AddItemToSlot(draggedFromSlotIndex, DraggedItemController.Instance.itemBeingDragged);
                ResetDrag();
            }
        }
    }

    public void CheckForShiftClick() // Checks if shift is held where to move item (quick move)
    {
        if (currentlyOverSlotIndex >= 0 && inventorySlots[currentlyOverSlotIndex].item)
        {
            if (currentlyOverSlotIndex >= 39)
            {
                // Do nothing for now till we add chests and crafting
                // Add to inventory
            }
            // Move from equipment slot to inventory
            else if (currentlyOverSlotIndex >= quickSlotSize + currentInventorySize && currentlyOverSlotIndex <= 38)
            {
                if (AddItem(inventorySlots[currentlyOverSlotIndex].item, "full inventory"))
                {
                    inventorySlots[currentlyOverSlotIndex].RemoveItem();
                }
            }
            // Move from inventory to quick slot
            else if (currentlyOverSlotIndex >= quickSlotSize && currentlyOverSlotIndex <= quickSlotSize + currentInventorySize)
            {
                if (AddItem(inventorySlots[currentlyOverSlotIndex].item, "quick slots only"))
                {
                    inventorySlots[currentlyOverSlotIndex].RemoveItem();
                }
            }
            // Move from quick slot to backpack
            else if (currentlyOverSlotIndex >= 0 && currentlyOverSlotIndex < quickSlotSize)
            {
                if (AddItem(inventorySlots[currentlyOverSlotIndex].item, "inventory only"))
                {
                    inventorySlots[currentlyOverSlotIndex].RemoveItem();
                }
            }
        }       
    }

    private void CheckIfEquippable() // Check whether we allow adding to equipment slot
    {
        
    }

    public void UpdateUI()
    {
        for (int i = 0; i < numberOfFilledSlots; i++)
        {
            inventorySlots[i].DisplayItem();
        }
    }

    // SAVING & LOADING
    public void Save()
    {
        // Save inventory data
        var inventoryData = new InventoryData();
        inventoryData.hasInventory = hasInventory;
        inventoryData.numberOfFilledSlots = numberOfFilledSlots;
        GameManager.Instance.gameData.inventory = inventoryData;

        // Save item data
        GameManager.Instance.gameData.items = new ItemData[numberOfFilledSlots];

        for (int i = 0; i < numberOfFilledSlots; i++)
        {
            var data = new ItemData();

            if (inventorySlots[i].item)
            {
                data.itemName = inventorySlots[i].item.itemName;
                data.currentCount = inventorySlots[i].item.currentCount;
                data.durability = inventorySlots[i].item.durability;

                GameManager.Instance.gameData.items[i] = data;
            }
            else
            {
                GameManager.Instance.gameData.items[i] = null;
            }
        }
    }

    public void Load()
    {
        hasInventory = GameManager.Instance.gameData.inventory.hasInventory;
        numberOfFilledSlots = GameManager.Instance.gameData.inventory.numberOfFilledSlots;

        for (int i = 0; i < numberOfFilledSlots; i++)
        {
            if (GameManager.Instance.gameData.items[i].itemName != "")
            {
                inventorySlots[i].item = Instantiate(ItemManager.Instance.itemDatabase[GameManager.Instance.gameData.items[i].itemName]);
                inventorySlots[i].item.currentCount = GameManager.Instance.gameData.items[i].currentCount;
                inventorySlots[i].item.durability = GameManager.Instance.gameData.items[i].durability;
            }
            else
            {
                inventorySlots[i].item = null;
            }           
        }

        UpdateUI();   
    }

    [System.Serializable]
    public class InventoryData
    {
        public bool hasInventory;
        public int numberOfFilledSlots;
    }

    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public int currentCount; // Amount in the stack
        public int durability;
    }
}
