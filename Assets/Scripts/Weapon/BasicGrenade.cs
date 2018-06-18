using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.CharacterController
{
    public class BasicGrenade : MonoBehaviour
    {
        public float damage = 120f;
        public float radius = 5f;
        public float delay = 4f;
        public Rigidbody rb;

        public GameObject explosionEffect;

        private float countdown;
        private bool exploded = false;
        private int playerLayerIndex = 1 << 8;

        // Use this for initialization
        void Start()
        {
            countdown = delay;
            rb = GetComponent<Rigidbody>() == null ? this.gameObject.AddComponent<Rigidbody>() : GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f && !exploded)
            {
                Explode();
                exploded = true;
            }
        }
        void Explode()
        {
            Destroy(Instantiate(explosionEffect, transform.position, Quaternion.LookRotation(Vector3.up)), 10f);

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius, playerLayerIndex);
            foreach (Collider nearbyObject in colliders)
            {
                vThirdPersonController targetcc = nearbyObject.gameObject.GetComponentInParent<vThirdPersonController>();
                targetcc.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}

