using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCharacter : Photon.MonoBehaviour
{
    private Vector3 realPos = Vector3.zero;
    private Quaternion realRotation = Quaternion.identity;
    private Animator anim;

    private float lastUpdateTime = 0;
    // Use this for initialization
    void Start()
    {
        anim = this.GetComponent<Animator>();
        if(anim == null)
        {
            Debug.LogError("There was no animator on this character.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //check to see if this character is mine
        if (!this.photonView.isMine) //and if this is true, it is not
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
            /*stream.SendNext(anim.GetBool("IsStrafing"));
            stream.SendNext(anim.GetBool("IsGrounded"));
            stream.SendNext(anim.GetFloat ("GroundDistance"));
            stream.SendNext(anim.GetFloat("VerticalVelocity"));
            stream.SendNext(anim.GetFloat("InputHorizontal"));
            stream.SendNext(anim.GetFloat("InputVertical"));*/
        }
        else //this is another player and we need to receive their position from a big ago
        {
            realPos = (Vector3)stream.ReceiveNext();
            realRotation = (Quaternion)stream.ReceiveNext();
            /*anim.SetBool("IsStrafing", (bool)stream.ReceiveNext());
            anim.SetBool("IsGrounded", (bool)stream.ReceiveNext());
            anim.SetFloat("GroundDistance", (float)stream.ReceiveNext());
            anim.SetFloat("VerticalVelocity", (float)stream.ReceiveNext());
            anim.SetFloat("InputHorizontal", (float)stream.ReceiveNext());
            anim.SetFloat("InputVertical", (float)stream.ReceiveNext());*/
            lastUpdateTime = 0;
        }
    }
}
