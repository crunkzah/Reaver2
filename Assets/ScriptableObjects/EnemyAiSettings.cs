using UnityEngine;

[CreateAssetMenu(fileName = "New enemy", menuName = "New enemy")]
public class EnemyAiSettings : ScriptableObject
{
    [Header("Idle:")]
    public float idleWalkRadius = 10f;
    public float walkSpeed = 3f;
    public float walkAngularSpeed = 360f;
    public float viewRadiusIdle = 9f;
    public float idleChangePositionRate = 4f;


    [Header("Curious")]
    public float curiousSpeed = 5.5f;
    public float curiousAngularSpeed = 1080f;

    [Header("Chasing:")]
    public float runSpeed = 8f;
    public float runAngularSpeed = 1080f;
    public float viewAngle = 70f;
    public float viewRadiusChase = 15f;
    

    [Header("Attacking:")]
    public int damageDealt = 20;
    public float attackRange = 1.5f;
    public float attackRate = 2.5f;

}
