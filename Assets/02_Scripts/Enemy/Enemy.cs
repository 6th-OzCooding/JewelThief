using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator _anim;

    private void Start()
    {
        // 애니메이션을 찾고 그 애니메이션을 담을 변수인 _anim으로 지정
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {

    }
}