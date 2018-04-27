using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicFollowAI : MonoBehaviour {

    public GameObject target;
    public float stoppingDistance = 3f;

    private Animator anim;
    private NavMeshAgent agent;

	// Use this for initialization
	void Start ()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(target.transform.position);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(agent.enabled)
        {
            //If beyond stoppingDistance, continue to the target
            if (Vector3.Distance(target.transform.position, this.transform.position) > stoppingDistance)
            {
                agent.SetDestination(target.transform.position);
                anim.SetFloat("InputVertical", 1f);
            }
            else
            {
                agent.SetDestination(transform.position);
                anim.SetFloat("InputVertical", 0f);
            }
        }
    }
}
