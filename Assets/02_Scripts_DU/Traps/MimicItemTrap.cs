using UnityEngine;
using TeamConvention.Interfaces;

public class MimicItemTrap : MonoBehaviour, IInteractable
{
    [Header("트랩 데이터")]
    [SerializeField] private int _trapId = 40000001;

    [Header("기본 설정")]
    [SerializeField] private string _interactPrompt = "보석 획득(E)";
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _soundRadius = 1f;
    [SerializeField] private float _timeimeReductionAmount = 1f;

    private bool _isDisarmed = false;   //도구로 무력화됐는지 여부 확인

    public string InteractPrompt => _interactPrompt;

    public void Interact(PlayerController player)
    {
        if (_isDisarmed)
        {
            Debug.Log($"[함정 무력화 상태] ID: {_trapId} - 이미 도구로 해제된 함정입니다.");   //도구로 무력화할 경우 보석으로 획득

            Destroy(gameObject);
            return;
        }
        

        Debug.Log($"[함정 발동] ID: {_trapId} - 함정 발동!");

        player.TakeDamage(_damage);

        if (GameManager.Instance != null && GameManager.Instance.AlertManager != null)
        {
            GameManager.Instance.AlertManager.ReduceTimer(_timeReductionAmount);
        }

            TriggerNoise();   //소음 발생

            Destroy(gameObject, 0.2f);   //파괴
     }

        public void DisarmTrap()   //임시로 설정한 트랩 무효화 함수(나중에 명령문 통일해야 함)
    {
        _isDisarmed = true;
        _interactPrompt = "함정 해제된 보석 획득 (E)";
        Debug.Log($"[함정 해제] ID: {_trapId} - 도구에 의해 무력화된 함정입니다.");
    }

    private void TriggerNoise()
    {
        Collider[] caughtEnemies = Physics.OverlapSphere(transform.position, _soundRadius);
        foreach (Collider col in caughtEnemies)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.HearNoise(transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _soundRadius);
    }
}
