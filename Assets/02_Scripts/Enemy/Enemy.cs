using UnityEngine;

public class Enemy : MonoBehaviour
{
    // 적의 애니메이션을 담는 변수
    private Animator _anim;
    // 적의 시야를 정하는 거리와 각도
    private float _viewRadius = 2.0f;
    private float _viewAngle = 120.0f;


    private void Start()
    {
        // 애니메이션을 찾고 그 애니메이션을 담을 변수인 _anim으로 지정
        _anim = GetComponent<Animator>();
    }
    private void OnDrawGizmos()
    {
        // 바닥에 혹시 붙을 수 있으므로 약간 y축으로 올린다.
        Vector3 origin = transform.position + Vector3.up * 0.8f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, _viewRadius);

        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -_viewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, _viewAngle / 2, 0) * forward;

        Gizmos.color = Color.red;
        
        Gizmos.DrawLine(origin, origin + leftBoundary * _viewRadius);
        Gizmos.DrawLine(origin, origin + rightBoundary * _viewRadius);
    }

    private void Update()
    {
        
    }
}