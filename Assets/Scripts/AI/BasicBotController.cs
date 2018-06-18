using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
        public Vector3 Offset;

        public GameObject moveTarget;
        public GameObject attackTarget;
        
        private NavMeshAgent agent;

        private GameObject originalMoveTarget;
        
        private Transform lastKnownMoveTarget;
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
            if(moveTarget)
                agent.SetDestination(moveTarget.transform.position);

            if(moveTarget != null)
            {
                originalMoveTarget = moveTarget;
                lastKnownMoveTarget = moveTarget.transform;
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
                    AgentAimAtAttackTarget();
                    AgentShootAttackTarget();
                }
            }
            Debug.DrawRay(cc.basicShoot.firespot.transform.position, cc.basicShoot.firespot.transform.forward * 100f, Color.black);
        }
        void AgentCheckForAttackTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectRadius, playerLayerIndex);
            foreach (Collider nearbyObject in colliders)
            {
                vThirdPersonController targetcc = nearbyObject.gameObject.GetComponentInParent<vThirdPersonController>();
                if (cc.Team != targetcc.Team && !targetcc.isDead)
                {
                    this.attackTarget = targetcc.animator.GetBoneTransform(HumanBodyBones.Spine).gameObject;
                    return;
                }
            }
        }

        void AgentWalk(bool value)
        {
            agent.isStopped = !value;
            cc.Walk(value);
            cc.input.y = value ? 1f : 0f;
        }
        void AgentMoveToTarget()
        {
            //If we have a move target
            if(moveTarget != null)
            {
                //if movetarget has moved since last known pos, navigate to it
                if (moveTarget.transform.position != lastKnownMoveTarget.position)
                    agent.SetDestination(moveTarget.transform.position);
                
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

            cc.UpdateAnimator();
            cc.UpdateMotor();
        }
        void AgentAimAtAttackTarget()
        {
            spine.LookAt(attackTarget.transform.position);
            spine.rotation *= Quaternion.Euler(Offset);

            //var targetRotation = Quaternion.LookRotation(attackTarget.transform.position - spine.transform.position) * Quaternion.Euler(Offset);
            //spine.rotation = Quaternion.Slerp(spine.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            //spine.rotation *= Quaternion.Euler(Offset);

            cc.basicShoot.firespot.transform.LookAt(attackTarget.transform.position);
        }

        void AgentShootAttackTarget()
        {
            if(cc.basicShoot.currentAmmo <= 0)
            {
                StartCoroutine(cc.basicShoot.Reload());
                isShooting = false;
            }
            else if (canShoot && !cc.isReloading)
            {
                RaycastHit hit;
                if (Physics.Raycast(cc.basicShoot.firespot.transform.position, cc.basicShoot.firespot.transform.forward, out hit, shootRange))
                {
                    vThirdPersonController ccHit = hit.transform.gameObject.GetComponentInParent<vThirdPersonController>();
                    if (ccHit != null && !ccHit.isDead && ccHit.Team != cc.Team)
                    {
                        isShooting = true;
                        cc.Shoot();
                    }
                    else if(ccHit != null && ccHit.isDead && ccHit.Team != cc.Team)
                    {
                        isShooting = false;
                        attackTarget = null;
                    }
                }
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
