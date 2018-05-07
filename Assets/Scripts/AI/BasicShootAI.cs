using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShootAI : BasicShoot
{
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        RaycastHit cast;
        //Fire the gun, and allow fire only after slight delay + animator is not bolting the gun on attack layer
        if (Time.time >= timeToNextFire && Physics.Raycast(firespot.transform.position, firespot.transform.forward, out cast, range) && cast.transform.root.name.Contains(enemyString))
        {
            //Calculate minimal delay and disable crosshair
            timeToNextFire = Time.time + 1f / fireAnimDelay;

            //Fire the raycast
            Shoot();
        }
    }
}
