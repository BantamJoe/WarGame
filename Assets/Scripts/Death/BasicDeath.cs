using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDeath : MonoBehaviour {

    private Animator anim;
    private Rigidbody rb;

    private int dieHash = Animator.StringToHash("Die");
    private bool dead = false;
    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
    }
    public void Die()
    {
        if(!dead)
        {
            dead = true;
            anim.SetTrigger(dieHash);
        }
    }
}
