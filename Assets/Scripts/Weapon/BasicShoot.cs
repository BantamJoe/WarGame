using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.CharacterController
{
    public class BasicShoot : MonoBehaviour
    {
        [Tooltip("1 = bolt rifle, 2 = semi rifle, 3 = shotgun, 4 = handgun")]
        public int weaponType = 1;
        [Tooltip("1/firerate in seconds is delay between shots")]
        public float fireRate = 1f;
        public float damage = 10f;
        public float range = 5000f;
        public float reloadDelay = 1f;
        [Tooltip("Magazine capacity")]
        public int capacity = 5;

        public AudioClip fire;
        public GameObject firespot;
        public GameObject muzzlespot;
        public GameObject impactEffect;
        public GameObject muzzleFlash;

        private AudioSource weaponAudio;
        private Animator anim;
        private float timeToNextFire = 0f;
        private int currentAmmo;

        void Start()
        {
            weaponAudio = this.gameObject.AddComponent<AudioSource>();
            weaponAudio.clip = fire;
            weaponAudio.spatialBlend = 1f;
            weaponAudio.panStereo = 1f;
            anim = transform.GetComponentInParent<Animator>();  //TODO: remove the root dependency
            currentAmmo = capacity;
        }

        public void Reload()
        {
            //Play reloading anim
            anim.SetTrigger("IsReloading");
            currentAmmo = capacity;
            timeToNextFire += reloadDelay;
        }

        public void Shoot()
        {
            //If next time to fire has been reached and animator is reset
            if (Time.time >= timeToNextFire && currentAmmo != 0)// && anim.GetCurrentAnimatorStateInfo(1).IsName("none"))
            {
                RaycastHit firespotHit, muzzlespotHit;
                Transform originalMuzzlespotTransform = muzzlespot.transform;
                timeToNextFire = Time.time + 1f / fireRate;

                //Fire a ray from the camera. This is the ideal hit position
                if (Physics.Raycast(firespot.transform.position, firespot.transform.forward, out firespotHit, range))
                {
                    //Point the gun's muzzle ray at where the cameras crosshair is
                    muzzlespot.transform.LookAt(firespotHit.point);

                    //Fire a ray from the guns muzzle to the cameras crosshair
                    if (Physics.Raycast(muzzlespot.transform.position, muzzlespot.transform.forward, out muzzlespotHit, range))
                    {
                        Component ccComponent = muzzlespotHit.transform.gameObject.GetComponentInParent(typeof(vThirdPersonController));

                        if (ccComponent != null)
                        {
                            vThirdPersonController cc = ccComponent.GetComponent<vThirdPersonController>();
                            cc.Die();
                            Destroy(Instantiate(cc.bloodEffect, muzzlespotHit.point, Quaternion.LookRotation(muzzlespotHit.normal)),1f);
                        }
                        Destroy(Instantiate(impactEffect, muzzlespotHit.point, Quaternion.LookRotation(muzzlespotHit.normal)),1f);
                    }

                    //Restore muzzlespots original orientation
                    muzzlespot.transform.SetPositionAndRotation(originalMuzzlespotTransform.position, originalMuzzlespotTransform.rotation);
                }
                
                //Play sound and effects regardless of hit
                weaponAudio.Play();

                currentAmmo -= 1;

                GameObject _muzzleflash = Instantiate(muzzleFlash, muzzlespot.transform.position, Quaternion.Euler(muzzlespot.transform.forward));
                _muzzleflash.transform.parent = muzzlespot.transform;
                Destroy(_muzzleflash, 1f);


                //Play shooting anim
                switch(weaponType)
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
                        break;
                    case 5:
                        break;
                }
            }
        }
    }
}

