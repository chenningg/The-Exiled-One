using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootContainerController : MonoBehaviour {

    public Item item;
    public SpriteRenderer itemSprite;
    public SortingOrderObjects sortingOrderScript;
    public Rigidbody2D rb;
    public bool isDroppedByPlayer = false; // Check if item is dropped by player

    private bool checkToAdd = false; // Check whether we should add the item or not

    // Allow pickup of item after 3s
    private void Start()
    {
        Invoke("DestroyRB", Random.Range(0.5f, 0.7f));

        if (isDroppedByPlayer)
        {
            Invoke("AllowPickup", 1.5f);
        }
        else
        {
            Invoke("AllowPickup", 0.5f);
        }  
    }

    private void AllowPickup()
    {
        checkToAdd = true;
    }

    private void DestroyRB()
    {
        if (rb && enabled)
        {
            sortingOrderScript.RefreshSortingOrder();
            Destroy(rb);
        }
    }

    public void SetItem(Item itemToAdd)
    {
        item = itemToAdd;
        if (item)
        {
            itemSprite.sprite = item.itemSprite;
            sortingOrderScript.RefreshSortingOrder();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //print(collision.tag);
        if (checkToAdd)
        {
            // Try to add item into inventory
            if (collision.CompareTag("Player Item Pickup Trigger"))
            {
                bool addItemCheck = Inventory.Instance.AddItem(item, "full inventory");

                // Item has been added to inventory, we make it fly to player and remove it from the world
                if (addItemCheck)
                {
                    StartCoroutine(ItemAdded());
                }
            }
        }     
    }

    private IEnumerator ItemAdded()
    {
        var currentTime = 0f;

        while (currentTime < 1f)
        {
            var playerPosition = PlayerController.Instance.gameObject.transform.position + new Vector3(0, 1, 0);
            currentTime += 0.1f;
            transform.position = Vector3.Lerp(transform.position, playerPosition, currentTime);
            yield return new WaitForSeconds(0.01f);
        }

        // Destroy item
        Destroy(gameObject);
    }
}
