using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ImpactEffectSpawner : MonoBehaviour
{
    private ObjectPool<GameObject> impactEffectPool;
    [SerializeField] private GameObject impactEffectPrefab;

    // Start is called before the first frame update
    void Start()
    {
        impactEffectPool = new ObjectPool<GameObject>(
            CreateImpactEffect,
            OnTakeFromPool,
            OnReturnToPool,
            OnDestroyEffect,
            true,
            10,
            10
            );
    }

    private GameObject CreateImpactEffect()
    {
        GameObject impactEffect = Instantiate(impactEffectPrefab);
        impactEffect.SetActive(false);
        return impactEffect;
    }

    private void OnTakeFromPool(GameObject effect)
    {
        effect.SetActive(true);
    }

    public void SpawnImpactEffect(Vector3 pos, Vector3 normal)
    {
        GameObject effect = impactEffectPool.Get();
        effect.transform.position = pos;
        effect.transform.rotation = Quaternion.LookRotation(normal);

        StartCoroutine(DestroyImpactEffectAfterDelay(effect, 3));
    }

    private IEnumerator DestroyImpactEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(effect); // Destroy the effect after the delay
    }

    private void OnReturnToPool(GameObject effect)
    {
        effect.SetActive(false);
    }

    private void OnDestroyEffect(GameObject effect)
    {
        Destroy(effect);
    }
}
