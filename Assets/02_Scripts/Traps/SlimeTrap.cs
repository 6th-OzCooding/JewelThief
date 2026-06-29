using UnityEngine;

public class SlimeTrap : BaseDisarmableObejct
{
    [SerializeField] private DebuffType debuffType = DebuffType.MoveSpeed;
    [SerializeField] private float debuffValue = 0.2f; // 50% 느려짐
    [SerializeField] private float duration = 3.0f;    // 한 번 갱신할 때 1.5초 지속
    private Collider trapCollider;
    [SerializeField] private float checkInterval = 0.5f; // 0.5초마다 플레이어에게 디버프 주사
    private float nextCheckTime = 0f;
    void Awake()
    {
        // 내 몸에 붙은 콜라이더(Box, Sphere 등)를 미리 찾아둡니다.
        trapCollider = GetComponent<Collider>();
    }
    private void OnEnable()
    {
        _isInteractable = true;
    }
    protected override void LoadData(string id) { }
    protected override void OnDisarm()
    {
        base.OnDisarm();
        _isDisarmed = true;
        gameObject.SetActive(false);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 0.5초마다만 작동.
            if (Time.time >= nextCheckTime)
            {
                nextCheckTime = Time.time + checkInterval;

                if (other.TryGetComponent(out IDebuffable debuffTarget))
                {
                    Debug.Log($"[거미줄 유지] {debuffType} 디버프 지속 주입");
                   
                    debuffTarget.ApplyDebuff(debuffType, debuffValue, duration);
                }
            }
        }
    }
}
