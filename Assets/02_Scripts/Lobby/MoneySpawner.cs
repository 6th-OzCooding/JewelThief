using UnityEngine;

public static class MoneySpawner
{
    // TODO(김경훈 2026-06-23): 돈 프리팹 리소스를 데이터로 분리하기. 지금은 임시 키.
    private const string MONEY_PREFAB_KEY = "Money";

    public static void SpawnMoney(int amount, Vector3 position, Quaternion rotation)
    {
        if (amount <= 0)
            return;

        // TODO(김경훈 2026-06-23): 골드 액수에 따라 다른 프리팹(동전/지폐 단위)을 쓸지 여부 결정 필요
        GameObject moneyPrefab = GameManager.Resource.GetLoadedAsset<GameObject>(MONEY_PREFAB_KEY);
        if (moneyPrefab == null)
        {
            Debug.LogError("MoneyPickup 프리팹을 로드하지 못했습니다.");
            return;
        }

        // TODO(김경훈 2026-06-23): PoolManager로 교체 검토 (반복 스폰되는 오브젝트이므로)
        GameObject instance = GameObject.Instantiate(moneyPrefab, position, rotation);

        if (instance.TryGetComponent(out Money money))
        {
            money.SetAmount(amount);
        }
        else
        {
            Debug.LogError("MoneyPickup 프리팹에 MoneyPickup 컴포넌트가 없습니다.");
        }
    }
}