using System.Collections;
using UnityEngine;

public class PooledParticle : MonoBehaviour
{
    private ParticleSystem _particle;
    private Coroutine _returnRoutine;

    private void Awake()
    {
        _particle = GetComponentInChildren<ParticleSystem>(true);
    }

    private void OnEnable()
    {
        if (_particle == null)
            return;

        _particle.Clear(true);
        _particle.Play(true);

        if (_returnRoutine != null)
            StopCoroutine(_returnRoutine);

        _returnRoutine = StartCoroutine(ReturnWhenFinished());
    }

    private IEnumerator ReturnWhenFinished()
    {
        yield return null;

        while (_particle.IsAlive(true))
        {
            yield return null;
        }

        GameManager.Pool.DespawnToPool(gameObject);
    }
}