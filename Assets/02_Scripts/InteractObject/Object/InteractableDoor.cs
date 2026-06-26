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
        _isInteractable = true;
        SpawnMeshDoor();
    }

    private void SpawnMeshDoor()
    {
        if (string.IsNullOrEmpty(_doorMeshPrefabPath))
        {
            Debug.LogError("Mesh 프리팹 경로 없음");
            return;
        }

        GameObject prefab = GameManager.Resource.GetLoadedAsset<GameObject>(_doorMeshPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Visual 프리팹이 ResourceManager에 로드되어 있지 않습니다: {_doorMeshPrefabPath}");
            return;
        }

        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        _doorMeshObject = obj;
        
        // 현재 애니메이션 없음
        // _animController.InitMeshAnime(obj);
    }

    private void DestroyMeshDoor()
    {
        if (_doorMeshObject == null) return;
            
        Destroy(_doorMeshObject);
        _doorMeshObject = null;
    }

    protected override void OnDisarm(bool isCollectToolUse)
    {
        Door data = GameManager.DataTable.GetDoorData(_disarmObjId);
        if (isCollectToolUse)
        {
            _isInteractable = false;
            DestroyMeshDoor();
        }
        else
        {
            _isInteractable = false;
            // TODO(안우재 2026-6-24) : 강제로 열었기에 ChangeStat 전에 차감 시간을 적용해야함
            //                          시간 차감 로직 성준님께 여쭤보기
            DestroyMeshDoor();
        }
    }
}
