using UnityEngine;

public class Projectile : MonoBehaviour {
    

    public virtual void OnHit()
    {
        Debug.Log("OnHit()");
    }

	public virtual void Tick()
    {

    }
}
