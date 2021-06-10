using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnPool : MonoBehaviour
{
    public float existTime;
    public string objPoolTag;

    public enum ObjType
    {   
        ParticleSystem,
        RigidObject,
        Object,
        ManualControl
    }

    public ObjType thisObjType;

    private ParticleSystem thisParticle;

    private bool firstSpawn;

    private void Awake()
    {
        firstSpawn = true;

        if(GetComponent<ParticleSystem>()!=null)
        {
            thisObjType = ObjType.ParticleSystem;
            thisParticle = GetComponent<ParticleSystem>();
        }
    }

    private void OnEnable()
    {
        if(firstSpawn)
        {
            firstSpawn = false;
            return;
        }

        switch (thisObjType)
        {
            case ObjType.ParticleSystem:
                thisParticle.Play();
                StartCoroutine(ReturnPool_Particle());
                break;
            case ObjType.RigidObject:
                PoolManager.instance.ReturnPool(gameObject, objPoolTag);
                break;
            case ObjType.Object:
                StartCoroutine(ReturnPool_Object());
                break;
            default:
                break;
        }
    }

    IEnumerator ReturnPool_Particle()
    {
        yield return new WaitUntil(() => thisParticle.isStopped);
        PoolManager.instance.ReturnPool(gameObject, objPoolTag);
    }

    IEnumerator ReturnPool_Object()
    {
        yield return new WaitForSeconds(existTime);
        PoolManager.instance.ReturnPool(gameObject, objPoolTag);
    }
}
