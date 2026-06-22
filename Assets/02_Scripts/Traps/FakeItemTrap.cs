using UnityEngine;
using TeamConvention.Interfaces;

public class FakeItemTrap : MonoBehaviour, IInteractable, IDisarmable
{
    [Header("트랩 데이터")]
    [SerializeField] private int _trapId = 40000001;

    [Header("기본 설정")]
    [SerializeField] private string _interactPrompt = "보석 획득";
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _soundRadius = 1f;
    [SerializeField] private float _timeReductionAmount = 1f;

    private bool _isDisarmed = false;
    private string _name = "가짜 보석";

    public string Name => _name;

    public string InteractPrompt => _interactPrompt;

    public bool CanInteract() => !_isDisarmed;

    public void Interact(IInteractor interactor)
    {
        if (!CanInteract()) return;

        Debug.Log($"[함정 발동] ID: {_trapId} - 함정 발동!");

        if (interactor != null)
        {
            // interactor.Transform.GetComponent<PlayerController>()?.TakeDamage(_damage);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SendMessage("ReduceTimer", _timeReductionAmount, SendMessageOptions.DontRequireReceiver);
        }

        TriggerNoise(); // 소음 발생
        Destroy(gameObject, 0.2f); // 파괴
    }

    public void Disarm()
    {
        _isDisarmed = true;
        _interactPrompt = "무력화된 함정";
        Debug.Log($"[함정 해제] ID: {_trapId} - 안전하게 해제되었습니다.");
    }

    private void TriggerNoise()
    {
        Collider[] caughtEnemies = Physics.OverlapSphere(transform.position, _soundRadius);
        foreach (Collider col in caughtEnemies)
        {
            // Enemy enemy = col.GetComponent<Enemy>();
            // if (enemy != null) enemy.HearNoise(transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _soundRadius);
    }
}