using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public bool started = false;
    public int maxHP;
    public int hp;
    public float timer;
    public float maxTimer;
    public string state = "Idle";
    public float attackSpeed;

    public ParticleSystem deathPS;
    public ParticleSystem sleepPS;
    public ParticleSystem triggerPS;

    public Vector3 knockback = new Vector3();
    public SpriteRenderer spriteRenderer;
    public Color origionalColor;
    public float flashTime = 0.05f;

    public UnityEngine.AI.NavMeshAgent navMeshAgent;


    public GameObject body;
    public GameObject healthBar;
    public healthBarScript healthBarScript;

    public Vector3 spawnPos;
    public Vector3 target;

    public int pos = -1;

    public int maxHealthTimer = 2;
    public float healthTimer;

    public void SharedUpdate(){
        transform.position += knockback;
        body.transform.position = transform.position;
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

    public void SharedStart(){
        body = Instantiate(body);
        healthBar = Instantiate(healthBar);
        healthBarScript = healthBar.GetComponent<healthBarScript>();
        spriteRenderer = body.GetComponent<SpriteRenderer>();

        origionalColor = spriteRenderer.color;
        timer = maxTimer;
        hp = maxHP;
        healthBarScript.canvasGroup.alpha = 0;
        healthBarScript.currentHP = hp;
        SetPS();
        Invoke("SetSpawnPos", 0.05f);
    }

    public void SetPS(){
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
    }

    public void AddHealth()
    {
        if (hp < maxHP)
            hp++;
    }

    public void WakeUp()
    {
        started = true;
        CancelInvoke("AddHealth");
        timer = maxTimer;
        state = "Attack";
        sleepPS.Stop();
        if (!triggerPS.isPlaying)
            triggerPS.Play();
        target = AIManager.instance.GetPos(pos, out pos);
    }

    public void Hit(Vector3 kb)
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

    public void Flash()
    {
        spriteRenderer.color = Color.white;
        Invoke("ResetColor", flashTime);
    }

    public void ResetColor()
    {
        spriteRenderer.color = origionalColor;
    }

    public void Death()
    {
        CancelInvoke("AddHealth");
        healthBarScript.canvasGroup.alpha = 0;
        hp = 0;
        deathPS.Play();
        sleepPS.Stop();
        DisableCollider();
        navMeshAgent.enabled = false;
        Destroy(body);
        Destroy(healthBar, 2f);
        Destroy(triggerPS, 2f);
        Destroy(sleepPS, 2f);
        Destroy(gameObject, 2f);
    }

    public float Angle(int prediction)
    {
        float y = Player.instance.transform.position.y - transform.position.y;
        float x = Player.instance.transform.position.x - transform.position.x;
        float yPrediction = Mathf.Abs(y) / attackSpeed * Player.instance.movement.y * prediction;
        float xPrediction = Mathf.Abs(x) / attackSpeed * Player.instance.movement.x * prediction;
        this.Log(x,y, yPrediction, xPrediction);
        return Mathf.Atan2(y + yPrediction, x + xPrediction) * Mathf.Rad2Deg;
    }

    public void SetSpawnPos()
    {
        spawnPos = transform.position;
        target = spawnPos;
    }

    public abstract void DisableCollider();

    
}
