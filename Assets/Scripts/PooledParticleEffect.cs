
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(PooledObject))]
public class PooledParticleEffect : MonoBehaviour
{
    private ParticleSystem particleEffect;
    private PooledObject pooledObject;

    private void Awake()
    {
        particleEffect = GetComponent<ParticleSystem>();
        pooledObject = GetComponent<PooledObject>();

        var main = particleEffect.main;

        main.loop = false;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    public void PlayEffect()
    {
        particleEffect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        particleEffect.Play(true);
    }

    private void OnParticleSystemStopped()
    {
        pooledObject.ReturnToPool();
    }
}
