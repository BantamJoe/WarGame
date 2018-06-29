using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Invector.CharacterController
{
    [RequireComponent(typeof(ShootingScript))]
    public class vThirdPersonController : vThirdPersonAnimator
    {
        //General variables
        [Tooltip("Team the character is on, used for identifying enemies")]
        public string Team;
        [Tooltip("Health points the character has before dying")]
        [SyncVar]
        public float health = 100f;
        [Tooltip("Blood effect particle played when damage taken")]
        public GameObject bloodEffect;
        [Tooltip("Grenade prefab that is thrown")]
        public GameObject grenadePrefab;
        [Tooltip("Force grenade is thrown with towards the target vector")]
        public float grenadeThrowForce = 50f;

        //Script references
        [HideInInspector]
        public BasicDeath basicDeath;


        //Containers and game object references
        [HideInInspector]
        public GameObject spine;
        [HideInInspector]
        public ShootingScript shooting;

        [Tooltip("Artillery prefab for officer class")]
        public GameObject artilleryPrefab;
        

        protected virtual void Start()
        {
            shooting = GetComponent<ShootingScript>();

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
            shooting.SelectWeapon(value);
        }
        

        public virtual void SpecialAbility(GameObject camera)
        {
            StartCoroutine(ArtilleryStrike(camera));
        }
        private IEnumerator ArtilleryStrike(GameObject camera)
        {
            RaycastHit strikeHit;

            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out strikeHit))
            {
                Debug.DrawRay(camera.transform.position, camera.transform.forward * Mathf.Abs(Vector3.Distance(camera.transform.position, strikeHit.point)), Color.red, 15f);
                animator.SetTrigger("IsThrowingGrenade");
                yield return new WaitForSeconds(10f);
                Destroy(Instantiate(artilleryPrefab, strikeHit.point + (Vector3.up * 20f), Quaternion.LookRotation(Vector3.up)), 45f);
            }
        }

        public virtual void BayonetAttack()
        {
            if(shooting.CurrentWeapon.bayonet && !isProning && !isSprinting && !isReloading)
            {
                StartCoroutine(shooting.BayonetAttack());
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
            if (!isSprinting && !isReloading && shooting.weaponObj.activeSelf && shooting.CurrentWeapon.weaponType < 5)
            {
                shooting.Shoot();
            }
            if(!isReloading && shooting.weaponObj.activeSelf && shooting.CurrentWeapon.weaponType == 5)
            {
                StartCoroutine(shooting.KnifeAttack());
            }
        }
        public virtual void Reload()
        {
            if (!isSprinting && shooting.weaponObj.activeSelf)
            {
                StartCoroutine(shooting.Reload());
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

            AdjustCapsule();
            basicDeath.Die();
        }

        [ClientRpc]
        public virtual void RpcTakeDamage(float damage)
        {
            Debug.Log("I took damage " + gameObject.name);
            health -= damage;
            Debug.Log("Now I have " + health + " health");
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
        public virtual IEnumerator ThrowGrenadeTowardCamera(GameObject camera)
        {
            if(!isReloading && !isSprinting)
            {
                animator.SetTrigger("IsThrowingGrenade");

                yield return new WaitForSeconds(0.6f);

                GameObject grenade = Instantiate(grenadePrefab, transform.position + transform.forward + transform.up * (_capsuleCollider.height), transform.rotation);
                Rigidbody grb = grenade.GetComponent<Rigidbody>();
                grb.AddForce(camera.transform.forward * grenadeThrowForce, ForceMode.VelocityChange);
            }
        }
        public virtual IEnumerator ThrowGrenadeForward()
        {
            if (!isReloading && !isSprinting)
            {
                animator.SetTrigger("IsThrowingGrenade");

                yield return new WaitForSeconds(0.6f);

                GameObject grenade = Instantiate(grenadePrefab, transform.position + transform.forward + transform.up * (_capsuleCollider.height), transform.rotation);
                Rigidbody grb = grenade.GetComponent<Rigidbody>();
                grb.AddForce(transform.forward * grenadeThrowForce, ForceMode.VelocityChange);
            }
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