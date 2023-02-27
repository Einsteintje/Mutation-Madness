using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyScript : MonoBehaviour
{
    GameObject managerObject;
    managerScript manager;
    public GameObject enemyBody;
    GameObject body;
    SpriteRenderer spriteRenderer;
    Color origionalColor;
    float flashTime = 0.05f;

    //pathfinding
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    int pos = -1;
    Vector3 target = new Vector3(0, 0, 0);

    //shooting
    GameObject player;
    playerMovement playerScript;
    public float bulletSpeed;
    public float prediction;
    public float maxCD;
    public float currentCD;
    public GameObject bullet;
    RaycastHit2D hit;
    public float hp;

    private Vector3 knockback = new Vector3();

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerMovement>();
        managerObject = GameObject.FindWithTag("Manager");
        manager = managerObject.GetComponent<managerScript>();

        body = Instantiate(enemyBody);
        spriteRenderer = body.GetComponent<SpriteRenderer>();
        origionalColor = spriteRenderer.color;
    }

    void FixedUpdate()
    {
        if (manager.state != "Clearing" && hp > 0)
        {
            if (Vector3.Distance(target, transform.position) < 3 || target.x * target.y == 0.0f)
            {
                target = AIManager.instance.GetPos(pos, out pos);
            }
            else
            {
                navMeshAgent.SetDestination(target);
            }

            this.Log(target, pos, Vector3.Distance(target, transform.position));

            knockback = new Vector3(
                Mathf.Lerp(knockback.x, 0, 0.2f),
                Mathf.Lerp(knockback.y, 0, 0.2f),
                0
            );
            transform.position += knockback;

            body.transform.position = transform.position;

            Vector3 offset = new Vector3(
                player.transform.position.x - transform.position.x,
                player.transform.position.y - transform.position.y,
                0
            );
            offset = Vector3.ClampMagnitude(offset, 4.0f);
            hit = Physics2D.Linecast(transform.position + offset, player.transform.position);
            if (hit)
            {
                if (!hit.collider.gameObject.tag.In("Box", "Barrel"))
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
                        script.moveSpeed = bulletSpeed;
                        script.size = 1f;
                        script.color = Color.red;
                        currentCD = maxCD;
                    }
                }
                else
                {
                    currentCD = maxCD;
                }
            }
        }
    }

    void Hit(Vector3 kb)
    {
        knockback = kb;
        hp--;
        Flash();
        if (hp <= 0)
        {
            Death();
        }
    }

    void Flash()
    {
        spriteRenderer.color = Color.white;
        Invoke("ResetColor", flashTime);
    }

    void ResetColor()
    {
        spriteRenderer.color = origionalColor;
    }

    void Death()
    {
        hp = 0;
        GetComponentInChildren<ParticleSystem>().Play();
        GetComponent<CircleCollider2D>().enabled = false;
        body.GetComponent<SpriteRenderer>().enabled = false;
        navMeshAgent.enabled = false;

        Destroy(gameObject, 2f);
    }

    float Angle()
    {
        float y = player.transform.position.y - transform.position.y;
        float x = player.transform.position.x - transform.position.x;
        float yPrediction = Mathf.Abs(y) / bulletSpeed * playerScript.movement.y * 50;
        float xPrediction = Mathf.Abs(x) / bulletSpeed * playerScript.movement.x * 50;
        return Mathf.Atan2(y + yPrediction, x + xPrediction) * Mathf.Rad2Deg;
    }
}
