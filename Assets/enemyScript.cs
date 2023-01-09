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

    //pathfinding
    private Transform target;
    public UnityEngine.AI.NavMeshAgent navMeshAgent;

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
    private float stopDistance;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerMovement>();
        target = player.transform;
        managerObject = GameObject.FindWithTag("Manager");
        manager = managerObject.GetComponent<managerScript>();
        stopDistance = 30;
        navMeshAgent.SetDestination(target.position);

        body = Instantiate(enemyBody);
        spriteRenderer = body.GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (manager.state != "Clearing" && hp > 0)
        {
            body.transform.position = transform.position;

            navMeshAgent.SetDestination(target.position);

            Vector3 offset = new Vector3(
                player.transform.position.x - transform.position.x,
                player.transform.position.y - transform.position.y,
                0
            );
            offset = Vector3.ClampMagnitude(offset, 4.0f);
            hit = Physics2D.Linecast(transform.position + offset, player.transform.position);
            if (hit != null)
            {
                if (
                    hit.collider.gameObject.tag != "Player"
                    && hit.collider.gameObject.tag != "Bullet"
                )
                    navMeshAgent.stoppingDistance = 5;
                else
                    navMeshAgent.stoppingDistance = stopDistance;

                if (hit.collider.gameObject.tag.In("Player", "Bullet"))
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

    void Hit()
    {
        hp--;
        if (hp <= 0)
        {
            Death();
        }
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
