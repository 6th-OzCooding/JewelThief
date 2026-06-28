using UnityEngine;

public class JewelPhysicsApplier : MonoBehaviour
{
    private Rigidbody _rb;
    private MeshCollider _mc;

    [SerializeField] private string _gemLayerName = "Gem";

    // 퍼즐 모드에 맞게 리지드바디 제약 걸기
    public void EnterPuzzleMode()
    {
        if (_rb == null) SetupPhysics();

        int layerIndex = LayerMask.NameToLayer(_gemLayerName);
        if (layerIndex != -1) gameObject.layer = layerIndex;

        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY;
    }

    // 퍼즐 모드에서 나갈때 제약 되돌리기
    public void ExitPuzzleMode()
    {
        if (_rb == null) return;

        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.None;
    }

    // 콜라이더 입히기
    private void SetupPhysics()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody>();

        _mc = GetComponent<MeshCollider>();

        if (_mc == null)
            _mc = gameObject.AddComponent<MeshCollider>();

        _mc.convex = true;

        MeshFilter visualMesh = GetComponentInChildren<MeshFilter>();

        if (visualMesh != null)
            _mc.sharedMesh = visualMesh.sharedMesh;
    }
}
