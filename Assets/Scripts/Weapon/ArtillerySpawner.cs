using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtillerySpawner : MonoBehaviour {

    public GameObject artilleryShell;
    public int repeat = 1;
    public float delay = 3f;
    public float initialDelay = 1f;
    public float delayRandom = 1f;
    public float positionRandom = 5f;

    private float counter = 0;

    // Use this for initialization
    void Start () {
        InvokeRepeating("Projectile", initialDelay, delay + Random.Range(-delayRandom, delayRandom));
    }
    void Projectile()
    {
        if(counter < repeat)
        {
            counter += 1;
            Vector3 directionOfShot = transform.forward;
            directionOfShot.x += Random.Range(-positionRandom, positionRandom);
            directionOfShot.z += Random.Range(-positionRandom, positionRandom);

            Instantiate(artilleryShell, transform.position + directionOfShot, Quaternion.LookRotation(Vector3.up));
        }
    }
}
