
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class PooledParticleEffect : MonoBehaviour
{
    private PooledObject pooledObject;
    private ParticleSystem[] particleSystems;

    private Coroutine returnCoroutine;

    private float playbackDuration = 2f;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();

        particleSystems =
            GetComponentsInChildren<ParticleSystem>(true);

        CalculatePlaybackDuration();
    }

    private void OnEnable()
    {
        PlayEffect();
    }

    public void PlayEffect()
    {
        if (particleSystems == null || particleSystems.Length == 0)
            return;

        // Stop an existing return timer.
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        // Restart all particle systems.
        foreach (ParticleSystem particle in particleSystems)
        {
            if (particle == null)
                continue;

            particle.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            particle.Play(true);
        }

        // Start a new return timer.
        returnCoroutine = StartCoroutine(ReturnAfterPlayback());
    }

    private void CalculatePlaybackDuration()
    {
        playbackDuration = 0f;

        foreach (ParticleSystem particle in particleSystems)
        {
            if (particle == null)
                continue;

            ParticleSystem.MainModule main = particle.main;

            float duration =
                main.duration + main.startLifetime.constantMax;

            playbackDuration = Mathf.Max(
                playbackDuration,
                duration
            );
        }

        playbackDuration = Mathf.Max(0.1f, playbackDuration);
    }

    private IEnumerator ReturnAfterPlayback()
    {
        yield return new WaitForSeconds(playbackDuration);

        returnCoroutine = null;

        if (pooledObject != null && !pooledObject.IsInPool)
        {
            pooledObject.ReturnToPool();
        }
    }

    private void OnDisable()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        if (particleSystems == null)
            return;

        foreach (ParticleSystem particle in particleSystems)
        {
            if (particle == null)
                continue;

            particle.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }
}
