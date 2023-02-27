using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turretScript : MonoBehaviour
{
    GameObject managerObject;
    managerScript manager;

    public float maxCD = 0.5f;
    public float currentCD;
    public GameObject player;
    public GameObject bullet;
    public playerMovement playerScript;
    public float prediction;
    public float bulletSpeed;
    public float hp;
    ParticleSystem ps;
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerMovement>();
        ps = GetComponent<ParticleSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        managerObject = GameObject.FindWithTag("Manager");
        manager = managerObject.GetComponent<managerScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (manager.state != "Clearing" && hp > 0)
        {
            Vector3 offset = new Vector3(
                player.transform.position.x - transform.position.x,
                player.transform.position.y - transform.position.y,
                0
            );
            offset = Vector3.ClampMagnitude(offset, 4.0f);
            RaycastHit2D hit = Physics2D.Linecast(
                transform.position + offset,
                player.transform.position
            );
            if (hit)
            {
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
            }
            else
            {
                currentCD = maxCD;
            }
        }
    }

    float Angle()
    {
        float y = player.transform.position.y - transform.position.y;
        float x = player.transform.position.x - transform.position.x;
        float yPrediction = Mathf.Abs(y) / bulletSpeed * playerScript.movement.y * 50;
        float xPrediction = Mathf.Abs(x) / bulletSpeed * playerScript.movement.x * 50;
        return Mathf.Atan2(y + yPrediction, x + xPrediction) * Mathf.Rad2Deg;
    }

    void Hit()
    {
        hp--;
        if (hp > 0)
            ps.Play();
        else
        {
            Death();
        }
    }

    void Death()
    {
        ps.Play(); //gonna be a different one later
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject, 1f);
    }
}
