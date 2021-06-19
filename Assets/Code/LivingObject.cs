using UnityEngine;
using UnityEngine.AI;

public class LivingObject : MonoBehaviour, IDamageable {

    public int hitPoints;

    public virtual void TakeDamage(int damage, Vector3 force)
    {
        Debug.Log("Took " + damage + " damage");
        hitPoints -= damage;
        if (hitPoints <= 0)
            Die(force);
        else
            this.gameObject.SendMessage("OnTakeDamage", SendMessageOptions.DontRequireReceiver);
    }

    public virtual void Die(Vector3 force)
    {
        Debug.Log(this.gameObject.name + " died!");
        //Destroy(this.gameObject);
        this.SendMessage("OnDeath", force, SendMessageOptions.DontRequireReceiver);
    }

}
