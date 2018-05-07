using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HealthManager : MonoBehaviour
{
    public float health = 10f;

    private Animator anim;
    private NavMeshAgent agent;

    private int dieHash = Animator.StringToHash("Die");
    private bool dead = false;

    private void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        agent = this.gameObject.GetComponent<NavMeshAgent>();
    }

    private void LateUpdate()
    {
        //are we dead yet?
        if(health <= 0f && !dead)
        {
            Die();
        }
    }

    /// <summary>
    /// Kill character by making their flag say true, and play death animation.
    /// If there is a navmesh agent, disable it.
    /// </summary>
    public void Die()
    {
        if (!dead)
        {
            dead = true;
            anim.SetTrigger(dieHash);
            //if this character has a nav mesh component we want to disable it
            if (agent)
            {
                agent.enabled = false;
            }            
        }
    }

    /// <summary>
    /// Message handle for when a character is shot.
    /// Applies damage to health.
    /// </summary>
    /// <param name="damageDone"></param>
    public void Shot(float damageDone)
    {
        health -= damageDone;
    }
}
