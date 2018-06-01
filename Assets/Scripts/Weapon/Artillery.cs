using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artillery : MonoBehaviour {

    public float thrust = 50f;
    public ParticleSystem explosion;

    private Rigidbody rb;
    private CapsuleCollider cap;

	// Use this for initialization
	void Awake () {
        cap = gameObject.GetComponent<CapsuleCollider>();
        rb = gameObject.AddComponent<Rigidbody>();
        rb.drag = 1f;
        rb.AddForce(gameObject.transform.up * thrust);
	}

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(Instantiate(explosion, gameObject.transform.position, Quaternion.Euler(Vector3.up)),3f);
        Destroy(this.gameObject,0.5f);
    }
}
