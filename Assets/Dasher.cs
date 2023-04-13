using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Dasher : Enemy
{
    bool dashing = false;
    float dashTimer;

    [SerializeField]
    float maxDashTimer;
    TrailRenderer trailRenderer;

    // Start is called before the first frame update
    void Start()
    {
        SharedStart();
        dashTimer = maxDashTimer;
        trailRenderer = body.GetComponent<TrailRenderer>();
    }

    public override void DisableCollider()
    {
        GetComponent<CircleCollider2D>().enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        altPos = Player.instance.transform.position;
        SharedUpdate();
    }

    public override void Attack(bool alternate = false)
    {
        if (!dashing)
        {
            body.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Angle() - 90));
        }
        if (cD > 0)
        {
            cD -= Time.fixedDeltaTime;
        }
        if (Vector3.Distance(transform.position, target) < 3 && cD <= 0)
        {
            cD = maxCD;
            dashing = true;
            navMeshAgent.enabled = false;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, Angle() - 90));
            trailRenderer.emitting = true;
        }
        if (dashing)
        {
            transform.position += transform.up * Time.fixedDeltaTime * attackSpeed;
            dashTimer -= Time.fixedDeltaTime;
        }
        if (dashTimer <= 0)
        {
            trailRenderer.emitting = false;
            dashTimer = maxDashTimer;
            navMeshAgent.enabled = true;
            dashing = false;
            target = AIManager.instance.GetPos(pos, out pos, distance);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (dashing && other.gameObject.tag != "Bullet")
        {
            other.gameObject.SendMessage("Hit", (transform.up * 3, mutation));
            if (other.gameObject.tag == "Player")
                dashTimer = 0;
        }
    }

    public override void AltAttack() { }

    public override void AttackMovingPattern()
    {
        if (
            !dashing
            && (
                Vector3.Distance(target, transform.position) < 3
                || Vector3.Distance(transform.position, Player.instance.transform.position) > 30
            )
        )
        {
            target = AIManager.instance.GetPos(pos, out pos, distance);
        }
    }
}
