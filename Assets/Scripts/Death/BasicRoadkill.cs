using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRoadkill : MonoBehaviour {

	void OnCollisionEnter(Collision obj)
    {
        if(obj.gameObject.name.Contains("MarkV"))
        {
            transform.root.GetComponent<BasicDeathAI>().Die();
        }
    }
}
