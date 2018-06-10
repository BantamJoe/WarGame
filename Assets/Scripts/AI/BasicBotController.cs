﻿using System.Collections;
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
        public Vector3 Offset;
        public GameObject target;
        
        private NavMeshAgent agent;
        private GameObject originalTarget;
        private Transform spine;

        private vThirdPersonController cc;
        private BasicShoot bs;

        private bool isDead = false;

        // Use this for initialization
        void Start()
        {
            agent = GetComponentInChildren<NavMeshAgent>();
            cc = GetComponent<vThirdPersonController>();
            bs = cc.weapon.GetComponent<BasicShoot>();

            if (cc != null)
                cc.Init();
            if (agent == null)
                Debug.LogError("Navmesh agent missing on bot");
            if(target)
                agent.SetDestination(target.transform.position);

            spine = cc.animator.GetBoneTransform(HumanBodyBones.Spine);
            originalTarget = target;
        }
        
        void LateUpdate()
        {
            //If the character controller is dead, kill the bot controller
            if(cc.isDead && !isDead)
            {
                isDead = true;
                cc.input.y = 0f;
                cc.Walk(false);   

                agent.enabled = false;
                
                //Look into removing the capsule a better way.
                cc._capsuleCollider.enabled = false;
            }
            if (!isDead && agent.enabled && target)
            {
                //Get rid of this loop immediately
                if(target.GetComponent<vThirdPersonController>() != null && target.GetComponent<vThirdPersonController>().isDead)
                {
                    target = originalTarget;
                }

                AgentMoveToTarget();
                //AgentRotate();
                AgentShoot();
                AgentAimAtTarget();
            }
        }

        void AgentMoveToTarget()
        {
            //If we have a target
            if(target != null)
            {
                //Attempt to navigate to it
                agent.SetDestination(target.transform.position);
                
                //We are not at target, so perform walking
                if(!IsAgentAtDestination())
                {
                    cc.Walk(true);
                    cc.input.y = 1f;
                }
                //If we are at the target, stop walking
                else
                {
                    cc.Walk(false);
                    cc.input.y = 0f;
                }
            }
            //If there is no target, also stop walking
            if(target == null)
            {
                cc.Walk(false);
                cc.input.y = 0f;
            }

            cc.UpdateAnimator();
            cc.UpdateMotor();
        }
        void AgentAimAtTarget()
        {
            spine.LookAt(target.transform.position);
            spine.rotation *= Quaternion.Euler(Offset);

            bs.firespot.transform.LookAt(target.transform.position);
            Debug.DrawRay(bs.firespot.transform.position, bs.firespot.transform.forward * 100f,Color.black);
            
        }
        void AgentRotate()
        {
            cc.gameObject.transform.LookAt(target.transform );
        }

        void AgentShoot()
        {
            cc.BotNeedToReload();
            if (canShoot)
            {
                RaycastHit hit;
                if (Physics.Raycast(cc.weapon.transform.GetChild(0).transform.position, cc.weapon.transform.GetChild(0).transform.forward, out hit, shootRange))
                {
                    vThirdPersonController ccHit = hit.transform.gameObject.GetComponentInParent<vThirdPersonController>();
                    if (ccHit != null && !ccHit.isDead && ccHit.Team != cc.Team)
                    {
                        cc.Shoot();
                    }
                }
            }
        }
        public void AgentViewConeTarget(GameObject target)
        {
            if(agent.enabled)
            {
                this.target = target.GetComponent<vThirdPersonController>().animator.GetBoneTransform(HumanBodyBones.Spine).gameObject;
            }
        }
        public void AgentViewConeTargetLost(GameObject target)
        {
            if (agent.enabled)
            {
                if (target == this.target)
                {
                    this.target = null;
                    Debug.Log("Target lost");
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
