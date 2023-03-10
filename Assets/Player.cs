using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }
    public float moveSpeed;
    public weaponScript weapon;
    public Vector3 movement;
    public CircleCollider2D col;
    private Vector3 knockback = new Vector3();
    public SpriteRenderer[] renderers;
    Color origionalColor;
    float flashTime = 0.05f;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        weapon = GetComponentInChildren<weaponScript>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        origionalColor = renderers[0].color;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        knockback = new Vector3(
            Mathf.Lerp(knockback.x, 0, 0.3f),
            Mathf.Lerp(knockback.y, 0, 0.3f),
            0
        );
        if (Manager.instance.state == "Idle")
        {
            col.enabled = true;
            weapon.enabled = true;
            //movement
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
            input = Vector3.ClampMagnitude(input, 1);
            input -= weapon.recoil;
            movement = input * moveSpeed * Time.fixedDeltaTime + knockback;
            transform.Translate(movement);
            //stay inside the screen
            float x = Mathf.Clamp(
                transform.position.x,
                -Manager.instance.screenSize.x + Manager.instance.objectSize.x / 2,
                Manager.instance.screenSize.x - Manager.instance.objectSize.x / 2
            );
            float y = Mathf.Clamp(
                transform.position.y,
                -Manager.instance.screenSize.y + Manager.instance.objectSize.y / 2,
                Manager.instance.screenSize.y - Manager.instance.objectSize.y / 2
            );
            if (x != transform.position.x || y != transform.position.y)
                transform.position = new Vector3(x, y, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(
                Mathf.Lerp(transform.position.x, 0, 0.04f),
                Mathf.Lerp(transform.position.y, 0, 0.04f),
                transform.position.z
            );
            col.enabled = false;
            weapon.enabled = false;
            weapon.recoil = new Vector3(0, 0, 0);
        }
    }

    void Hit(Vector3 kb)
    {
        knockback = kb;
        Flash();
    }

    void Flash()
    {
        foreach (SpriteRenderer renderer in renderers)
            renderer.color = Color.white;
        Invoke("ResetColor", flashTime);
    }

    void ResetColor()
    {
        foreach (SpriteRenderer renderer in renderers)
            renderer.color = origionalColor;
    }
}
