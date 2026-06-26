using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class BagOverloadDetector : MonoBehaviour
{
    [Header("공간 설정")]
    [SerializeField] private SpriteRenderer _boundaryLineRenderer; // 경계 선 (넘침 체크 선)
    [SerializeField] private float _returnDelay = 0.5f; // 선을 넘은 후 판정까지 버티는 시간

    private Dictionary<Collider, CancellationTokenSource> _activeTrackings = new Dictionary<Collider, CancellationTokenSource>();

    public bool IsSpaceSafe
    {
        get
        {
            return _activeTrackings.Count == 0;
        }
    }

    private void Start()
    {
        UpdateLineColor();
    }

    private void OnDestroy()
    {
        foreach (var cts in _activeTrackings.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _activeTrackings.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Jewel>() != null)
        {
            if (!_activeTrackings.ContainsKey(other))
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                _activeTrackings.Add(other, cts);

                UpdateLineColor();

                TrackOverflowGemAsync(other, cts.Token).Forget();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_activeTrackings.TryGetValue(other, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _activeTrackings.Remove(other);

            UpdateLineColor();
        }
    }

    // 보석 넘친 상태 감시 함수
    private async UniTaskVoid TrackOverflowGemAsync(Collider gemCollider, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(_returnDelay), delayTiming: PlayerLoopTiming.Update, cancellationToken: token);

            ReturnToPickupSpace(gemCollider);
        }
        catch (System.OperationCanceledException)
        {

        }
    }

    // 보석을 줍기 공간으로 되돌리는 함수
    private void ReturnToPickupSpace(Collider gemCollider)
    {
        if (gemCollider == null) return;

        if (_activeTrackings.TryGetValue(gemCollider, out var cts))
        {
            cts.Dispose();
            _activeTrackings.Remove(gemCollider);
            UpdateLineColor();
        }

        if (JewelInventoryManager.Instance != null)
        {
            Jewel gem = gemCollider.GetComponent<Jewel>();
            if (gem != null)
            {
                JewelInventoryManager.Instance.RemoveJewelFromBag(gem);
                Debug.Log($"<color=orange>{gemCollider.name}</color>이(가) 가방 용량을 초과하여 임시 보관함으로 반환되었습니다.");
            }
        }
    }

    // 경계선 색깔 변경 함수
    private void UpdateLineColor()
    {
        if (_boundaryLineRenderer == null) return;

        if (_activeTrackings.Count > 0)
        {
            _boundaryLineRenderer.color = Color.red; // 위험 상태
        }
        else
        {
            _boundaryLineRenderer.color = Color.green; // 안전 상태
        }
    }
}
