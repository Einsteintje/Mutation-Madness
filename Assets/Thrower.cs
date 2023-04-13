using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Thrower : Enemy
{
    [SerializeField]
    GameObject shell;

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

    public override void Attack(bool alternate = false)
    {
        cD -= Time.fixedDeltaTime;
        if (cD < 0)
        {
            cD = maxCD;
            GameObject spawn = Instantiate(shell);
            Shell script = spawn.GetComponent<Shell>();
            script.transform.position = transform.position;
            script.destination = Player.instance.transform.position;
            script.color = color;
            script.mutation = mutation;
        }
    }

    public override void AttackMovingPattern()
    {
        RaycastHit2D hit = Physics2D.Linecast(
            transform.position
                + (Player.instance.transform.position - transform.position).normalized * 3,
            Player.instance.transform.position
        );
        if (hit)
        {
            if (
                hit.collider.gameObject.tag == "Player"
                && Vector3.Distance(transform.position, target) < 3
            )
                target = AIManager.instance.GetPos(pos, out pos, distance);
        }
        else if (Vector3.Distance(transform.position, target) < 1)
            target = RandomNavSphere(transform.position, wanderRadius, -1);
    }

    public override void AltAttack()
    {
        Attack();
    }

    Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}
