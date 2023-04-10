using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Thrower : Enemy
{
    [SerializeField]
    float maxDistance;

    [SerializeField]
    float fleeDistance;

    [SerializeField]
    float wanderRadius;

    void Start()
    {
        SharedStart();
        altPos = spawnPos;
    }

    void FixedUpdate()
    {
        SharedUpdate();
    }

    public override void DisableCollider()
    {
        GetComponent<CircleCollider2D>().enabled = false;
    }

    public override void Attack(bool alternate = false) { }

    public override void AttackMovingPattern()
    {
        if (
            Physics2D
                .Linecast(
                    transform.position
                        + (Player.instance.transform.position - transform.position).normalized * 3,
                    Player.instance.transform.position
                )
                .collider.gameObject.tag == "Player"
            && Vector3.Distance(transform.position, target) < 3
        )
            target = AIManager.instance.GetPos(pos, out pos, distance);
        else if (Vector3.Distance(transform.position, target) < 1)
            target = RandomNavSphere(transform.position, wanderRadius, -1);
    }

    public override void AltAttack() { }

    Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}
