using UnityEngine;

public class InteractableDoor : BaseDisarmableObejct
{
    [SerializeField] private InteractableContainerAnimeController _animController;
    [SerializeField] private AudioSource _audioSource;

    private string _doorMeshPrefabPath;
    private GameObject _doorMeshObject;

    private void OnEnable()
    {
        LoadData("Door_01");
    }

    private void OnDisable()
    {
        Destroy(_doorMeshObject);
    }

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

        if (_animController != null)
        {
            _animController.InitMeshAnime(obj);
            _audioSource = _doorMeshObject.GetComponentInChildren<AudioSource>();
        }
    }

    private void OpenDoor()
    {
        _animController.SetStat(InteractableObjectAnimState.Open);
        _audioSource.Play();
    }

    protected override void OnDisarm(bool isCollectToolUse)
    {
        Door data = GameManager.DataTable.GetDoorData(_disarmObjId);
        if (isCollectToolUse)
        {
            _isInteractable = false;
            OpenDoor();
        }
        else
        {
            _isInteractable = false;
            // 시간 감소 리스트의 2번째가 줄어드는 것이므로 1로 설정
            GameManager.Alert.ReduceTimer(_timeReductionAmountList[1]);
            OpenDoor();
        }
    }
}
