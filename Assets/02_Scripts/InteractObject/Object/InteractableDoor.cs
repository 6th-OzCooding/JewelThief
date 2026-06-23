using NUnit.Framework;
using TeamConvention.Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class InteractableDoor : BaseDisarmableObejct
{
    private string _doorMeshPrefabPath;
    private GameObject _doorMeshObject;

    protected override void LoadData(string id)
    {
        Door data = GameManager.DataTable.GetDoorData(id);
        _disarmObjId = data.Id;
        _isDisarmed = data.IsDisarm;
        _doorMeshPrefabPath = data.DoorMeshPrefabPath;
        _requiredToolIdList = data.DoorRequiresToolIdList;
        _timeReductionAmountList = data.DoorTimeReductionAmountList;
        SpawnMeshBox();
    }

    private async void SpawnMeshBox()
    {
        if (_doorMeshPrefabPath == null || _doorMeshPrefabPath == "")
        {
            Debug.LogError("Mesh 프리팹 경로 없음");
            return;
        }

        GameObject obj = await Addressables.InstantiateAsync(_doorMeshPrefabPath).Task;
        if (obj == null) return;

        obj.transform.SetParent(transform, false);
        _doorMeshObject = obj;
        // 현재 애니메이션 없음
        // _animController.InitMeshAnime(obj);
    }

    private void DestroyMeshBox()
    {
        Destroy(_doorMeshObject);
    }

    protected override void OnDisarm(bool isCollectToolUse)
    {
        InteractableContainerData data = GameManager.DataTable.GetInteractableObjectData(_disarmObjId);
        if (isCollectToolUse)
        {
            Destroy(this.gameObject);
        }
        else
        {
            // TODO(안우재 2026-6-24) : 강제로 열었기에 ChangeStat 전에 차감 시간을 적용해야함
            Destroy(this.gameObject);
        }
    }
}
