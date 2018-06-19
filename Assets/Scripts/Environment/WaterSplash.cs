using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplash : MonoBehaviour {

    public GameObject waterSplashEffect;

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == 1 << 8)
            Destroy(Instantiate(waterSplashEffect, other.transform.position, Quaternion.LookRotation(Vector3.up)), 1f);
    }
}
