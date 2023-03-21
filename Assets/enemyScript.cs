using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyScript : Enemy
{
    public GameObject bullet;
    RaycastHit2D hit;

    void Start()
    {
        SharedStart();
        altPos = spawnPos;
        prediction = 50;
    }

    public override void DisableCollider()
    {
        GetComponent<CircleCollider2D>().enabled = false;
    }

    void FixedUpdate()
    {
        SharedUpdate();
    }

    public override void Attack(bool overwrite = false)
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
            if (!hit.collider.gameObject.tag.In("Box", "Barrel") || overwrite)
            {
                currentCD -= Time.fixedDeltaTime;
                if (currentCD <= 0)
                {
                    GameObject spawned = Instantiate(
                        bullet,
                        transform.position + offset,
                        Quaternion.Euler(new Vector3(0, 0, Angle()))
                    );
                    bulletMovement script = spawned.GetComponent<bulletMovement>();
                    script.moveSpeed = attackSpeed;
                    script.size = 2f;
                    script.color = Color.red;
                    currentCD = maxCD;
                }
            }
            else
                currentCD = maxCD;
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
            target = AIManager.instance.GetPos(pos, out pos);
    }
}
