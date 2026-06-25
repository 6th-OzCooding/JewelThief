using TeamConvention.Interfaces;
using UnityEngine;

public class DropTrap : BaseDisarmableObejct
{
    [SerializeField] private float detectionRange = 10f;
    private bool _isWorked = false;
    void Update()
    {
        if (IsDisarmed) return;
        if (_isWorked) return;
        CheckPlayerTrapped();
    }

    private void CheckPlayerTrapped()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("플레이어 감지 ");

                if (hit.collider.TryGetComponent(out IInventoryOwner inventoryOwner))
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
                        SpawnDroppedItem(targetItem.ItemData.Id, hit.collider.transform.position);
                    }
                    _isWorked = true;
                }
            }
        }
    }
    private void SpawnDroppedItem(string itemId, Vector3 playerPos)
    {
        string poolPrefab = GameManager.DataTable.GetItemData(itemId).Husks;
        var stoneObject = GameManager.Pool.SpawnFromPool("Pool_Jewel", playerPos);
        stoneObject.GetComponent<Jewel>().InitFromSpawner(itemId);


    }
    private void OnDrawGizmos()
    {
        // 1. 기본 레이의 시작점과 끝점 계산
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + (Vector3.down * detectionRange);

        // 2. 에디터 재생 중(런타임)일 때와 아닐 때를 구분해서 시각화하면 좋습니다.
        if (Application.isPlaying)
        {
            // 재생 중일 때 실제로 레이를 쏴서 부딪힌 곳이 있는지 체크
            if (Physics.Raycast(startPosition, Vector3.down, out RaycastHit hit, detectionRange))
            {
                // 무언가 감지되었다면 녹색선으로 그리고, 부딪힌 지점에 빨간 구체 생성
                Gizmos.color = Color.green;
                Gizmos.DrawLine(startPosition, hit.point);

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, 0.2f); // 부딪힌 지점에 조그만 구체 그리기
            }
            else
            {
                // 아무것도 감지되지 않았다면 평소엔 빨간색 선으로 표시
                Gizmos.color = Color.red;
                Gizmos.DrawLine(startPosition, endPosition);
            }
        }
        else
        {
            // 게임이 실행 중이 아닐 때(에디터 편집 상태)는 상시 노란색 선으로 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPosition, endPosition);
        }
    }
    protected override void LoadData(string id) { }
    protected override void OnDisarm()
    {
        base.OnDisarm();
        _isDisarmed = true;
    }
    
}
