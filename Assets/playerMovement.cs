using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public float moveSpeed;
    private Vector3 screenSize;
    private Vector3 objectSize;

    // Start is called before the first frame update
    void Start()
    {
        objectSize = GetComponentInChildren<SpriteRenderer>().bounds.size/2;
        screenSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height,0));
    }

    // Update is called once per frame
         void FixedUpdate (){
        //movement
         Vector3 input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
         input = Vector3.ClampMagnitude(input, 1);
         transform.Translate(input * moveSpeed * Time.fixedDeltaTime);
        //stay inside the screen
         float x = Mathf.Clamp(transform.position.x, -screenSize.x + objectSize.x, screenSize.x - objectSize.x);
         float y = Mathf.Clamp(transform.position.y, -screenSize.y + objectSize.y, screenSize.y - objectSize.y);
         transform.position = new Vector3(x, y, transform.position.z);
    }
}