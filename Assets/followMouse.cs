using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class followMouse : MonoBehaviour {
 
     // Update is called once per frame
     void Update () 
     {
         Vector2 positionOnScreen = Camera.main.WorldToViewportPoint (transform.position);
         Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
         float angle = AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);
         transform.rotation =  Quaternion.Euler (new Vector3(0f,0f,angle));
         
     }
 
     float AngleBetweenTwoPoints(Vector3 a, Vector3 b) {
         return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
     }
 
 }