using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicDeathAI : MonoBehaviour
{
    private Animator anim;
    private NavMeshAgent agent;

    private int dieHash = Animator.StringToHash("Die");
    private bool dead = false;

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        agent = (this.gameObject.GetComponent<NavMeshAgent>()) != null ? this.gameObject.GetComponent<NavMeshAgent>() : this.gameObject.AddComponent<NavMeshAgent>();
    }

    public void Die()
    {
        if (!dead)
        {
            dead = true;
            anim.SetTrigger(dieHash);
            agent.enabled = false;
        }
    }
}
