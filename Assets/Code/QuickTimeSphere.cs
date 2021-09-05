using UnityEngine;
using Photon.Pun;

public enum QuickTimeType : byte
{
    NotWorking,
    Default,
}

public class QuickTimeSphere : MonoBehaviour
{
    public int caller_id;
    public QuickTimeType type;
    public float qts_col_radius = 1;
    public int damage;
    public float lifeTimer;
    
    public AudioSource audio_src;
    public ParticleSystem ps;
    public SphereCollider col;
    
    public Transform thisTransform;
    
    void Awake()
    {
        thisTransform = this.transform;
    }
    
    
    public void OnAppearance()
    {
        switch(type)
        {
            case(QuickTimeType.Default):
            {
                audio_src.Play();
                ps.Play();
                //InGameConsole.LogFancy("OnAppearance() " + this.gameObject.name);
                
                break;    
            }
            default:
            {
                break;
            }
        }     
    }
    
    public void OnHit()
    {
        switch(type)
        {
            case(QuickTimeType.Default):
            {
                Vector3 pos = transform.position;
                NetworkObjectsManager.CallGlobalCommand(GlobalCommand.Explode_QTS, RpcTarget.Others, pos);
                
                GameObject obj = ObjectPool.s().Get(ObjectPoolKey.Kaboom1, false);
                float explosionRadius = 6;
                float explosionForce = 40;
                int explosionDamage = damage;
                bool isMine = true;
                
                obj.GetComponent<Kaboom1>().ExplodeDamageHostile(pos, explosionRadius, explosionForce, explosionDamage, isMine, false, 0);
                InGameConsole.LogFancy("OnHit()");
                
                col.enabled = false;
                type = QuickTimeType.NotWorking;
                Destroy(this.gameObject, 2);
                
                break;
            }
            case(QuickTimeType.NotWorking):
            {
                break;
            }
        }
    }
    
    void OnDestroy()
    {
        //InGameConsole.LogFancy("QuickTimeSphere OnDestroy()");
        UberManager.RemoveQTSFromHashSet(caller_id);
    }
    
    public void LifeTimeOut()
    {
        type = QuickTimeType.NotWorking;
        col.enabled = false;
        ps.Stop();
        
        Destroy(this.gameObject, 2);
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        switch(type)
        {
            case(QuickTimeType.Default):
            {
                PlayerController local_pc = PhotonManager.GetLocalPlayer();
                if(local_pc)
                {
                    thisTransform.LookAt(local_pc.GetHeadPosition());
                }
                
                lifeTimer -= dt;
                if(lifeTimer <= 0f)
                {
                    LifeTimeOut();
                }
                break;    
            }
            default:
            {
                break;
            }
        }        
    }
    
}
