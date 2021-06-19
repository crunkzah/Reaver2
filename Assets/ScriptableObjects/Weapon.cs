using UnityEngine;

[CreateAssetMenu(fileName = "New weapon", menuName = "Weapons/New weapon")]
public class Weapon : ScriptableObject
{
    
    public EntityType ent_type = EntityType.UNDEFINED_ENTITY;
    public AmmoType ammoType;
    // public float Damage = 0;
    public float FireRate;
    public float reloadingTime = 0.5f;
    public int clipSize;
    public GameObject prefab;
    public SceneNetObject netObjectEnum = SceneNetObject.UNDEFINED;
    public GameObject droppedOnGroundPrefab;
    public GameObject bulletPrefab;
    public AudioClip fireAudioClip;
    public AudioClip emptyMagazineSound;
    public float fireSoundRadius = 16f;
    
    
    public ObjectPoolKey poolKey = ObjectPoolKey.CrossbowDropped;   
}
