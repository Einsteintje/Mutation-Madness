using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponScript : MonoBehaviour
{
    public GameObject bullet;
    public float maxCD = 0.2f;
    public float currentCD = 0;
    public float ammo = 50;
    public float kickback = 1.5f;
    public Vector3 recoil = new Vector3(0, 0, 0);
    public ParticleSystem muzzleFlash;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetMouseButton(0) && currentCD <= 0 && ammo > 0)
        {
            Vector3 spawnPos = transform.position + Quaternion.Euler(0, 0, -90) * transform.up * 4f;
            muzzleFlash.transform.position = spawnPos;
            muzzleFlash.Play();

            GameObject spawned = Instantiate(bullet, spawnPos, transform.rotation);
            BulletStats(spawned);
            currentCD = maxCD;
            ammo--;
            recoil = Quaternion.Euler(0, 0, -90) * transform.up * kickback;
        }
        else
        {
            recoil = new Vector3(Mathf.Lerp(recoil.x, 0, 0.1f), Mathf.Lerp(recoil.y, 0, 0.1f), 0);
            if (Mathf.Abs(recoil.x) < 0.01f && Mathf.Abs(recoil.y) < 0.01f)
            {
                recoil = new Vector3(0, 0, 0);
            }
        }
        if (currentCD > 0)
        {
            currentCD -= Time.fixedDeltaTime;
        }
    }

    void BulletStats(GameObject spawned)
    {
        bulletMovement script = spawned.GetComponent<bulletMovement>();
        script.moveSpeed = 50;
        script.size = 1f;
        script.color = Color.cyan;
    }
}
