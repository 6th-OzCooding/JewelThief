using UnityEngine;

/// <summary>
/// Links a 3D bag-view object to the runtime inventory item it represents.
/// </summary>
public class BagInventoryViewItem : MonoBehaviour
{
    /// <summary>
    /// Runtime inventory item represented by this view object.
    /// </summary>
    public InventoryItem InventoryItem { get; private set; }

    /// <summary>
    /// Assigns the runtime inventory item represented by this view object.
    /// </summary>
    public void Initialize(InventoryItem inventoryItem)
    {
        InventoryItem = inventoryItem;
    }
}
