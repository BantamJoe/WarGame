using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Networking;

namespace Invector.CharacterController
{
    public class BasicShoot : NetworkBehaviour
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
        [HideInInspector]
        public bool reloading = false;

        private AudioSource weaponAudio;
        private Animator anim;
        private NetworkManagerUnity manager;
        

        void Start()
        {
            weaponAudio = this.gameObject.AddComponent<AudioSource>();
            weaponAudio.playOnAwake = false;
            weaponAudio.clip = fire;
            weaponAudio.spatialBlend = 1f;
            weaponAudio.panStereo = 1f;
            anim = transform.GetComponentInParent<Animator>();  //TODO: remove the root dependency
            currentAmmo = capacity;
            muzzleForward = muzzlespot.transform.localRotation.eulerAngles;
            manager = FindObjectOfType<NetworkManagerUnity>();
        }


        public IEnumerator Reload()
        {
            if(!reloading)//This was done to prevent multiple entries into reloading loop. Consider moving all weapon code to the guns themselves instead of on cc
            {
                reloading = true;
                anim.SetTrigger("IsReloading");
                currentAmmo = 0;

                yield return new WaitForSeconds(reloadDelay);

                currentAmmo = capacity;
                reloading = false;
            }
        }

        [Command]
        public void CmdShoot()
        {
            //If weapon is knife, don't shoot
            if(weaponType == 5)
            {
                StartCoroutine(KnifeAttack());
                return;
            }
            //If next time to fire has been reached and animator is reset
            if (Time.time >= timeToNextFire && currentAmmo != 0 && !reloading)// && anim.GetCurrentAnimatorStateInfo(1).IsName("none"))
            {
                RaycastHit firespotHit, muzzlespotHit;
                Transform originalMuzzlespotTransform = muzzlespot.transform;
                timeToNextFire = Time.time + 1f / fireRate;

                //Fire a ray from the camera. This is the ideal hit position
                if (Physics.Raycast(firespot.transform.position, firespot.transform.forward, out firespotHit, range))
                {
                    //Iterate over the number of fireIterations (Useful for shotguns)
                    for (int i = 0; i < fireIterations; i++)
                    {
                        //Point the gun's muzzle ray at where the cameras crosshair is
                        muzzlespot.transform.LookAt(firespotHit.point);

                        //Deviate shot based on spreadFactor (accuracy simulation)
                        Vector3 directionOfShot = muzzlespot.transform.forward;
                        directionOfShot.x += Random.Range(-spreadFactor, spreadFactor);
                        directionOfShot.y += Random.Range(-spreadFactor, spreadFactor);
                        directionOfShot.z += Random.Range(-spreadFactor, spreadFactor);

                        //Fire a ray from the guns muzzle to the cameras crosshair
                        if (Physics.Raycast(muzzlespot.transform.position, directionOfShot, out muzzlespotHit, range))
                        {
                            //Look towards this sort of solution to remove cc dependency
                            //muzzlespotHit.transform.gameObject.SendMessageUpwards("TakeDamage",damage);

                            Component ccComponent = muzzlespotHit.transform.gameObject.GetComponentInParent(typeof(vThirdPersonController));

                            if (ccComponent != null)
                            {
                                vThirdPersonController cc = ccComponent.GetComponent<vThirdPersonController>();
                                cc.RpcTakeDamage(damage);
                                Destroy(Instantiate(cc.bloodEffect, muzzlespotHit.point, Quaternion.LookRotation(muzzlespotHit.normal)), 1f);
                            }
                            Destroy(Instantiate(impactEffect, muzzlespotHit.point, Quaternion.LookRotation(muzzlespotHit.normal)), 1f);
                        }
                    }
                    //Restore muzzlespots original orientation
                    muzzlespot.transform.SetPositionAndRotation(originalMuzzlespotTransform.position, originalMuzzlespotTransform.rotation);
                }
                //Play sound and effects regardless of hit
                weaponAudio.Play();

                //Decrement Ammo
                currentAmmo -= 1;

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

        public IEnumerator BayonetAttack()
        {
            anim.SetTrigger("IsBayonetting");

            yield return new WaitForSeconds(0.3f);

            RaycastHit bayonetHit;
            if (Physics.Raycast(muzzlespot.transform.position, muzzlespot.transform.forward, out bayonetHit, 7f))
            {
                Component ccComponent = bayonetHit.transform.gameObject.GetComponentInParent(typeof(vThirdPersonController));

                if (ccComponent != null)
                {
                    vThirdPersonController cc = ccComponent.GetComponent<vThirdPersonController>();
                    cc.RpcTakeDamage(100f);
                    Destroy(Instantiate(cc.bloodEffect, bayonetHit.point, Quaternion.LookRotation(bayonetHit.normal)), 1f);
                }
            }
        }
        public IEnumerator KnifeAttack()
        {
            anim.SetTrigger("IsKnifing");

            yield return new WaitForSeconds(0.2f);

            RaycastHit knifeHit;
            if (Physics.Raycast(muzzlespot.transform.position, muzzlespot.transform.forward, out knifeHit, 2f))
            {
                Component ccComponent = knifeHit.transform.gameObject.GetComponentInParent(typeof(vThirdPersonController));

                if (ccComponent != null)
                {
                    vThirdPersonController cc = ccComponent.GetComponent<vThirdPersonController>();
                    cc.RpcTakeDamage(damage);
                    Destroy(Instantiate(cc.bloodEffect, knifeHit.point, Quaternion.LookRotation(knifeHit.normal)), 1f);
                }
            }
        }
    }
}

