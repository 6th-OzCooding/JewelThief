using TeamConvention.Interfaces;
using UnityEngine;

public class StoneTrap : BaseTrap
{
    [SerializeField] private Transform spawnPo;
    private bool _isWorked = false;
    
    private void SpawnRemainItem(string itemId, Vector3 playerPos) 
    {
        var stoneObject = GameManager.Pool.SpawnFromPool("ItemObject", playerPos);
        stoneObject.GetComponent<Item>().InitFromSpawner(itemId);
    }

    protected override void OnDisarm()
    {
        base.OnDisarm();
        _isDisarmed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isWorked|| _isDisarmed) { return; }
        ItemData stoneData = GameManager.DataTable.GetItemData("Item_Junk_Stone");
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어 감지 ");

            if (other.TryGetComponent(out IInventoryOwner inventoryOwner))
            {
                Debug.Log("트랩 발동 - 인벤토리 채우기 시작");

                if (stoneData == null)
                {
                    Debug.LogError("stoneData가 등록되지 않았습니다!");
                    return;
                }

                int dropCount = 10;


                for (int i = 0; i < dropCount; i++)
                {
                    // 가방에 넣기
                    if (inventoryOwner.TryAcquireItem(stoneData, HoldType.Pocket))
                    {
                        Debug.Log($"[함정] {stoneData.Name}을(를) 강제로 넣었습니다. ({i + 1}/{dropCount})");
                    }
                    else
                    {
                        Debug.Log($"[함정] 가방이 가득 찼습니다! ({i + 1}/{dropCount})");
                    }
                }
                for (int i = 0; i < dropCount; i++)
                {
                        Debug.Log($"[함정] 플레이어 주변 바닥에 생성합니다. ({i + 1}/{dropCount})");
                        SpawnRemainItem(stoneData.Id, spawnPo.position);
                }
                _isWorked = true;
            }
        }
    }

}
