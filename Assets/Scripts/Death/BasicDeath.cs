using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDeath : MonoBehaviour {

    private Animator anim;
    private Rigidbody rb;

    private int deathIndex;
    private int dieHash = Animator.StringToHash("Die");
    private bool dead = false;
    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        deathIndex = Random.Range(0, 6);
    }
    public void Die()
    {
        if (!dead)
        {
            dead = true;
            switch(deathIndex)
            {
                case 1:
                    anim.SetFloat("DeathAnim", 0f);
                    break;
                case 2:
                    anim.SetFloat("DeathAnim", 0.25f);
                    break;
                case 3:
                    anim.SetFloat("DeathAnim", 0.5f);
                    break;
                case 4:
                    anim.SetFloat("DeathAnim", 0.75f);
                    break;
                case 5:
                    anim.SetFloat("DeathAnim", 1f);
                    break;

            }
            anim.SetTrigger(dieHash);
        }
    }
}
