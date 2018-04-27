using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShoot : MonoBehaviour {
    public float damge = 10f;
    public float range = 5000f;

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
    }

	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Fire1"))
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
            Debug.Log(hit.transform.name);
            if(hit.transform.root.name.Contains("German"))
            {
                GameObject bloodhit = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }
}
