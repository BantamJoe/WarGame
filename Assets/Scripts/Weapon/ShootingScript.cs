using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Networking;
using Invector.CharacterController;

public class ShootingScript : NetworkBehaviour
{
    [HideInInspector]
    public bool reloading = false;
    [HideInInspector]
    public GameObject weaponObj;

    public GameObject weaponContainer;

    public WeaponScript CurrentWeapon { get; private set; }

    //Private vars used for weapon selection
    private int selectedWeapon = 0;
    private int previousWeapon;

    //Time before weapons are actually swapped, always draw animation time to occur
    private float weaponDrawDelay = 0.15f;
    private bool weaponSwapCRRunning = false;

    private NetworkManagerUnity manager;

    private Animator anim;


    void Start()
    {
        //currentWeapon = 
        manager = FindObjectOfType<NetworkManagerUnity>();
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("Animator found to be null on " + this.gameObject.name);
        }
        //Prepare the weapon and weapon container
        weaponContainer = gameObject.transform.FindDeepChild("WeaponContainer").gameObject;
        if (weaponContainer == null) Debug.LogError("Weapon container NOT FOUND on " + gameObject.name);
        if (weaponContainer.transform.childCount > 0)
        {
            InitializeWeapons();
        }
        else
        {
            selectedWeapon = -1;
            Debug.LogWarning("Weapon container on " + gameObject.name + " is empty. Was this intentional?");
        }
    }


    public IEnumerator Reload()
    {
        if (!reloading && CurrentWeapon.currentAmmo != CurrentWeapon.capacity)//This was done to prevent multiple entries into reloading loop. Consider moving all weapon code to the guns themselves instead of on cc
        {
            reloading = true;
            CmdOnReload();
            yield return CurrentWeapon.ReloadWeapon();
            reloading = false;
        }
    }

    [Command]
    private void CmdOnReload()
    {
        RpcOnReload();
    }

    [ClientRpc]
    private void RpcOnReload()
    {
        CurrentWeapon.ReloadWeapon();
    }

    public virtual void SelectWeapon(bool value)
    {
        previousWeapon = selectedWeapon;
        if (value)
        {
            if (selectedWeapon >= weaponContainer.transform.childCount - 1)
                selectedWeapon = 0;
            else
                selectedWeapon++;
        }
        else
        {
            if (selectedWeapon <= 0)
                selectedWeapon = weaponContainer.transform.childCount - 1;
            else
                selectedWeapon--;
        }
        if (weaponSwapCRRunning)
        {
            StopCoroutine(ActivateWeapons());
        }
        StartCoroutine(ActivateWeapons());
    }

    private IEnumerator ActivateWeapons()
    {
        weaponSwapCRRunning = true;
        //Only reactivate weapons if new weapon selected and character has weapon
        yield return new WaitForSeconds(weaponDrawDelay);

        if (previousWeapon != selectedWeapon && selectedWeapon != -1)
        {
            int i = 0;
            //TODO: could possibly be sped up with having weapons in a hash table and just index to them directly
            foreach (Transform weapon in weaponContainer.transform)
            {
                if (i == selectedWeapon)
                {
                    weapon.gameObject.SetActive(true);
                    this.weaponObj = weapon.gameObject;
                    weapon.gameObject.GetComponent<WeaponScript>().firespot = CurrentWeapon.firespot;
                    this.CurrentWeapon = weapon.GetComponent<WeaponScript>();

                    anim.SetInteger("WeaponType", CurrentWeapon.weaponType);
                    anim.SetTrigger("IsDrawingWeapon");
                }
                else
                {
                    weapon.gameObject.SetActive(false);
                }
                i++;
            }
        }
        weaponSwapCRRunning = false;
    }

    private void InitializeWeapons()
    {
        int i = 0;
        foreach (Transform weapon in weaponContainer.transform)
        {
            if (i == 0)
            {
                this.weaponObj = weapon.gameObject;
                CurrentWeapon = weapon.GetComponent<WeaponScript>();
                if (CurrentWeapon == null) Debug.LogError("WeaponContainer contained gameObject that did NOT have a WeaponScript.");
            }
            else
                weapon.gameObject.SetActive(false);
            i++;
        }
    }

    [Client]
    public void Shoot()
    {
        if (!isLocalPlayer)
        {
            return; //this should never get here but it is my sanity check
        }
        //If weapon is knife, don't shoot
        if (CurrentWeapon.weaponType == 5)
        {
            StartCoroutine(KnifeAttack());
            return;
        }
        //If next time to fire has been reached and animator is reset
        if (Time.time >= CurrentWeapon.timeToNextFire && CurrentWeapon.currentAmmo != 0 && !reloading)// && anim.GetCurrentAnimatorStateInfo(1).IsName("none"))
        {
            CurrentWeapon.timeToNextFire = Time.time + 1f / CurrentWeapon.fireRate;
            CurrentWeapon.currentAmmo--;
            CurrentWeapon.GunFired();
            CmdShoot(this.netId);
        }
    }

    [ClientRpc]
    public void RpcGunFired()
    {
        if (isLocalPlayer)
        {
            CurrentWeapon.GunFired();
        }
        //CurrentWeapon.currentAmmo -= 1;
    }

    [ClientRpc]
    public void RpcUpdateTimeToFireAndAmmo(float timeTillNextFire)
    {
        if (isLocalPlayer)
        {
            CurrentWeapon.timeToNextFire = Time.time + 1f / CurrentWeapon.fireRate;
            CurrentWeapon.currentAmmo--;
        }
    }

    [Command]
    public void CmdShoot(NetworkInstanceId playerShootingId)
    {
        vThirdPersonController playerShooting = NetworkServer.FindLocalObject(playerShootingId).GetComponent<vThirdPersonController>();
        ShootingScript theirShooting = playerShooting.shooting;
        RaycastHit firespotHit, muzzlespotHit;
        Transform originalMuzzlespotTransform = theirShooting.CurrentWeapon.muzzlespot.transform;
        //Fire a ray from the camera. This is the ideal hit position
        if (Physics.Raycast(theirShooting.CurrentWeapon.firespot.transform.position, theirShooting.CurrentWeapon.firespot.transform.forward, out firespotHit, theirShooting.CurrentWeapon.range))
        {
            //Iterate over the number of fireIterations (Useful for shotguns)
            for (int i = 0; i < theirShooting.CurrentWeapon.fireIterations; i++)
            {
                //Point the gun's muzzle ray at where the cameras crosshair is
                theirShooting.CurrentWeapon.muzzlespot.transform.LookAt(firespotHit.point);

                //Deviate shot based on spreadFactor (accuracy simulation)
                Vector3 directionOfShot = theirShooting.CurrentWeapon.muzzlespot.transform.forward;
                directionOfShot.x += Random.Range(-theirShooting.CurrentWeapon.spreadFactor, theirShooting.CurrentWeapon.spreadFactor);
                directionOfShot.y += Random.Range(-theirShooting.CurrentWeapon.spreadFactor, theirShooting.CurrentWeapon.spreadFactor);
                directionOfShot.z += Random.Range(-theirShooting.CurrentWeapon.spreadFactor, theirShooting.CurrentWeapon.spreadFactor);

                //Fire a ray from the guns muzzle to the cameras crosshair
                if (Physics.Raycast(theirShooting.CurrentWeapon.muzzlespot.transform.position, directionOfShot, out muzzlespotHit, theirShooting.CurrentWeapon.range))
                {
                    //Look towards this sort of solution to remove cc dependency
                    //muzzlespotHit.transform.gameObject.SendMessageUpwards("TakeDamage",damage);

                    Component ccComponent = muzzlespotHit.transform.gameObject.GetComponentInParent(typeof(vThirdPersonController));

                    if (ccComponent != null)
                    {
                        vThirdPersonController cc = ccComponent.GetComponent<vThirdPersonController>();
                        //cc.TakeDamage(CurrentWeapon.damage);
                        cc.RpcTakeDamage(theirShooting.CurrentWeapon.damage);
                        //TODO: may want to make these effects not be coroutines
                        //RpcCreateBloodEffect(cc.netId, muzzlespotHit.point, muzzlespotHit.normal);
                    }
                    //RpcCreateImpactEffect(muzzlespotHit.point, muzzlespotHit.normal);
                }
            }
            //Restore muzzlespots original orientation
            theirShooting.CurrentWeapon.muzzlespot.transform.SetPositionAndRotation(originalMuzzlespotTransform.position, originalMuzzlespotTransform.rotation);
        }
    }

    [ClientRpc]
    public void RpcCreateImpactEffect(Vector3 impactPoint, Vector3 impactNormal)
    {
        if (isLocalPlayer)
        {
            StartCoroutine(CurrentWeapon.CreateImpactEffect(impactPoint, impactNormal));
        }        
    }

    [ClientRpc]
    public void RpcCreateBloodEffect(NetworkInstanceId ccID, Vector3 impactPoint, Vector3 impactNormal)
    {
        if (isLocalPlayer)
        {
            GameObject ccObj = NetworkServer.FindLocalObject(ccID);
            vThirdPersonController cc = ccObj.GetComponent<vThirdPersonController>();
            StartCoroutine(CurrentWeapon.CreateBloodEffect(cc, impactPoint, impactNormal));
        }        
    }

    public IEnumerator BayonetAttack()
    {
        anim.SetTrigger("IsBayonetting");

        yield return new WaitForSeconds(0.3f);

        RaycastHit bayonetHit;
        if (Physics.Raycast(CurrentWeapon.muzzlespot.transform.position, CurrentWeapon.muzzlespot.transform.forward, out bayonetHit, 7f))
        {
            Component ccComponent = bayonetHit.transform.gameObject.GetComponentInParent(typeof(vThirdPersonController));

            if (ccComponent != null)
            {
                vThirdPersonController cc = ccComponent.GetComponent<vThirdPersonController>();
                cc.RpcTakeDamage(100f);
                //StartCoroutine(CurrentWeapon.CreateBloodEffect(cc, bayonetHit));
            }
        }
    }
    public IEnumerator KnifeAttack()
    {
        anim.SetTrigger("IsKnifing");

        yield return new WaitForSeconds(0.2f);

        RaycastHit knifeHit;
        if (Physics.Raycast(CurrentWeapon.muzzlespot.transform.position, CurrentWeapon.muzzlespot.transform.forward, out knifeHit, 2f))
        {
            Component ccComponent = knifeHit.transform.gameObject.GetComponentInParent(typeof(vThirdPersonController));

            if (ccComponent != null)
            {
                vThirdPersonController cc = ccComponent.GetComponent<vThirdPersonController>();
                cc.RpcTakeDamage(CurrentWeapon.damage);
                //StartCoroutine(CurrentWeapon.CreateBloodEffect(cc, knifeHit));
            }
        }
    }
}


