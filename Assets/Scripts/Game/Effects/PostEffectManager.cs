using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using LitMotion;
using Cysharp.Threading.Tasks;
using Void2610.UnityTemplate;

[RequireComponent(typeof(Volume))]
public class PostEffectManager : SingletonMonoBehaviour<PostEffectManager>
{
    [SerializeField] private Volume volume;
    
    public void SetEffect<T>(Action<T> modifier) where T : VolumeComponent
    {
        volume.profile.TryGet<T>(out var component);
        modifier?.Invoke(component);
    }
}
