using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicLookAt : MonoBehaviour {

    public GameObject target;

    private Animator anim;
    private NavMeshAgent agent;

    // Use this for initialization
    void Start ()
    {
        anim = this.gameObject.GetComponent<Animator>();
        agent = (this.gameObject.GetComponent<NavMeshAgent>()) != null ? this.gameObject.GetComponent<NavMeshAgent>() : this.gameObject.AddComponent<NavMeshAgent>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.LookAt(target.transform);
	}
}
