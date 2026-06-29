using UnityEngine;

/// <summary>
/// 플레이어 손 Root 아래에 현재 손에 든 아이템의 임시 시각 오브젝트를 표시합니다.
/// </summary>
public class PlayerHandItemViewer : MonoBehaviour
{
    private const string LEFT_HAND_ROOT_NAME = "LeftHandRoot";
    private const string RIGHT_HAND_ROOT_NAME = "RightHandRoot";

    [Header("손 Root")]
    [SerializeField] private Transform _leftHandRoot;
    [SerializeField] private Transform _rightHandRoot;

    private GameObject _leftHandVisualObject;
    private GameObject _rightHandVisualObject;

    /// <summary>
    /// 양손의 현재 아이템 표시를 갱신합니다.
    /// </summary>
    public void RefreshHands(InventoryItem leftHandItem, InventoryItem rightHandItem)
    {
        SetHandItem(PlayerHandType.Left, leftHandItem);
        SetHandItem(PlayerHandType.Right, rightHandItem);
    }

    /// <summary>
    /// 지정한 손에 아이템 시각 오브젝트를 표시하거나 제거합니다.
    /// </summary>
    public void SetHandItem(PlayerHandType handType, InventoryItem inventoryItem)
    {
        EnsureHandRoots();

        if (handType == PlayerHandType.Left)
        {
            ReplaceHandVisual(ref _leftHandVisualObject, _leftHandRoot, inventoryItem);
            return;
        }

        if (handType == PlayerHandType.Right)
        {
            ReplaceHandVisual(ref _rightHandVisualObject, _rightHandRoot, inventoryItem);
        }
    }

    private void EnsureHandRoots()
    {
        if (_leftHandRoot == null)
        {
            _leftHandRoot = FindChildByName(transform, LEFT_HAND_ROOT_NAME);
        }

        if (_rightHandRoot == null)
        {
            _rightHandRoot = FindChildByName(transform, RIGHT_HAND_ROOT_NAME);
        }
    }

    private void ReplaceHandVisual(ref GameObject currentVisualObject, Transform handRoot, InventoryItem inventoryItem)
    {
        ClearHandVisual(ref currentVisualObject);

        if (inventoryItem == null || inventoryItem.ItemData == null)
            return;

        if (handRoot == null)
        {
            Debug.LogWarning("손 아이템을 표시할 HandRoot가 연결되지 않았습니다.");
            return;
        }

        GameObject prefab = LoadHeldItemPrefab(inventoryItem.ItemData);
        if (prefab == null)
            return;

        Vector3 prefabLocalScale = prefab.transform.localScale;

        currentVisualObject = Instantiate(prefab, handRoot);
        currentVisualObject.name = $"{inventoryItem.ItemData.Id}_HeldVisual";

        Transform visualTransform = currentVisualObject.transform;
        visualTransform.localPosition = Vector3.zero;
        visualTransform.localRotation = Quaternion.identity;
        visualTransform.localScale = GetScalePreservingPrefabShape(handRoot, prefabLocalScale);

        InitializeHeldVisual(currentVisualObject, inventoryItem.ItemData);
        DisableWorldBehaviours(currentVisualObject);
    }

    private void ClearHandVisual(ref GameObject currentVisualObject)
    {
        if (currentVisualObject == null)
            return;

        Destroy(currentVisualObject);
        currentVisualObject = null;
    }

    private GameObject LoadHeldItemPrefab(ItemData itemData)
    {
        string prefabAddress = ResolvePrefabAddress(itemData);
        if (string.IsNullOrEmpty(prefabAddress))
        {
            Debug.LogWarning($"{itemData.Name}의 손 표시용 프리팹 주소가 없습니다.");
            return null;
        }

        GameObject prefab = GameManager.Resource.GetLoadedAsset<GameObject>(prefabAddress);
        if (prefab == null)
        {
            Debug.LogWarning($"{itemData.Name}의 손 표시용 프리팹을 찾을 수 없습니다. Address: {prefabAddress}");
        }

        return prefab;
    }

    private string ResolvePrefabAddress(ItemData itemData)
    {
        if (itemData == null)
            return null;

        if (itemData.Husks == "ToolObject")
            return "Pool_Tool";

        if (itemData.Husks == "JewelObject")
            return "Pool_Jewel";

        if (itemData.Husks == "PaintingObject")
            return "Pool_Painting";

        if (itemData.Husks == "StatueObject")
            return "Pool_Statue";

        return itemData.Husks;
    }

    private void DisableWorldBehaviours(GameObject visualObject)
    {
        Collider[] colliders = visualObject.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Rigidbody[] rigidbodies = visualObject.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].linearVelocity = Vector3.zero;
            rigidbodies[i].angularVelocity = Vector3.zero;
            rigidbodies[i].useGravity = false;
            rigidbodies[i].isKinematic = true;
        }

        BaseInteractableObject[] interactables = visualObject.GetComponentsInChildren<BaseInteractableObject>(true);
        for (int i = 0; i < interactables.Length; i++)
        {
            interactables[i].enabled = false;
        }

        JewelPhysicsApplier[] jewelPhysicsAppliers = visualObject.GetComponentsInChildren<JewelPhysicsApplier>(true);
        for (int i = 0; i < jewelPhysicsAppliers.Length; i++)
        {
            jewelPhysicsAppliers[i].enabled = false;
        }
    }

    private void InitializeHeldVisual(GameObject visualObject, ItemData itemData)
    {
        if (visualObject == null || itemData == null)
            return;

        BaseInteractableObject[] interactables = visualObject.GetComponentsInChildren<BaseInteractableObject>(true);
        for (int i = 0; i < interactables.Length; i++)
        {
            interactables[i].InitFromSpawner(itemData.Id);
        }
    }

    private Vector3 GetScalePreservingPrefabShape(Transform handRoot, Vector3 prefabLocalScale)
    {
        if (handRoot == null)
            return prefabLocalScale;

        Vector3 handWorldScale = handRoot.lossyScale;
        return new Vector3(
            GetScaleAdjustedByParent(prefabLocalScale.x, handWorldScale.x),
            GetScaleAdjustedByParent(prefabLocalScale.y, handWorldScale.y),
            GetScaleAdjustedByParent(prefabLocalScale.z, handWorldScale.z)
        );
    }

    private float GetScaleAdjustedByParent(float targetScale, float parentScale)
    {
        if (Mathf.Approximately(parentScale, 0f))
            return targetScale;

        return targetScale / parentScale;
    }

    private Transform FindChildByName(Transform currentTransform, string targetName)
    {
        if (currentTransform == null)
            return null;

        if (currentTransform.name == targetName)
            return currentTransform;

        for (int i = 0; i < currentTransform.childCount; i++)
        {
            Transform foundChild = FindChildByName(currentTransform.GetChild(i), targetName);
            if (foundChild != null)
                return foundChild;
        }

        return null;
    }
}
