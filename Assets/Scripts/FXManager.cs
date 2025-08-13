using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FXManager : MonoBehaviour
{
    public AudioSource SFXManager;
    
    private float lastIceSoundTime = -1f;
    private float icesoundCooldown = 0.1f;

    [Header("Ice Break FX")]
    public GameObject iceBreakVFX;
    public AudioClip iceBreakSFX;

    [Header("Block Destroy FX")]
    public GameObject blockDestroyVFX;
    public AudioClip blockDestroySFX;
    

    private void Start()
    {
        GameEventSystem.Instance.IceBreakEvent.AddListener(PlayIceBreakFX);
        GameEventSystem.Instance.BlockDestroyedEvent.AddListener(PlayBlockDestroyFX);

    }

    private void OnDisable()
    {
        GameEventSystem.Instance.IceBreakEvent.RemoveListener(PlayIceBreakFX);
        GameEventSystem.Instance.BlockDestroyedEvent.RemoveListener(PlayBlockDestroyFX);

    }

    public void PlayIceBreakFX(Vector3 position)
    {
        GameObject iceVFX = Instantiate(iceBreakVFX, position, Quaternion.identity);
        Destroy(iceVFX, 5f);
        
        if (Time.time - lastIceSoundTime > icesoundCooldown)
        {
            lastIceSoundTime = Time.time;
            float randomPitch = Random.Range(1f, 1.5f);
            SFXManager.pitch = randomPitch;
            SFXManager.PlayOneShot(iceBreakSFX);
        }
        
    }

    public void PlayBlockDestroyFX(Vector3 position, Color _color)
    {
        GameObject vfxInstance  = Instantiate(blockDestroyVFX, position, Quaternion.identity);
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(new GradientColorKey[] {
                new GradientColorKey(_color, 0f),               
                new GradientColorKey(Color.white, 1f)          
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),                  
                new GradientAlphaKey(0f, 1f)                   
            });
        
        ParticleSystem[] allParticles = vfxInstance.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in allParticles)
        {
            var colOverLifetime = ps.colorOverLifetime;
            colOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
            var main = ps.main;
            main.startColor = gradient.Evaluate(0f);
        }

        Destroy(vfxInstance , 2f);
        
        float randomPitch = Random.Range(1f, 1.5f);
        SFXManager.pitch = randomPitch;
        SFXManager.PlayOneShot(blockDestroySFX);
    }

    
}
