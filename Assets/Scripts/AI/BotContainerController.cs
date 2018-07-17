using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.CharacterController
{
    public class BotContainerController : MonoBehaviour
    {
        public string alliedTeamName = "British";
        public string axisTeamName = "German";

        private Dictionary<vThirdPersonController, BotInfo> alliedBots;
        private Dictionary<vThirdPersonController, BotInfo> axisBots;
    
        // Use this for initialization
        void Start()
        {
            PopulateBotLists();
        }
        
        void PopulateBotLists()
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                vThirdPersonController bot = transform.GetChild(i).GetComponent<vThirdPersonController>();
                if (bot.Team == alliedTeamName)
                {
                    //alliedBots.Add(bot);
                }
                    
                if (bot.Team == axisTeamName)
                {
                    //axisBots.Add(bot);
                }
                else
                    Debug.LogWarning("A bot in the BOT container has a team NOT LISTED");
            }
        }
        void FindNearestEnemy(vThirdPersonController bot)
        {

        }
    }
}

public class BotInfo : MonoBehaviour
{
    //public float distance
}

