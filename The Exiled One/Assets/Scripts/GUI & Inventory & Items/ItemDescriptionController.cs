using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescriptionController : MonoBehaviour {

    public Text itemName;
    public Text itemType;
    public Text itemDescription;

    public void SetItem(Item item)
    {
        itemName.text = LocalizationManager.Instance.LocalizeText(item.itemName);
        itemType.text = LocalizationManager.Instance.LocalizeText(item.itemType.ToString());
        itemDescription.text = LocalizationManager.Instance.LocalizeText(item.description);
    }
}
