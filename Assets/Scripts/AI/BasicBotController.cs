using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Invector.CharacterController
{
    public class BasicBotController : MonoBehaviour
    {
        [Tooltip("Can the bot use his firearm based weapon?")]
        public bool canShoot = true;
        [Tooltip("Raycast length from gun when determing to shoot")]
        public float shootRange = 30f;
        [Tooltip("How fast can the bot rotate when aiming at something?")]
        public float rotationSpeed = 10f;
        [Tooltip("Radius of enemy detection")]
        public float detectRadius = 20f;
        [Tooltip("Spine offset for aiming. Adjust in playmode and then copy values over")]
        public Vector3 Offset;
        [Tooltip("Bots aiming accuracy prior to shooting. Lower means MORE accurate. This is seperate to weapon accuracy, which is applied during the shot.")]
        public float aimAccuracy = 0.02f;
        [Tooltip("Distance bot will try to throw grenade")]
        public float grenadeRange = 20f;

        public GameObject moveTarget;
        public GameObject attackTarget;
        
        private NavMeshAgent agent;

        private GameObject originalMoveTarget;
        
        private Vector3 lastKnownMoveTargetPosition;

        private Transform spine;

        private vThirdPersonController cc;

        private bool isDead = false;
        private bool isShooting = false;
        private int playerLayerIndex = 1 << 8;

        // Use this for initialization
        void Start()
        {
            agent = GetComponentInChildren<NavMeshAgent>();
            cc = GetComponent<vThirdPersonController>();

            if (cc != null)
            {
                cc.Init();
                spine = cc.animator.GetBoneTransform(HumanBodyBones.Spine);
            }
            if (agent == null)
                Debug.LogError("Navmesh agent missing on bot");
            if(moveTarget != null)
                agent.SetDestination(moveTarget.transform.position);

            if(moveTarget != null)
            {
                originalMoveTarget = moveTarget;
                lastKnownMoveTargetPosition = moveTarget.transform.position;
            }
        }

        void LateUpdate()
        {
            //If the character controller is dead, kill the bot controller
            if (cc.isDead && !isDead)
            {
                isDead = true;
                AgentWalk(false);   
                agent.enabled = false;

                //Look into removing the capsule a better way.
                cc._capsuleCollider.enabled = false;
            }
            //Bot is alive and functioning
            if (!isDead && agent.enabled)
            {
                if(attackTarget == null)
                {
                    AgentCheckForAttackTarget();
                }
                if (moveTarget != null)
                {
                    AgentMoveToTarget();
                }
                if (attackTarget != null)
                {
                    if (Mathf.Abs(Vector3.Distance(attackTarget.transform.position, transform.position)) > 4f)
                    {
                        AgentAimAtAttackTarget();
                        //AgentGrenadeAttackTarget();
                        AgentShootAttackTarget();
                    }
                    else
                    {
                        AgentBayonetAttackTarget();
                    }
                }
            }
            cc.UpdateAnimator();
            cc.UpdateMotor();
            Debug.DrawRay(cc.shooting.CurrentWeapon.firespot.transform.position, cc.shooting.CurrentWeapon.firespot.transform.forward * shootRange, Color.black);

            //Debug.Log("MoveTarget = " + moveTarget.transform.position + " : Last Known Target: " + lastKnownMoveTarget.transform.position);
        }

        void AgentBayonetAttackTarget()
        {
            Debug.Log("Bayonet");
            cc.BayonetAttack();
        }

        void AgentCheckForAttackTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectRadius, playerLayerIndex);
            foreach (Collider nearbyObject in colliders)
            {
                vThirdPersonController targetcc = nearbyObject.gameObject.GetComponentInParent<vThirdPersonController>();
                if (cc.Team != targetcc.Team && !targetcc.isDead)
                {
                    this.attackTarget = targetcc.animator.GetBoneTransform(HumanBodyBones.Head).gameObject;
                    return;
                }
            }
        }

        void AgentWalk(bool value)
        {
            agent.isStopped = !value;
            cc.Walk(value);

            if(value)
            {
                cc.input.y = agent.desiredVelocity.z;
                cc.input.x = agent.desiredVelocity.x;
                if (agent.desiredVelocity.y > 0.25f)
                    cc.Jump();
            }
            else
            {
                cc.input.y = 0f;
                cc.input.x = 0f;
            }
        }
        void AgentGrenadeAttackTarget()
        {
            if(Mathf.Abs(Vector3.Distance(attackTarget.transform.position, transform.position)) <= grenadeRange)
            {
                Debug.Log("Attempting to nade");
                cc.Walk(false);
                StartCoroutine(cc.ThrowGrenadeForward());
            }
        }
        void AgentMoveToTarget()
        {
            //If we have a move target
            if(moveTarget != null)
            {
                //if movetarget has moved since last known pos, navigate to it
                if (moveTarget.transform.position != lastKnownMoveTargetPosition)
                {
                    agent.SetDestination(moveTarget.transform.position);
                    lastKnownMoveTargetPosition = moveTarget.transform.position;
                }
                
                //We are not at target, so perform walking
                if(!IsAgentAtDestination() && !isShooting)
                {
                    AgentWalk(true);
                }
                //If we are at the target, stop walking
                else
                {
                    AgentWalk(false);
                }
            }
            //If there is no target, also stop walking
            if(moveTarget == null)
            {
                AgentWalk(false);
            }
        }
        void AgentAimAtAttackTarget()
        {
            spine.LookAt(attackTarget.transform.position);
            spine.rotation *= Quaternion.Euler(Offset);

            //var targetRotation = Quaternion.LookRotation(attackTarget.transform.position - spine.transform.position) * Quaternion.Euler(Offset);
            //spine.rotation = Quaternion.Slerp(spine.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            //spine.rotation *= Quaternion.Euler(Offset);

            cc.shooting.CurrentWeapon.firespot.transform.LookAt(attackTarget.transform.position);

            //Rotate whole body if spine twists too much
            Vector3 vectorDifference = attackTarget.transform.position - transform.position;
            float angleBetween = Vector3.Angle(transform.forward, vectorDifference);
            if (angleBetween > 30f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vectorDifference), rotationSpeed * Time.deltaTime);
            }
        }

        void AgentShootAttackTarget()
        {
            if(cc.shooting.CurrentWeapon.currentAmmo <= 0)
            {
                StartCoroutine(cc.shooting.Reload());
                isShooting = false;
            }
            else if (canShoot && !cc.isReloading)
            {
                RaycastHit hit;

                //if (Physics.CapsuleCast(transform.position, transform.forward * shootRange, detectRadius, transform.forward, out hit, shootRange, playerLayerIndex, QueryTriggerInteraction.UseGlobal))

                if (Physics.Raycast(cc.shooting.CurrentWeapon.firespot.transform.position, cc.shooting.CurrentWeapon.firespot.transform.forward, out hit, shootRange))
                {
                    vThirdPersonController ccHit = hit.transform.gameObject.GetComponentInParent<vThirdPersonController>();
                    if (ccHit != null && !ccHit.isDead && ccHit.Team != cc.Team)
                    {
                        Vector3 directionOfShot = cc.shooting.CurrentWeapon.firespot.transform.transform.forward;
                        directionOfShot.x += Random.Range(-aimAccuracy, aimAccuracy);
                        directionOfShot.y += Random.Range(-aimAccuracy, aimAccuracy);
                        directionOfShot.z += Random.Range(-aimAccuracy, aimAccuracy);

                        cc.shooting.CurrentWeapon.firespot.transform.forward = directionOfShot;

                        isShooting = true;
                        cc.Shoot();
                    }
                    else if(ccHit != null && ccHit.isDead && ccHit.Team != cc.Team)
                    {
                        isShooting = false;
                        attackTarget = null;
                    }
                }
                cc.shooting.CurrentWeapon.muzzlespot.transform.localRotation = Quaternion.Euler(cc.shooting.CurrentWeapon.muzzleForward);
            }
        }
        bool IsAgentAtDestination()
        {
            // Check if we've reached the destination
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
