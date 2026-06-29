using TeamConvention.Interfaces;
using UnityEngine;

public class DropTrap : BaseDisarmableObejct
{
    private DropTrapAnimController _animController;
    private bool _isActivated = false;
    private void OnEnable()
    {
        _animController = GetComponent<DropTrapAnimController>();
    }
    private void SpawnDroppedItem(string itemId, Vector3 playerPos)
    {
        var dropObject = GameManager.Pool.SpawnFromPool("ItemObject", playerPos);
        dropObject.GetComponent<Item>().InitFromSpawner(itemId);
    }
    protected override void LoadData(string id) { }
    protected override void OnDisarm()
    {
        base.OnDisarm();
        _isDisarmed = true;
        _animController.SetState(TrapAnimState.Broken);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (_isDisarmed) return;
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
                        break; 
                    }
                    
                    int randomIndex = Random.Range(0, items.Count);
                    InventoryItem targetItem = items[randomIndex];
                    
                    inventoryOwner.RemoveBagItem(targetItem);

                    Debug.Log($"[함정] {targetItem.ItemData.Name}을(를) 강제로 떨어뜨렸습니다. ({i + 1}/{dropCount})");
                    //바닥에 떨어뜨리기
                    SpawnDroppedItem(targetItem.ItemData.Id, collision.transform.position);
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_isActivated || _isDisarmed) { return; }
        if (other.CompareTag("Player"))
        {
            _isActivated = true;
            Debug.Log("플레이어 감지 ");
            if (_animController != null)
            {
                _animController.SetState(TrapAnimState.Trapped);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isActivated = false;
            Debug.Log("플레이어 감지되지 않음");
            if (_animController != null)
            {
                _animController.SetState(TrapAnimState.Idle);
            }
        }
    }
}
