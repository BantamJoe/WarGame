using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.CharacterController
{
    public class BasicShoot : MonoBehaviour
    {
        public float damage = 10f;
        public float range = 5000f;

        public AudioClip fire;
        public GameObject firespot;
        public GameObject muzzlespot;
        public GameObject impactEffect;
        public GameObject muzzleFlash;

        private AudioSource weaponAudio;
        private Animator anim;
        private float fireAnimDelay = 1f;
        private float timeToNextFire = 0f;

        void Start()
        {
            weaponAudio = this.gameObject.AddComponent<AudioSource>();
            weaponAudio.clip = fire;
            anim = transform.root.gameObject.GetComponent<Animator>();
        }

        public void Shoot()
        {
            //If next time to fire has been reached and animator is reset
            if (Time.time >= timeToNextFire && anim.GetCurrentAnimatorStateInfo(1).IsName("none"))
            {
                RaycastHit firespotHit, muzzlespotHit;
                Transform originalMuzzlespotTransform = muzzlespot.transform;
                
                //Fire a ray from the camera. This is the ideal hit position
                if (Physics.Raycast(firespot.transform.position, firespot.transform.forward, out firespotHit, range))
                {
                    //Point the gun's muzzle ray at where the cameras crosshair is
                    muzzlespot.transform.LookAt(firespotHit.point);

                    //Fire a ray from the guns muzzle to the cameras crosshair
                    if (Physics.Raycast(muzzlespot.transform.position, muzzlespot.transform.forward, out muzzlespotHit, range, -1, QueryTriggerInteraction.Ignore))
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
                timeToNextFire = Time.time + 1f / fireAnimDelay;
                GameObject _muzzleflash = Instantiate(muzzleFlash, muzzlespot.transform.position, Quaternion.Euler(muzzlespot.transform.forward));
                _muzzleflash.transform.parent = muzzlespot.transform;
                Destroy(_muzzleflash, 1f);

                anim.SetTrigger("IsFiringBolt");
            }
        }
    }
}

