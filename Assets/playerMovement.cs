using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    GameObject managerObject;
    managerScript manager;

    public float moveSpeed;
    private weaponScript weapon;
    public Vector3 movement;
    public CircleCollider2D col;
    private Vector3 knockback = new Vector3();
    public SpriteRenderer[] renderers;
    Color origionalColor;
    float flashTime = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        weapon = GetComponentInChildren<weaponScript>();

        managerObject = GameObject.FindWithTag("Manager");
        manager = managerObject.GetComponent<managerScript>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        origionalColor = renderers[0].color;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        knockback = new Vector3(Mathf.Lerp(knockback.x, 0, 0.2f), Mathf.Lerp(knockback.y, 0, 0.2f), 0);
        if (manager.state == "Idle")
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
                -manager.screenSize.x + manager.objectSize.x / 2,
                manager.screenSize.x - manager.objectSize.x / 2
            );
            float y = Mathf.Clamp(
                transform.position.y,
                -manager.screenSize.y + manager.objectSize.y / 2,
                manager.screenSize.y - manager.objectSize.y / 2
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
        }
    }

    void Hit(Vector3 kb) {
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
