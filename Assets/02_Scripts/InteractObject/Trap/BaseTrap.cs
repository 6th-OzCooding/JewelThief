using UnityEngine;

public class BaseTrap : BaseDisarmableObejct
{
    TrapData trapData;

    protected override void LoadData(string id)
    {
        trapData = GameManager.DataTable.GetTrapData(id);
        _disarmObjId = trapData.Id;
        _disarmObjName = trapData.Name;

        SpawnTrapPrefab();
    }

    private void SpawnTrapPrefab()
    {
        GameManager.Pool.SpawnFromPool(trapData.ObjectPrefabPath, this.transform, true);
    }
}
