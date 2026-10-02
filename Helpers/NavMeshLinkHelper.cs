using LethalBots.Constants;
using NavMeshLib;
using System.Collections.Generic;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace LethalBotsNavMeshProject.Helpers
{
    /// <summary>
    /// Helper class dedicated to updating an mantaining auto created NavMeshLinks
    /// </summary>
    internal class NavMeshLinkHelper : MonoBehaviour
    {
        // Public
        public NavMeshLink? link = null!;
        public InteractTrigger? ladder = null!;
        public Transform? topTransform = null!;
        public Transform? bottomTransform = null!;
        public float UpdateInterval = 0.5f; // How often to check
        public int AgentIndex = CustomAgentManager.DEFAULT_AGENT_ID;
        public int AreaMask = NavMesh.AllAreas;

        // Private
        private float updateTimer = 0f;

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer > UpdateInterval)
            {
                updateTimer = 0f;
                SyncPoints();
            }
        }

        private void SyncPoints()
        {
            if (link != null)
            {
                // Check if the ladder is active
                bool isLadderActive = ladder == null || ladder.isActiveAndEnabled;
                if (link.enabled != isLadderActive)
                {
                    link.enabled = isLadderActive;
                }

                // Only update the ladder if its active
                // NOTE: If we were not given a ladder object, then we don't care
                if (!isLadderActive) return;

                if (topTransform == null)
                {
                    if (ladder == null) return;
                    topTransform = ladder.topOfLadderPosition;
                }
                if (bottomTransform == null)
                {
                    if (ladder == null) return;
                    bottomTransform = ladder.bottomOfLadderPosition;
                }

                NavMeshQueryFilter queryFilter = new NavMeshQueryFilter { agentTypeID = AgentIndex, areaMask = AreaMask };
                RoundManager instanceRM = RoundManager.Instance;
                Vector3 ladderTopPos = instanceRM.GetNavMeshPosition(topTransform.position, queryFilter, instanceRM.navHit, Const.DISTANCE_NPCBODY_FROM_LADDER);
                Vector3 ladderBottomPos = instanceRM.GetNavMeshPosition(bottomTransform.position, queryFilter, instanceRM.navHit, Const.DISTANCE_NPCBODY_FROM_LADDER);
                link.startPoint = ladderTopPos;
                link.endPoint = ladderBottomPos;
                //link.UpdateLink(); // Auto Update should handle this for us, it also makes sure we don't needlessly spam update calls
            }
        }

        private void OnDestroy()
        {
            if (link != null)
            {
                Object.DestroyImmediate(link.gameObject);
            }
        }
    }
}
