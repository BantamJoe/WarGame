using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShoot : MonoBehaviour {
    public float damge = 10f;
    public float range = 5000f;

    public Transform crosshair;
    public AudioClip fire;
    public GameObject firespot;
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

	// Update is called once per frame
	void Update () {

        //Update the 3D crosshair by casting a point and updating the draw position. Ray is casted from gun's muzzle
        RaycastHit cast;
        Physics.Raycast(firespot.transform.position, firespot.transform.forward, out cast);
        crosshair.transform.position = cast.point;

        //Reenable the crosshair after delay and bolting animation complete
        if(anim.GetCurrentAnimatorStateInfo(1).IsName("none") && Time.time >= timeToNextFire && !crosshair.gameObject.activeSelf)
        {
            crosshair.gameObject.SetActive(true);
        }

        //Fire the gun, and allow fire only after slight delay + animator is not bolting the gun on attack layer
        if (Input.GetButtonDown("Fire1") && crosshair.gameObject.activeSelf)
        {
            //Calculate minimal delay and disable crosshair
            timeToNextFire = Time.time + 1f / fireAnimDelay;
            crosshair.gameObject.SetActive(false);

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
            Debug.Log(hit.transform.root.name);
            if(hit.transform.root.name.Contains("German"))
            {
                GameObject bloodhit = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                bloodhit.transform.parent = hit.transform.root.gameObject.transform;
                hit.transform.root.GetComponent<BasicDeath>().Die();
            }
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }

        //Animation
        anim.SetTrigger("IsFiringBolt");
    }
}
