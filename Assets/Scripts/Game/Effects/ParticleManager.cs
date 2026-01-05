using UnityEngine;
using UnityEngine.VFX;
using Void2610.UnityTemplate;

public class ParticleManager : SingletonMonoBehaviour<ParticleManager>
{
    [SerializeField] private VisualEffect magicEffect;
    [SerializeField] private VisualEffect magicPathEffect;
    [SerializeField] private VisualEffect clickEffect;

    public void PlaySpawnEffect(Vector3 pos)
    {
        var attr = magicEffect.CreateVFXEventAttribute();
        attr.SetVector3("position", pos);
        magicEffect.SendEvent("OnPlay", attr);
    }

    public void PlaySpawnEffectWithPath(Vector3 startPos, Vector3 endPos)
    {
        var attr = magicPathEffect.CreateVFXEventAttribute();
        attr.SetVector3("FromPos", startPos);
        attr.SetVector3("ToPos", endPos);
        magicPathEffect.SendEvent("OnPlay", attr);
    }

    public void PlayClickEffect(Vector3 pos)
    {
        var attr = clickEffect.CreateVFXEventAttribute();
        attr.SetVector3("position", pos);
        clickEffect.SendEvent("OnPlay", attr);
    }

    protected override void Awake()
    {
        IsDontDestroyOnLoad = false;
        base.Awake();
    }
}
