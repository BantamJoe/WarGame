using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.CharacterController
{
    public class BasicArtilleryShell : MonoBehaviour
    {
        public float damage = 120f;
        public float radius = 5f;
        public Rigidbody rb;

        public GameObject explosionEffect;

        private BoxCollider col;
        private bool exploded = false;
        private int playerLayerIndex = 1 << 8;

        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody>() == null ? this.gameObject.AddComponent<Rigidbody>() : GetComponent<Rigidbody>();
            col = GetComponent<BoxCollider>() == null ? this.gameObject.AddComponent<BoxCollider>() : GetComponent<BoxCollider>();

            col.isTrigger = true;
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
        private void OnTriggerEnter(Collider other)
        {
            if(!exploded)
                Explode();
        }
    }
}

