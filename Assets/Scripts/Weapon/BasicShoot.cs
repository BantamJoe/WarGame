using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShoot : MonoBehaviour
{
    #region Public Variables
    public float damage = 10f;
    public float range = 5000f;
    public string enemyString = "German";

    public Transform crosshair;
    public AudioClip fire;
    public GameObject firespot;
    public GameObject impactEffect;
    public GameObject bloodEffect;
    public GameObject muzzleFlash;
    public GameObject bulletSpawnPoint;
    #endregion

    #region Private Variables
    protected AudioSource weaponAudio;
    protected Animator anim;
     
    protected float fireAnimDelay = 1f;
    protected float timeToNextFire = 0f;

    protected int animAttackLayer = 1;
     
    protected string noAttackAnim = "none";
    #endregion

    #region Private Callbacks
    protected virtual void Start()
    {
        weaponAudio = this.gameObject.AddComponent<AudioSource>();
        weaponAudio.clip = fire;
        var bloodMain = bloodEffect.GetComponent<ParticleSystem>().main;
        bloodMain.loop = false;
        anim = transform.root.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Update the 3D crosshair by casting a point and updating the draw position. Ray is casted from gun's muzzle
        RaycastHit cast;
        Physics.Raycast(bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.forward, out cast);
        crosshair.transform.position = cast.point;

        //Reenable the crosshair after delay and bolting animation complete
        if (anim.GetCurrentAnimatorStateInfo(animAttackLayer).IsName(noAttackAnim) && Time.time >= timeToNextFire && !crosshair.gameObject.activeSelf)
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

    /// <summary>
    /// Plays a weapon's full activity of shooting:
    /// -plays muzzle flash
    /// -plays audio clip of weapon going off
    /// -sends damage message to shot item
    /// 
    /// This is a hit scan style of shoot.
    /// </summary>
    protected virtual void Shoot()
    {
        RaycastHit hit;
        //spawn muzzle flash
        Instantiate(muzzleFlash, firespot.transform.position, Quaternion.Euler(firespot.transform.forward));
        weaponAudio.Play();

        //Did we hit something?
        if (Physics.Raycast(bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.root.name);
            hit.transform.root.gameObject.SendMessage("Shot", damage, SendMessageOptions.DontRequireReceiver);
            //hit.transform.root.GetComponent<BasicDeathAI>().Die();
            if (hit.transform.root.name.Contains(enemyString))
            {
                GameObject bloodhit = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                bloodhit.transform.parent = hit.transform.root.gameObject.transform;
            }
            else
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        //Animation
        anim.SetTrigger("IsFiringBolt");
    }
    #endregion

    #region Public Callbacks

    #endregion
}
