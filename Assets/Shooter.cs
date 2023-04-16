using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : Enemy
{
    public GameObject bullet;
    RaycastHit2D hit;

    void Start()
    {
        SharedStart();
        altPos = spawnPos;
    }

    public override void DisableCollider()
    {
        GetComponent<CircleCollider2D>().enabled = false;
    }

    void FixedUpdate()
    {
        SharedUpdate();
    }

    public override void Attack(bool alternate = false)
    {
        Vector3 offset = new Vector3(
            Player.instance.transform.position.x - transform.position.x,
            Player.instance.transform.position.y - transform.position.y,
            0
        );
        offset = Vector3.ClampMagnitude(offset, 4.0f);
        hit = Physics2D.Linecast(transform.position + offset, Player.instance.transform.position);
        if (hit)
        {
            if (!hit.collider.gameObject.tag.In("Box", "Barrel") || alternate)
            {
                cD -= Time.fixedDeltaTime * charged;
                if (cD <= 0)
                {
                    GameObject spawned = Instantiate(
                        bullet,
                        transform.position + offset,
                        Quaternion.Euler(new Vector3(0, 0, Angle()))
                    );
                    Bullet script = spawned.GetComponent<Bullet>();
                    script.moveSpeed = attackSpeed;
                    script.size = 2f;
                    script.color = color;
                    script.mutation = mutation;
                    cD = maxCD;
                }
            }
            else
                cD = maxCD;
        }
    }

    public override void AltAttack()
    {
        Attack(true);
    }

    public override void AttackMovingPattern()
    {
        if (
            Vector3.Distance(target, transform.position) < 3
            || Vector3.Distance(transform.position, Player.instance.transform.position) > 30
        )
            target = AIManager.instance.GetPos(pos, out pos, distance);
    }
}
