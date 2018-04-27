using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShootAI : MonoBehaviour {

    public float damge = 10f;
    public float range = 5000f;
    
    public AudioClip fire;
    public GameObject firespot;
    public GameObject impactEffect;
    public GameObject bloodEffect;
    public GameObject muzzleFlash;
    public string enemyString;

    private AudioSource weaponAudio;
    private Animator anim;
    private float fireAnimDelay = 1f;
    private float timeToNextFire = 0f;

    void Start()
    {
        weaponAudio = this.gameObject.AddComponent<AudioSource>();
        weaponAudio.clip = fire;
        bloodEffect.GetComponent<ParticleSystem>().loop = false;
        anim = transform.root.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit cast;
        //Fire the gun, and allow fire only after slight delay + animator is not bolting the gun on attack layer
        if (Time.time >= timeToNextFire && Physics.Raycast(firespot.transform.position, firespot.transform.forward, out cast) && cast.transform.root.name.Contains(enemyString))
        {
            //Calculate minimal delay and disable crosshair
            timeToNextFire = Time.time + 1f / fireAnimDelay;

            //Fire the raycast
            Shoot();
        }
    }
    void Shoot()
    {
        RaycastHit hit;
        Instantiate(muzzleFlash, firespot.transform.position, Quaternion.Euler(firespot.transform.forward));
        weaponAudio.Play();


        //Did we hit something?
        if (Physics.Raycast(firespot.transform.position, firespot.transform.forward, out hit, range))
        {
            if (hit.transform.root.name.Contains(enemyString))
            {
                GameObject bloodhit = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                bloodhit.transform.parent = hit.transform.root.gameObject.transform;

                if(hit.transform.root.GetComponent<BasicDeathAI>() != null)
                {
                    Debug.Log("Hitting AI Death");
                    hit.transform.root.GetComponent<BasicDeathAI>().Die();
                }
                else
                {
                    hit.transform.root.GetComponent<BasicDeath>().Die();
                }
            }
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }

        //Animation
        anim.SetTrigger("IsFiringBolt");
    }
}
