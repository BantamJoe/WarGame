using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.CharacterController;

public class WeaponScript : MonoBehaviour
{
    [Tooltip("Enables bayonet attack")]
    public bool bayonet = false;
    [Tooltip("1 = bolt rifle, 2 = semi rifle, 3 = shotgun, 4 = handgun, 5 = knife")]
    public int weaponType = 1;
    [Tooltip("1/firerate in seconds is delay between shots")]
    public float fireRate = 1f;
    [Tooltip("Damage per shot/pellet")]
    public float damage = 10f;
    [Tooltip("How much a shot deviates. The smaller, the more accurate the weapon")]
    public float spreadFactor = 0f;
    [Tooltip("Number of bullets/pellets fired from the gun per shot")]
    public float fireIterations = 1f;
    [Tooltip("Magazine capacity")]
    public int capacity = 5;
    [Tooltip("How long a reload takes")]
    public float reloadDelay = 1f;
    [Tooltip("Maximum range the weapon's raycast will extend")]
    public float range = 5000f;
    [HideInInspector]
    public float timeToNextFire = 0f;

    [HideInInspector]
    public int currentAmmo;

    [Tooltip("SFX when gun fires (including cycling)")]
    public AudioClip fire;
    [Tooltip("Object that projects ideal shot raycast")]
    public GameObject firespot;
    [Tooltip("Object that projects actual shot raycast")]
    public GameObject muzzlespot;
    [Tooltip("Muzzleflash effect")]
    public GameObject muzzleFlash;
    [Tooltip("Bullet impact effect")]
    public GameObject impactEffect;

    [HideInInspector]
    public Vector3 muzzleForward;

    private AudioSource weaponAudio;
    private Animator anim;

    // Use this for initialization
    private void Start()
    {
        weaponAudio = this.gameObject.AddComponent<AudioSource>();
        weaponAudio.playOnAwake = false;
        weaponAudio.clip = fire;
        weaponAudio.spatialBlend = 1f;
        weaponAudio.panStereo = 1f;
        anim = transform.GetComponentInParent<Animator>();  //TODO: remove the root dependency
        currentAmmo = capacity;
        muzzleForward = muzzlespot.transform.localRotation.eulerAngles;
    }


    public IEnumerator ReloadWeapon()
    {
        anim.SetTrigger("IsReloading");
        currentAmmo = 0;

        yield return new WaitForSeconds(reloadDelay);

        currentAmmo = capacity;
    }

    public IEnumerator CreateImpactEffect(Vector3 impactPoint, Vector3 impactNormal)
    {
        Destroy(Instantiate(impactEffect, impactPoint, Quaternion.LookRotation(impactNormal)), 1f);
        yield return null;
    }

    public IEnumerator CreateBloodEffect(vThirdPersonController ccHit, Vector3 impactPoint, Vector3 impactNormal)
    {
        Destroy(Instantiate(ccHit.bloodEffect, impactPoint, Quaternion.LookRotation(impactNormal)), 1f);
        yield return null;
    }

    public void PlayGunshot()
    {
        weaponAudio.Play();
    }

    public void GunFired()
    {
        PlayGunshot();

        //Instantiate particles like muzzleflash
        GameObject _muzzleflash = Instantiate(muzzleFlash, muzzlespot.transform.position, Quaternion.Euler(muzzlespot.transform.forward));
        _muzzleflash.transform.parent = muzzlespot.transform;
        Destroy(_muzzleflash, 1f);

        //Play shooting anim
        switch (weaponType)
        {
            case 1:
                anim.SetTrigger("IsFiringBolt");
                break;
            case 2:
                anim.SetTrigger("IsFiringSemi");
                break;
            case 3:
                anim.SetTrigger("IsFiringShotgun");
                break;
            case 4:
                anim.SetTrigger("IsFiringHandgun");
                break;
            case 5:
                break;
        }
    }
}
