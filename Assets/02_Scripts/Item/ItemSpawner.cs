using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public void SpawnItemFromPool(string itemId, Vector3 spawnPosition)
    {
        ItemData itemData = GameManager.DataTable.GetItemData(itemId);
        if (itemData == null)
        {
            Debug.LogError($"[Spawner] 해당 ID의 아이템 데이터가 없습니다: {itemId}");
            return;
        }
        string targetPoolId = itemData.PrefabPath;

        ItemBase spawnedItem = GameManager.Pool.SpawnFromPool<ItemBase>(targetPoolId, spawnPosition);

        if (spawnedItem != null)
        {
            spawnedItem.InitItem(itemData);
        }
    }
}
