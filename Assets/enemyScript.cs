using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyScript : Enemy
{
    //shooting
    public float maxCD;
    public float currentCD;
    public GameObject bullet;
    RaycastHit2D hit;

    void Start()
    {
        SharedStart();
        prediction = 50;
    }

    public override void DisableCollider(){
        GetComponent<CircleCollider2D>().enabled = false;
    }
    void FixedUpdate()
    {
        if (Manager.instance.state != "Clearing" && hp > 0)
        {
            if (state == "Idle")
            {
                SharedIdle();
            }
            else if (state == "Attack")
            {
                if (
                    Vector3.Distance(target, transform.position) < 3
                    || Vector3.Distance(transform.position, Player.instance.transform.position) > 30
                )
                    target = AIManager.instance.GetPos(pos, out pos);

                if (target.x * target.y == 0)
                {
                    if (hp < maxHP)
                        state = "Idle";
                    else
                        state = "AltAttack";
                    target = spawnPos;
                }

                Shoot();
            }
            else if (state == "AltAttack")
            {
                if (Vector3.Distance(target, transform.position) < 3)
                {
                    timer -= Time.fixedDeltaTime;
                    Shoot(true);
                    if (timer < 0)
                        state = "Attack";
                }
            }
            
            SharedUpdate();
        }
    }

    void Shoot(bool overwrite = false)
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
                    script.moveSpeed =attackSpeed;
                    script.size = 1f;
                    script.color = Color.red;
                    currentCD = maxCD;
                }
            }
            else
                currentCD = maxCD;
        }
    }
}
