using UnityEngine;
using System.Collections;

namespace Invector.CharacterController
{
    public class vThirdPersonController : vThirdPersonAnimator
    {
        public string Team;
        public GameObject spine;
        public GameObject bloodEffect;

        /*
         *Come up with a cleaner way of getting to the shooting script. This is better than before - keep access to the script here. 
         */
        public GameObject weapon;

        private BasicShoot basicShoot;
        private BasicDeath basicDeath;

        protected virtual void Start()
        {
            basicShoot = weapon.GetComponent<BasicShoot>();
            basicDeath = GetComponent<BasicDeath>();

            if (spine == null) return;
            if (weapon != null && basicShoot == null) return;
            
#if !UNITY_EDITOR
                Cursor.visible = false;
#endif
        }
        public virtual void Shoot()
        {
            if(!isSprinting)
                basicShoot.Shoot();
        }

        public virtual void Sprint(bool value)
        {                                   
            isSprinting = value;            
        }

        public virtual void Walk(bool value)
        {
            isAiming = value;
            isWalking = value;
        }
        public virtual void Die(bool value)
        {
            isDead = value;
            basicDeath.Die();
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

        public virtual void RotateWithAnotherTransform(Transform referenceTransform)
        {
            var newRotation = new Vector3(transform.eulerAngles.x, referenceTransform.eulerAngles.y, transform.eulerAngles.z);
            var newSpineRotation = new Vector3(referenceTransform.eulerAngles.x, spine.transform.eulerAngles.y, spine.transform.eulerAngles.z);

            //Rotate the spine of the biped
            spine.transform.rotation = Quaternion.Lerp(Quaternion.Euler(newSpineRotation),spine.transform.rotation, strafeRotationSpeed * Time.fixedDeltaTime);
            
            //Rotate the whole character for looking left and right
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation), strafeRotationSpeed * Time.fixedDeltaTime);
            
            //Update rotations for smooth updating
            targetRotation = transform.rotation;
            spine.transform.rotation = Quaternion.Euler(newSpineRotation);

            /* Original version
            var newRotation = new Vector3(transform.eulerAngles.x, referenceTransform.eulerAngles.y, transform.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation), strafeRotationSpeed * Time.fixedDeltaTime);
            targetRotation = transform.rotation;
            */
        }
    }
}