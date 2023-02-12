using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletMovement : MonoBehaviour
{
    GameObject managerObject;
    managerScript manager;

    public float moveSpeed;
    public float size;
    public Color color;
    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
    }

    void Start()
    {
        ParticleSystem.MainModule main = ps.main;
        main.startColor = color;
        GetComponent<SpriteRenderer>().color = color;
        GetComponent<TrailRenderer>().startColor = color;
        GetComponent<TrailRenderer>().endColor = color;

        transform.up = Quaternion.Euler(0, 0, -90) * transform.up;
        transform.localScale = new Vector3(size, size, 0);

        managerObject = GameObject.FindWithTag("Manager");
        manager = managerObject.GetComponent<managerScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += transform.up * Time.fixedDeltaTime * moveSpeed;
        if (manager.state == "Clearing" && ps != null && moveSpeed != 0)
            Death();
        if (
            transform.position.y < -manager.screenSize.y
            || transform.position.y > manager.screenSize.y
            || transform.position.x < -manager.screenSize.x
            || transform.position.x > manager.screenSize.x
        )
            Destroy(gameObject, 1f);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (color == Color.cyan || !other.gameObject.tag.In("Enemy", "Turret"))
        {
            if (other.gameObject.tag != "Bullet" && moveSpeed != 0)
                other.gameObject.SendMessage("Hit");
            Death();
        }
    }

    void Death()
    {
        moveSpeed = 0;
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<TrailRenderer>().enabled = false;
        ps.Play();
        Destroy(gameObject, 2f);
    }
}
