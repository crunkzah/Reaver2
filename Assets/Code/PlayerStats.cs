using UnityEngine;

public class PlayerStats : LivingObject {

    public int initHitPoints = 100;

    public System.Action RedrawUIAction;

    public override void TakeDamage(int damage, Vector3 force)
    {
        
        base.TakeDamage(damage, Vector3.zero);
        if(RedrawUIAction != null)
            RedrawUIAction();
    }

    private void Start()
    {
        hitPoints = initHitPoints;
        if(RedrawUIAction != null)
            RedrawUIAction();
    }

    public override void Die(Vector3 force)
    {
        base.Die(force);
    }

}
