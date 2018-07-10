using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropLoot : MonoBehaviour {

    public Loot[] lootTable;

	// Use this for initialization
	void Start () {
        // If no loot drop, remove this component
		if (lootTable.Length == 0)
        {
            Destroy(this);
        }
	}
	
    public void DropItems()
    {
        for (int i = 0; i < lootTable.Length; i++)
        {
            if (Random.Range(0f, 1f) <= lootTable[i].dropChance)
            {
                // Amount to drop
                var dropAmount = Random.Range(lootTable[i].minDropAmount, lootTable[i].maxDropAmount + 1);

                // If stackable we just stack or if just one we just spawn it and drop else we spawn individually
                if (lootTable[i].loot.isStackable || dropAmount == 1)
                {
                    // Instantiate loot container and assign its item
                    var lootContainerScript = Instantiate(PrefabManager.Instance.prefabDatabase["Loot Container"]).GetComponent<LootContainerController>();
                    lootContainerScript.transform.position = (gameObject.transform.position + new Vector3(0, 0.1f, 0));
                    lootContainerScript.SetItem(Instantiate(lootTable[i].loot));
                    lootContainerScript.item.currentCount = dropAmount;
                    lootContainerScript.isDroppedByPlayer = false;
                    lootContainerScript.rb.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(6f, 8f)), ForceMode2D.Impulse);
                }
                else
                {
                    // We drop individually
                    for (int j = 0; j < dropAmount; j++)
                    {
                        // Instantiate loot container and assign its item
                        var lootContainerScript = Instantiate(PrefabManager.Instance.prefabDatabase["Loot Container"]).GetComponent<LootContainerController>();
                        lootContainerScript.transform.position = (gameObject.transform.position + new Vector3(0, 0.1f, 0));
                        lootContainerScript.SetItem(Instantiate(lootTable[i].loot));
                        lootContainerScript.item.currentCount = 1;
                        lootContainerScript.isDroppedByPlayer = false;
                        lootContainerScript.rb.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(6f, 8f)), ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class Loot
    {
        public Item loot;
        [Range(0f, 1f)]
        public float dropChance;
        public int minDropAmount;
        public int maxDropAmount;
    }
}
