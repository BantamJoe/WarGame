using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasicShoot : MonoBehaviour
{
    public float damage = 10f;
    public float range = 5000f;

    public Transform crosshair;

    public AudioClip fire;
    public GameObject firespot;
    public GameObject muzzlespot;
    public GameObject impactEffect;
    public GameObject bloodEffect;
    public GameObject muzzleFlash;

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

    public void Shoot()
    {
        //If next time to fire has been reached and animator is reset
        if (Time.time >= timeToNextFire && anim.GetCurrentAnimatorStateInfo(1).IsName("none"))
        {
            RaycastHit hit;
            Instantiate(muzzleFlash, muzzlespot.transform.position, Quaternion.Euler(muzzlespot.transform.forward));
            weaponAudio.Play();


            //Did we hit something?
            if (Physics.Raycast(firespot.transform.position, firespot.transform.forward, out hit, range))
            {
                Debug.Log(hit.transform.root.name);
                if (hit.transform.root.name.Contains("German"))
                {
                    GameObject bloodhit = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    bloodhit.transform.parent = hit.transform.root.gameObject.transform;
                    hit.transform.root.GetComponent<BasicDeathAI>().Die();
                }
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
            timeToNextFire = Time.time + 1f / fireAnimDelay;
            //Animation
            anim.SetTrigger("IsFiringBolt");
        }
    }
}
