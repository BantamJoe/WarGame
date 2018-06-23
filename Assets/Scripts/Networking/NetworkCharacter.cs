using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCharacter : Photon.MonoBehaviour
{
    private Vector3 realPos = Vector3.zero;
    private Quaternion realRotation = Quaternion.identity;

    private float lastUpdateTime = 0;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //check to see if this character is mine
        if (!this.photonView.isMine)
        {
            lastUpdateTime += Time.deltaTime;
            this.transform.position = Vector3.Lerp(this.transform.position, realPos, 0.1f);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, realRotation, 0.1f);
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //this is us writing to the photon view, so send our position to the network
        if (stream.isWriting)
        {
            stream.SendNext(this.transform.position);
            stream.SendNext(this.transform.rotation);
        }
        else //this is another player and we need to receive their position from a big ago
        {
            realPos = (Vector3)stream.ReceiveNext();
            realRotation = (Quaternion)stream.ReceiveNext();
            lastUpdateTime = 0;
        }
    }
}
