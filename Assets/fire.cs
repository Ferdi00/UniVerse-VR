using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fire : MonoBehaviour
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform sp;
    [SerializeField] private float speed = 15f;
    public void fireBullet()
    {
       GameObject spawnBullet = Instantiate(bullet, sp.position, sp.rotation);
        spawnBullet.GetComponent<Rigidbody>().velocity = sp.forward * speed;
        Destroy(spawnBullet, 10f);

    }
}
