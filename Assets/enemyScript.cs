using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyScript : MonoBehaviour
{
    GameObject managerObject;
    managerScript manager;
    public GameObject body;
    SpriteRenderer spriteRenderer;
    Color origionalColor;
    float flashTime = 0.05f;
    private Vector3 knockback = new Vector3();
    private string state = "Idle";
    public ParticleSystem deathPS;
    public ParticleSystem sleepPS;
    public ParticleSystem triggerPS;

    private float timer;
    private float maxTimer = 5.0f;

    //pathfinding
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    int pos = -1;
    Vector3 target;
    Vector3 spawnPos;

    //shooting
    GameObject player;
    playerMovement playerScript;
    public float bulletSpeed;
    public float prediction;
    public float maxCD;
    public float currentCD;
    public GameObject bullet;
    RaycastHit2D hit;
    private bool started = false;

    //health
    public int hp;
    public int maxHP = 5;
    public GameObject healthBar;
    private healthBarScript healthBarScript;
    private int maxHealthTimer = 2;
    private float healthTimer;

    void Start()
    {
        //other game objects
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerMovement>();
        managerObject = GameObject.FindWithTag("Manager");
        manager = managerObject.GetComponent<managerScript>();

        //own components
        body = Instantiate(body);
        healthBar = Instantiate(healthBar);
        healthBarScript = healthBar.GetComponent<healthBarScript>();
        spriteRenderer = body.GetComponent<SpriteRenderer>();
        sleepPS = Instantiate(
            sleepPS,
            transform.position + Vector3.up * 2,
            Quaternion.Euler(-90, 0, 0)
        );
        triggerPS = Instantiate(
            triggerPS,
            transform.position + Vector3.up * 2,
            Quaternion.Euler(-90, 0, 0)
        );

        //own values
        //transform.position = new Vector3(transform.position.x, transform.position.y,0);
        spawnPos = transform.position;
        origionalColor = spriteRenderer.color;
        timer = maxTimer;
        hp = maxHP;
        healthBarScript.canvasGroup.alpha = 0;
        healthBarScript.currentHP = hp;
        target = spawnPos;
    }

    void FixedUpdate()
    {
        if (manager.state != "Clearing" && hp > 0)
        {
            if (state == "Idle")
            {
                if (target != transform.position){
                    this.Log(transform.position, target);
                }
                if (Vector3.Distance(target, transform.position) < 3)
                {
                    timer -= Time.fixedDeltaTime;
                    if (!sleepPS.isPlaying)
                        sleepPS.Play();
                    if (!IsInvoking("AddHealth"))
                        InvokeRepeating("AddHealth", 0.5f, 1.0f);
                }

                if (timer < 0)
                {
                    WakeUp();
                }
            }
            else if (state == "Attack")
            {
                if (
                    Vector3.Distance(target, transform.position) < 3
                    || Vector3.Distance(transform.position, player.transform.position) > 30
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

            navMeshAgent.SetDestination(target);
            knockback = new Vector3(
                Mathf.Lerp(knockback.x, 0, 0.2f),
                Mathf.Lerp(knockback.y, 0, 0.2f),
                0
            );
            if (playerScript.weapon.recoil.x * playerScript.weapon.recoil.y != 0 && !started)
            {
                started = true;
                WakeUp();
            }
            transform.position += knockback;
            body.transform.position = transform.position;
            //health
            healthBar.transform.position = transform.position + Vector3.up * 3;
            if (hp != healthBarScript.currentHP)
            {
                healthBarScript.FadeIn();
                healthBarScript.UpdateSlider(hp, maxHP);
                healthTimer = maxHealthTimer;
            }
            else if (healthTimer > 0)
                healthTimer -= Time.fixedDeltaTime;
            else
                healthBarScript.FadeOut();
        }
    }

    void AddHealth()
    {
        if (hp < maxHP)
            hp++;
    }

    void WakeUp()
    {
        CancelInvoke("AddHealth");
        timer = maxTimer;
        state = "Attack";
        sleepPS.Stop();
        if (!triggerPS.isPlaying)
            triggerPS.Play();
        target = AIManager.instance.GetPos(pos, out pos);
    }

    void Shoot(bool overwrite = false)
    {
        Vector3 offset = new Vector3(
            player.transform.position.x - transform.position.x,
            player.transform.position.y - transform.position.y,
            0
        );
        offset = Vector3.ClampMagnitude(offset, 4.0f);
        hit = Physics2D.Linecast(transform.position + offset, player.transform.position);
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
                    script.moveSpeed = bulletSpeed;
                    script.size = 1f;
                    script.color = Color.red;
                    currentCD = maxCD;
                }
            }
            else
                currentCD = maxCD;
        }
    }

    void Hit(Vector3 kb)
    {
        knockback = kb;
        hp--;
        if (state == "Idle")
            WakeUp();

        if (hp <= 0)
            Death();
        else
            Flash();
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
        CancelInvoke("AddHealth");
        healthBarScript.canvasGroup.alpha = 0;
        hp = 0;
        deathPS.Play();
        sleepPS.Stop();
        GetComponent<CircleCollider2D>().enabled = false;
        navMeshAgent.enabled = false;
        Destroy(body);
        Destroy(healthBar, 2f);
        Destroy(triggerPS, 2f);
        Destroy(sleepPS, 2f);
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
