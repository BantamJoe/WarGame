﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShoot : MonoBehaviour {
    public float damge = 10f;
    public float range = 5000f;
    public float fireDelay = 0.1f;

    public Transform crosshair;
    public AudioClip fire;
    public GameObject firespot;
    public GameObject impactEffect;
    public GameObject bloodEffect;
    public ParticleSystem muzzleFlash;

    private AudioSource weaponAudio;
    void Start()
    {
        weaponAudio = this.gameObject.AddComponent<AudioSource>();
        weaponAudio.clip = fire;
        bloodEffect.GetComponent<ParticleSystem>().loop = false;
    }

	// Update is called once per frame
	void Update () {

        //Update the 3D crosshair by casting a point and updating the draw position. Ray is casted from gun's muzzle
        RaycastHit cast;
        Physics.Raycast(firespot.transform.position, firespot.transform.forward, out cast, range);
        crosshair.transform.position = cast.point;

        //Fire the gun
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
	}
    void Shoot()
    {
        RaycastHit hit;
        muzzleFlash.Play();
        weaponAudio.Play();

        //Did we hit something?
        if(Physics.Raycast(firespot.transform.position, firespot.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.root.name);
            if(hit.transform.root.name.Contains("German"))
            {
                GameObject bloodhit = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                bloodhit.transform.parent = hit.transform.root.gameObject.transform;
                hit.transform.root.GetComponent<BasicDeath>().Die();
            }
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }
}
