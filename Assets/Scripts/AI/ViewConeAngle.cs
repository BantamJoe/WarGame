using UnityEngine;

namespace Invector.CharacterController
{
    public class ViewConeAngle : MonoBehaviour
    {
        private vThirdPersonController cc;
        public GameObject botController;

        private void Start()
        {
            cc = GetComponentInParent<vThirdPersonController>();
            
            if(cc == null)
            {
                Debug.LogWarning("No character controller attached to view cone");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(!cc.isDead)
            {
                vThirdPersonController targetcc = other.gameObject.GetComponentInParent<vThirdPersonController>();

                if (targetcc != null)
                {
                    if (botController != null && cc.Team != targetcc.Team && !targetcc.isDead)
                        botController.SendMessage("AgentViewConeTarget", targetcc.gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        /*
        private void OnTriggerExit(Collider other)
        {
            if(!cc.isDead)
            {
                vThirdPersonController targetcc = other.gameObject.GetComponentInParent<vThirdPersonController>();

                if (targetcc != null)
                {
                    if (botController != null && cc.Team != targetcc.Team && !targetcc.isDead)
                        botController.SendMessage("AgentViewConeTargetLost", targetcc.gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        */
    }
}
