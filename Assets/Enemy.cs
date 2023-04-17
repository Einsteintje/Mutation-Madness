using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    public int maxHP,
        maxHealthTimer,
        maxStateTimer,
        maxPathFixTimer,
        maxCD,
        attackSpeed,
        prediction;
    public float flashTime,
        knockbackLerpSpeed;
    public string distance;

    public ParticleSystem deathPS,
        deathPS2,
        sleepPS,
        triggerPS,
        hitPS;
    public GameObject body,
        healthBar;
    public UnityEngine.AI.NavMeshAgent navMeshAgent;

    [HideInInspector]
    public string state = "Idle";

    [HideInInspector]
    public bool started = false;

    [HideInInspector]
    public float stateTimer,
        cD,
        healthTimer,
        pathFixTimer;

    [HideInInspector]
    public int hP,
        falsePathCounter;

    [HideInInspector]
    public Vector3 knockback,
        target = new Vector3();

    [HideInInspector]
    public Vector3 spawnPos,
        altPos;

    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    [HideInInspector]
    public Color color;

    [HideInInspector]
    public healthBarScript healthBarScript;

    [HideInInspector]
    public int pos = -1;

    [HideInInspector]
    public string mutation;

    [HideInInspector]
    public float fireTimer = 0f;

    [HideInInspector]
    public float charged = 1f;

    Dictionary<string, ParticleSystem> mutationPS = new Dictionary<string, ParticleSystem>();

    [HideInInspector]
    public Dictionary<string, float> mutationEffects = new Dictionary<string, float>
    {
        { "Ice", 0.0f },
        { "Fire", 0.0f },
        { "Electric", 0.0f }
    };

    public void SharedUpdate()
    {
        if (Manager.instance.state != "Clearing" && hP > 0)
        {
            if (state == "Idle")
            {
                if (Vector3.Distance(target, transform.position) < 3)
                {
                    if (started)
                        stateTimer -= Time.fixedDeltaTime;
                    if (!sleepPS.isPlaying)
                        sleepPS.Play();
                    if (!IsInvoking("AddHealth"))
                        InvokeRepeating("AddHealth", 0.5f, 1.0f);
                }
                if (stateTimer < 0)
                    WakeUp();
            }
            else if (state == "Attack")
            {
                Attack();
                AttackMovingPattern();
                if (target.x * target.y == 0)
                {
                    if (hP < maxHP)
                        state = "Idle";
                    else
                        state = "AltAttack";
                    target = altPos;
                }
            }
            else if (state == "AltAttack")
                if (Vector3.Distance(target, transform.position) < 3)
                {
                    stateTimer -= Time.fixedDeltaTime;
                    AltAttack();
                    if (stateTimer < 0)
                    {
                        stateTimer = maxStateTimer + 1;
                        state = "Attack";
                    }
                }
            transform.position += knockback;
            body.transform.position = transform.position;
            healthBar.transform.position = transform.position + Vector3.up * 3;
            if (hP != healthBarScript.hP)
            {
                healthBarScript.FadeIn();
                healthBarScript.UpdateSlider(hP, maxHP);
                healthTimer = maxHealthTimer;
            }
            else if (healthTimer > 0)
                healthTimer -= Time.fixedDeltaTime;
            else
                healthBarScript.FadeOut();
            if (
                (
                    (
                        Player.instance.weapon.recoil.x * Player.instance.weapon.recoil.y != 0
                        && Vector3.Distance(Player.instance.transform.position, transform.position)
                            < 30
                    )
                    || Vector3.Distance(Player.instance.transform.position, transform.position) < 10
                ) && !started
            )
                WakeUp();
            if (started && target != navMeshAgent.destination && navMeshAgent.enabled)
                navMeshAgent.SetDestination(target);
            //fixing path for closed off areas
            if (navMeshAgent.pathPending)
                pathFixTimer -= Time.fixedDeltaTime;
            else
                pathFixTimer = maxPathFixTimer;
            if (pathFixTimer <= 0)
            {
                falsePathCounter++;
                pathFixTimer = maxPathFixTimer;
                target = AIManager.instance.GetPos(pos, out pos, distance);
            }
            if (falsePathCounter > 1)
            {
                state = "Altattack";
            }

            knockback = new Vector3(
                Mathf.Lerp(knockback.x, 0, knockbackLerpSpeed),
                Mathf.Lerp(knockback.y, 0, knockbackLerpSpeed),
                0
            );
            MutationHandler();
            if (hP <= 0)
                Death();
        }
    }

    public void SharedStart()
    {
        NavMeshHit hit;
        while (!NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            transform.position = new Vector3(
                Random.Range(-Manager.instance.screenSize.x, Manager.instance.screenSize.x),
                Random.Range(-Manager.instance.screenSize.y, Manager.instance.screenSize.y),
                0
            );
        }
        body = Instantiate(body);
        healthBar = Instantiate(healthBar);
        healthBarScript = healthBar.GetComponent<healthBarScript>();
        spriteRenderer = body.GetComponent<SpriteRenderer>();

        mutation = MutationManager.instance.mutations.Keys.ElementAt(
            Random.Range(0, MutationManager.instance.mutations.Keys.Count)
        );
        color = MutationManager.instance.mutations[mutation].color;
        spriteRenderer.color = color;

        stateTimer = maxStateTimer;
        hP = maxHP;
        healthBarScript.canvasGroup.alpha = 0;
        healthBarScript.hP = hP;
        SetPS();
        Invoke("SetSpawnPos", 0.05f);
    }

    public void SetPS()
    {
        sleepPS = Instantiate(
            sleepPS,
            transform.position + Vector3.up * 2,
            Quaternion.Euler(-90, 0, 0),
            transform
        );
        triggerPS = Instantiate(
            triggerPS,
            transform.position + Vector3.up * 2,
            Quaternion.Euler(-90, 0, 0),
            transform
        );
    }

    public void AddHealth()
    {
        if (hP < maxHP)
            hP++;
    }

    public void WakeUp()
    {
        started = true;
        CancelInvoke("AddHealth");
        stateTimer = maxStateTimer;
        state = "Attack";
        sleepPS.Stop();
        if (!triggerPS.isPlaying)
            triggerPS.Play();
        target = AIManager.instance.GetPos(pos, out pos, distance);
    }

    public void Hit((Vector3, string) tuple)
    {
        mutationEffects[tuple.Item2] = 1.0f;
        AudioManager.instance.hitSound.Play();
        knockback = tuple.Item1;
        hP--;
        HitPS(tuple.Item1);
        if (state == "Idle")
            WakeUp();

        if (hP <= 0)
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
        spriteRenderer.color = color;
    }

    public void Death()
    {
        if (Manager.instance.state == "Idle")
            Player.instance.score += 100;
        ScreenShake.instance.Shake();
        CancelInvoke("AddHealth");
        CancelInvoke("ResetColor");
        healthBarScript.canvasGroup.alpha = 0;
        hP = 0;
        deathPS = Instantiate(deathPS, Manager.instance.transform, transform);
        SetPSColor(deathPS);
        deathPS.transform.position = transform.position;
        deathPS.Play();
        deathPS2.Play();
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
        float y =
            Player.instance.transform.position.y
            - transform.position.y
            + Mathf.Pow(charged, 10) * Random.Range(-1f, 1f);
        ;
        float x =
            Player.instance.transform.position.x
            - transform.position.x
            + Mathf.Pow(charged, 10) * Random.Range(-1f, 1f);
        float yPrediction = Mathf.Abs(y) / attackSpeed * Player.instance.movement.y * prediction;
        float xPrediction = Mathf.Abs(x) / attackSpeed * Player.instance.movement.x * prediction;
        return Mathf.Atan2(y + yPrediction, x + xPrediction) * Mathf.Rad2Deg;
    }

    public void HitPS(Vector3 kb)
    {
        ParticleSystem ps = Instantiate(hitPS, Manager.instance.transform);
        SetPSColor(ps);
        ps.transform.rotation = Quaternion.Euler(kb);
        ps.transform.position = transform.position;
        ps.Play();
    }

    public void SetSpawnPos()
    {
        spawnPos = transform.position;
        target = spawnPos;
    }

    public void SetPSColor(ParticleSystem ps)
    {
        var subEmitter = ps.subEmitters.GetSubEmitterSystem(0);
        var main = ps.main;
        main.startColor = color;
        main = subEmitter.main;
        main.startColor = color;
    }

    void MutationHandler()
    {
        foreach (string mutation in MutationManager.instance.mutations.Keys)
        {
            if (mutationEffects[mutation] > 0)
            {
                if (!mutationPS.Keys.Contains(mutation))
                    mutationPS[mutation] = Instantiate(
                        MutationManager.instance.mutations[mutation].ps,
                        transform
                    );
                if (!mutationPS[mutation].isPlaying)
                    mutationPS[mutation].Play();
                MutationManager.instance.mutations[mutation].action(this.gameObject);
            }
            else if (mutationPS.Keys.Contains(mutation))
            {
                if (mutationPS[mutation].isPlaying)
                    mutationPS[mutation].Stop();
                if (mutation == "Ice")
                    navMeshAgent.acceleration = 50;
                else if (mutation == "Fire")
                {
                    navMeshAgent.speed = 30;
                    fireTimer = 0f;
                }
                else
                    charged = 1.0f;
            }
        }
    }

    public abstract void DisableCollider();

    public abstract void Attack(bool alternate = false);

    public abstract void AltAttack();

    public abstract void AttackMovingPattern();
}
