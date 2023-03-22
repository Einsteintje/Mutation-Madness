using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public int maxHP,
        maxHealthTimer,
        maxStateTimer,
        maxCD,
        attackSpeed,
        prediction;
    public float flashTime,
        knockbackLerpSpeed;

    public ParticleSystem deathPS,
        sleepPS,
        triggerPS;
    public GameObject body,
        healthBar;
    public UnityEngine.AI.NavMeshAgent navMeshAgent;

    [HideInInspector]
    public string state = "Idle";

    [HideInInspector]
    public bool started = false;

    [HideInInspector]
    public float currentStateTimer,
        currentCD,
        currentHealthTimer;

    [HideInInspector]
    public int currentHP;

    [HideInInspector]
    public Vector3 knockback,
        target = new Vector3();

    [HideInInspector]
    public Vector3 spawnPos,
        altPos;

    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    [HideInInspector]
    public Color origionalColor;

    [HideInInspector]
    public healthBarScript healthBarScript;

    [HideInInspector]
    public int pos = -1;

    public void SharedUpdate()
    {
        if (Manager.instance.state != "Clearing" && currentHP > 0)
        {
            if (state == "Idle")
            {
                if (Vector3.Distance(target, transform.position) < 3)
                {
                    currentStateTimer -= Time.fixedDeltaTime;
                    if (!sleepPS.isPlaying)
                        sleepPS.Play();
                    if (!IsInvoking("AddHealth"))
                        InvokeRepeating("AddHealth", 0.5f, 1.0f);
                }

                if (currentStateTimer < 0)
                {
                    WakeUp();
                }
            }
            else if (state == "Attack")
            {
                Attack();
                AttackMovingPattern();
                if (target.x * target.y == 0)
                {
                    if (currentHP < maxHP)
                        state = "Idle";
                    else
                        state = "AltAttack";
                    target = altPos;
                }
            }
            else if (state == "AltAttack")
            {
                this.Log(target);
                if (Vector3.Distance(target, transform.position) < 3)
                {
                    currentStateTimer -= Time.fixedDeltaTime;
                    AltAttack();
                    if (currentStateTimer < 0)
                        state = "Attack";
                }
            }
            transform.position += knockback;
            body.transform.position = transform.position;
            healthBar.transform.position = transform.position + Vector3.up * 3;
            if (currentHP != healthBarScript.currentHP)
            {
                healthBarScript.FadeIn();
                healthBarScript.UpdateSlider(currentHP, maxHP);
                currentHealthTimer = maxHealthTimer;
            }
            else if (currentHealthTimer > 0)
                currentHealthTimer -= Time.fixedDeltaTime;
            else
                healthBarScript.FadeOut();
            if (
                (
                    Player.instance.weapon.recoil.x * Player.instance.weapon.recoil.y != 0
                    || Vector3.Distance(Player.instance.transform.position, transform.position) < 10
                ) && !started
            )
            {
                WakeUp();
            }
            if (started && target != navMeshAgent.destination && navMeshAgent.enabled)
                navMeshAgent.SetDestination(target);
            knockback = new Vector3(
                Mathf.Lerp(knockback.x, 0, knockbackLerpSpeed),
                Mathf.Lerp(knockback.y, 0, knockbackLerpSpeed),
                0
            );
        }
    }

    public void SharedStart()
    {
        body = Instantiate(body);
        healthBar = Instantiate(healthBar);
        healthBarScript = healthBar.GetComponent<healthBarScript>();
        spriteRenderer = body.GetComponent<SpriteRenderer>();

        origionalColor = spriteRenderer.color;
        currentStateTimer = maxStateTimer;
        currentHP = maxHP;
        healthBarScript.canvasGroup.alpha = 0;
        healthBarScript.currentHP = currentHP;
        SetPS();
        Invoke("SetSpawnPos", 0.05f);
    }

    public void SetPS()
    {
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
        if (currentHP < maxHP)
            currentHP++;
    }

    public void WakeUp()
    {
        started = true;
        CancelInvoke("AddHealth");
        currentStateTimer = maxStateTimer;
        state = "Attack";
        sleepPS.Stop();
        if (!triggerPS.isPlaying)
            triggerPS.Play();
        target = AIManager.instance.GetPos(pos, out pos);
    }

    public void Hit(Vector3 kb)
    {
        knockback = kb;
        currentHP--;
        if (state == "Idle")
            WakeUp();

        if (currentHP <= 0)
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
        currentHP = 0;
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

    public float Angle()
    {
        float y = Player.instance.transform.position.y - transform.position.y;
        float x = Player.instance.transform.position.x - transform.position.x;
        float yPrediction = Mathf.Abs(y) / attackSpeed * Player.instance.movement.y * prediction;
        float xPrediction = Mathf.Abs(x) / attackSpeed * Player.instance.movement.x * prediction;
        return Mathf.Atan2(y + yPrediction, x + xPrediction) * Mathf.Rad2Deg;
    }

    public void SetSpawnPos()
    {
        spawnPos = transform.position;
        target = spawnPos;
    }

    public abstract void DisableCollider();

    public abstract void Attack(bool overwrite = false);

    public abstract void AltAttack();

    public abstract void AttackMovingPattern();
}
