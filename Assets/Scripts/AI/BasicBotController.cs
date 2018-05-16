using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Invector.CharacterController
{
    public class BasicBotController : MonoBehaviour
    {
        public GameObject target;
        public float shootRange = 30f;
        public float stoppingDistance = 3f;
        
        private NavMeshAgent agent;
        private vThirdPersonController cc;
        private bool isDead = false;

        // Use this for initialization
        void Start()
        {
            agent = GetComponentInChildren<NavMeshAgent>();
            cc = GetComponent<vThirdPersonController>();

            if (cc != null)
                cc.Init();
            if (agent == null)
                Debug.LogError("Navmesh agent missing on bot");
           
        }

        // Update is called once per frame
        void Update()
        {
            if(cc.isDead && !isDead)
            {
                isDead = true;
                cc.input.y = 0f;
                cc.Walk(false);   
                //cc.UpdateMotor();

                agent.enabled = false;
                
                //Look into removing the capsule a better way.
                cc._capsuleCollider.enabled = false;
            }
            if (!isDead && agent.enabled)
            {
                AgentMove();
                AgentShoot();
            }
        }

        void AgentMove()
        {
            agent.SetDestination(target.transform.position);

            cc.Walk(true);
            cc.input.y = 1f;

            cc.UpdateAnimator();
            cc.UpdateMotor();
        }

        void AgentShoot()
        {
            RaycastHit hit;
            if(Physics.Raycast(cc.weapon.transform.GetChild(0).transform.position, cc.weapon.transform.GetChild(0).transform.forward, out hit, shootRange))
            {
                vThirdPersonController ccHit = hit.transform.gameObject.GetComponentInParent<vThirdPersonController>();
                if (ccHit != null && ccHit.Team != cc.Team)
                {
                    cc.Shoot();
                }
            }
        }
    }
}
