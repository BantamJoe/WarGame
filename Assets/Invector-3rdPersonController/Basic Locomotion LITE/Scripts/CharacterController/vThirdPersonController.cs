using UnityEngine;
using System.Collections;

namespace Invector.CharacterController
{
    public class vThirdPersonController : vThirdPersonAnimator
    {
        public float grenadeThrowForce = 50f;
        public float health = 100f;
        public string Team;
        public GameObject bloodEffect;

        [HideInInspector]
        public GameObject spine;
        [HideInInspector]
        public GameObject weaponContainer;
        [HideInInspector]
        public GameObject weapon;
        public GameObject grenadePrefab;
        //[HideInInspector]
        public BasicShoot basicShoot;

        private int selectedWeapon = 0;
        private int previousWeapon;
        private BasicDeath basicDeath;

        protected virtual void Start()
        {
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

            //Prepare the death script
            basicDeath = GetComponent<BasicDeath>();

            //Prepare the spine
            spine = gameObject.transform.FindDeepChild("Bip001 Spine").gameObject;
            if (spine == null) Debug.LogError("Spine was not found for " + gameObject.name);

            
#if !UNITY_EDITOR
                Cursor.visible = false;
#endif
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
            ActivateWeapons();
        }
        private void ActivateWeapons()
        {
            //Only reactivate weapons if new weapon selected and character has weapon
            if (previousWeapon != selectedWeapon && selectedWeapon != -1)
            {
                int i = 0;
                foreach (Transform weapon in weaponContainer.transform)
                {
                    if (i == selectedWeapon)
                    {
                        weapon.gameObject.SetActive(true);
                        this.weapon = weapon.gameObject;
                        weapon.gameObject.GetComponent<BasicShoot>().firespot = basicShoot.firespot;
                        this.basicShoot = weapon.GetComponent<BasicShoot>();
                        
                        animator.SetInteger("WeaponType", basicShoot.weaponType);
                        animator.SetTrigger("IsDrawingWeapon");
                    }
                    else
                        weapon.gameObject.SetActive(false);
                    i++;
                }
            }
        }
        private void InitializeWeapons()
        {
            int i = 0;
            foreach (Transform weapon in weaponContainer.transform)
            {
                if(i == 0)
                {
                    this.weapon = weapon.gameObject;
                    basicShoot = weapon.GetComponent<BasicShoot>();
                    if (basicShoot == null) Debug.LogError("WeaponContainer contained gameObject that did NOT have a BasicShoot script.");
                }
                else
                    weapon.gameObject.SetActive(false);
                i++;
            }
        }
        public virtual void Prone(bool value)
        {
            //if attempting to stand, check for clearance
            if (!value && isProning && !CanStandFromProne())
            {
                return;
            }

            isProning = value;
            isCrouching = false;

            animator.SetBool("IsProning", value);
            animator.SetBool("IsCrouching", false);
            
            AdjustCapsule();
        }
        public virtual void Crouch(bool value)
        {
            //if attempting to stand, check for clearance
            if(!value && isCrouching && !CanStandFromCrouch())
            {
                return;
            }
            if(value && isProning && !CanCrouchFromProne())
            {
                return;
            }

            isCrouching = value;
            isProning = false;

            animator.SetBool("IsProning", false);
            animator.SetBool("IsCrouching", value);

            AdjustCapsule();
        }
        public virtual void Shoot()
        {
            if (!isSprinting && !isReloading && weapon.activeSelf)
            {
                basicShoot.Shoot();
            }
        }
        public virtual void Reload()
        {
            if (!isSprinting && weapon.activeSelf)
            {
                StartCoroutine(basicShoot.Reload());
            }
        }

        public virtual void Sprint(bool value)
        {                 
            if(!isCrouching && !isProning)
                isSprinting = value;            
        }

        public virtual void Walk(bool value)
        {
            isWalking = value;
        }
        public virtual void Aim(bool value)
        {
            isAiming = value;
        }
        public virtual void Die()
        {
            isDead = true;
            isWalking = false;
            isSprinting = false;
            isAiming = false;

            input.x = 0f;
            input.y = 0f;
            speed = 0f;

            basicDeath.Die();
        }
        public virtual void TakeDamage(float damage)
        {
            health -= damage;
            if(health <= 0)
            {
                Die();
            }
            else
            {
                animator.SetTrigger("IsHurt");
            }
        }
        public virtual void Strafe()
        {
            if (locomotionType == LocomotionType.OnlyFree) return;
            isStrafing = !isStrafing;
        }
        public virtual void Jump()
        {
            // conditions to do this action
            bool jumpConditions = isGrounded && !isJumping;
            // return if jumpCondigions is false
            if (!jumpConditions) return;
            // trigger jump behaviour
            jumpCounter = jumpTimer;            
            isJumping = true;
            // trigger jump animations            
            if (_rigidbody.velocity.magnitude < 1)
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                animator.CrossFadeInFixedTime("JumpMove", 0.2f);
        }
        public virtual void ThrowGrenade()
        {
            animator.SetTrigger("IsThrowingGrenade");
            GameObject grenade = Instantiate(grenadePrefab, transform.position + transform.forward + transform.up * (_capsuleCollider.height), transform.rotation);
            Rigidbody grb = grenade.GetComponent<Rigidbody>();
            grb.AddForce(transform.forward * grenadeThrowForce, ForceMode.Impulse);
        }

        public virtual void RotateWithAnotherTransform(Transform referenceTransform)
        {
            var newRotation = new Vector3(transform.eulerAngles.x, referenceTransform.eulerAngles.y, transform.eulerAngles.z);
            var newSpineRotation = new Vector3(spine.transform.eulerAngles.x, spine.transform.eulerAngles.y, spine.transform.eulerAngles.z - referenceTransform.eulerAngles.x);

            //Rotate the spine of the biped
            spine.transform.rotation = Quaternion.Lerp(Quaternion.Euler(newSpineRotation), spine.transform.rotation, strafeRotationSpeed * Time.fixedDeltaTime);

            //Rotate the whole character for looking left and right
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation), strafeRotationSpeed * Time.fixedDeltaTime);

            //Update rotations for smooth updating
            targetRotation = transform.rotation;
            //spine.transform.rotation = Quaternion.Euler(newSpineRotation);
        }
    }
}

//Use this for finding grandchild by name at Start, extension class for Transforms
public static class TransformDeepChildExtension
{
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }
}