using TeamConvention.Interfaces;
using UnityEngine;

public class DropTrap : BaseDisarmableObejct
{
    [SerializeField] private float detectionRange = 10f;
    private bool _isWorked = false;
   
    private void SpawnDroppedItem(string itemId, Vector3 playerPos)
    {
        string poolPrefab = GameManager.DataTable.GetItemData(itemId).Husks;
        var stoneObject = GameManager.Pool.SpawnFromPool("Pool_Jewel", playerPos);
        stoneObject.GetComponent<Jewel>().InitFromSpawner(itemId);
    }
    protected override void LoadData(string id) { }
    protected override void OnDisarm()
    {
        base.OnDisarm();
        _isDisarmed = true;
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.TryGetComponent(out IInventoryOwner inventoryOwner)) 
    //    {
    //        Debug.Log("트랩 발동 - 인벤토리 털기 시작");

    //        int dropCount = 10;

    //        for (int i = 0; i < dropCount; i++)
    //        {
    //            var items = inventoryOwner.BagItems;
    //            if (items == null || items.Count == 0)
    //            {
    //                Debug.Log("가방이 비어서 더 이상 털 아이템이 없습니다.");
    //                break; // 아이템이 10개보다 적으면 반복문 탈출
    //            }

    //            // 가방에서 랜덤으로 아이템 하나 선택
    //            int randomIndex = Random.Range(0, items.Count);
    //            InventoryItem targetItem = items[randomIndex];

    //            // 인벤토리 오너의 기존 메서드를 사용해 아이템 제거
    //            inventoryOwner.RemoveBagItem(targetItem);

    //            Debug.Log($"[함정] {targetItem.ItemData.Name}을(를) 강제로 떨어뜨렸습니다. ({i + 1}/{dropCount})");
    //            //바닥에 떨어뜨리기
    //            SpawnDroppedItem(targetItem.ItemData.Id, other.transform.position);
    //        }
    //        _isWorked = true;
    //    }
    //}
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("플레이어가 부딪혔습니다");

            if (collision.gameObject.TryGetComponent(out IInventoryOwner inventoryOwner))
            {
                Debug.Log("트랩 발동 - 인벤토리 털기 시작");

                int dropCount = 10;

                for (int i = 0; i < dropCount; i++)
                {
                    var items = inventoryOwner.BagItems;
                    if (items == null || items.Count == 0)
                    {
                        Debug.Log("가방이 비어서 더 이상 털 아이템이 없습니다.");
                        break; // 아이템이 10개보다 적으면 반복문 탈출
                    }

                    // 가방에서 랜덤으로 아이템 하나 선택
                    int randomIndex = Random.Range(0, items.Count);
                    InventoryItem targetItem = items[randomIndex];

                    // 인벤토리 오너의 기존 메서드를 사용해 아이템 제거
                    inventoryOwner.RemoveBagItem(targetItem);

                    Debug.Log($"[함정] {targetItem.ItemData.Name}을(를) 강제로 떨어뜨렸습니다. ({i + 1}/{dropCount})");
                    //바닥에 떨어뜨리기
                    SpawnDroppedItem(targetItem.ItemData.Id, collision.transform.position);
                }
            }
        }
    }
}
