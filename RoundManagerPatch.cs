using HarmonyLib;
using LethalBots.Constants;
using LethalBotsNavMeshProject.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace LethalBotsNavMeshProject
{
    [HarmonyPatch(typeof(RoundManager))]
    public class RoundManagerPatch
    {
        /// <summary>
        /// Auto creates <see cref="NavMeshLink"/>s after the NavMesh is generated to allow bots 
        /// to use ladder <see cref="InteractTrigger"/>s around the map.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch("BakeDunGenNavMesh")]
        [HarmonyPostfix]
        public static void BakeDunGenNavMesh_Postfix(RoundManager __instance)
        {
            #if DEBUG
            // Debug materials
            if (blueMat == null)
            {
                blueMat = new Material(Shader.Find("HDRP/Lit"));
                blueMat.SetColor("_BaseColor", Color.blue);

                redMat = new Material(Shader.Find("HDRP/Lit"));
                redMat.SetColor("_BaseColor", Color.red);

                orangeMat = new Material(Shader.Find("HDRP/Lit"));
                orangeMat.SetColor("_BaseColor", new Color(1f, 0.5f, 0f));
            }
            #endif

            // Clear old NavMeshLinks
            ClearExistingAutoLinks();

            // Make sure we are allowed to auto generate links
            if (!Plugin.Config.AutoGenerateLadderLinks.Value)
            {
                Plugin.LogDebug("Auto-generation of ladder links is disabled via config.");
                return;
            }

            Plugin.LogDebug("Auto creating NavMeshLinks for ladders on the moon for bots.");
            #pragma warning disable CS0618 // Type or member is obsolete
            NavMeshLink[] navMeshLinks = Object.FindObjectsByType<NavMeshLink>(FindObjectsSortMode.None);
            InteractTrigger[] interactsTrigger = Object.FindObjectsByType<InteractTrigger>(FindObjectsSortMode.None);
            OffMeshLink[] offMeshLinks = Object.FindObjectsByType<OffMeshLink>(FindObjectsSortMode.None);
            ExtensionLadderItem[] extensionLadders = Object.FindObjectsByType<ExtensionLadderItem>(FindObjectsSortMode.None);
            for (int i = 0; i < interactsTrigger.Length; i++)
            {
                // Sanity check, make sure the ladder is valid
                InteractTrigger? ladder = interactsTrigger[i];
                if (ladder == null)
                {
                    continue;
                }

                // Make sure this is a ladder, not a ladder attached to a Extention Ladder, and that there isn't a OffMeshLink that already exists
                if (ladder.isLadder 
                    && ladder.ladderHorizontalPosition != null
                    && !extensionLadders.Any(x => x.ladderScript == ladder)
                    && !OffMeshLinkExists(__instance, ladder, navMeshLinks, offMeshLinks))
                {
                    Plugin.LogDebug($"Creating NavMeshLink for ladder {ladder} with name {ladder.name}");
                    GameObject linkObj = new GameObject("LadderNavMeshLink");
                    linkObj.transform.position = Vector3.zero;
                    linkObj.transform.rotation = Quaternion.identity;
                    linkObj.transform.localScale = Vector3.one; //ladder.transform.localScale;
                    linkObj.layer = LayerMask.NameToLayer("NavigationSurface");

                    Vector3 ladderTopPos = __instance.GetNavMeshPosition(ladder.topOfLadderPosition.position, __instance.navHit, Const.DISTANCE_NPCBODY_FROM_LADDER);
                    Vector3 ladderBottomPos = __instance.GetNavMeshPosition(ladder.bottomOfLadderPosition.position, __instance.navHit, Const.DISTANCE_NPCBODY_FROM_LADDER);
                    Plugin.LogDebug($"Nav Position Top: {ladderTopPos} and Bottom: {ladderBottomPos}");

                    NavMeshLink newLink = linkObj.AddComponent<NavMeshLink>();
                    newLink.agentTypeID = 0;
                    newLink.area = Const.LETHAL_BOT_ONLY_NAVAREA;
                    newLink.startPoint = ladderTopPos;
                    newLink.endPoint = ladderBottomPos;
                    newLink.bidirectional = true;
                    newLink.autoUpdate = true;
                    newLink.width = 0f;
                    newLink.UpdateLink();

                    NavMeshLinkHelper linkHelper = ladder.gameObject.AddComponent<NavMeshLinkHelper>();
                    linkHelper.link = newLink;
                    linkHelper.ladder = ladder;
                    linkHelper.topTransform = ladder.topOfLadderPosition;
                    linkHelper.bottomTransform = ladder.bottomOfLadderPosition;
                }
            }
            #pragma warning restore CS0618 // Type or member is obsolete
        }

        #pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>
        /// Helper function that checks if the given <paramref name="ladder"/> already has a OffMeshLink
        /// </summary>
        /// <param name="ladder"></param>
        /// <param name="navMeshLinks"></param>
        /// <param name="offMeshLinks"></param>
        /// <returns></returns>
        private static bool OffMeshLinkExists(RoundManager instanceRM, InteractTrigger ladder, NavMeshLink[] navMeshLinks, OffMeshLink[] offMeshLinks)
        {
            // Cache the top and bottom of the ladder
            Vector3 ladderTopPos = instanceRM.GetNavMeshPosition(ladder.topOfLadderPosition.position, instanceRM.navHit, Const.DISTANCE_NPCBODY_FROM_LADDER);
            Vector3 ladderBottomPos = instanceRM.GetNavMeshPosition(ladder.bottomOfLadderPosition.position, instanceRM.navHit, Const.DISTANCE_NPCBODY_FROM_LADDER);
            for (int i = 0; i < navMeshLinks.Length; i++)
            {
                // Make sure the link is valid and only the default agent type
                NavMeshLink navMeshLink = navMeshLinks[i];
                if (navMeshLink != null 
                    && navMeshLink.agentTypeID == 0)
                {
                    // Check if the link connects the stand and end point of the ladder
                    Vector3 startPoint = navMeshLink.transform.TransformPoint(navMeshLink.startPoint);
                    Vector3 endPoint = navMeshLink.transform.TransformPoint(navMeshLink.endPoint);
                    if ((ladderTopPos - startPoint).sqrMagnitude < Const.DISTANCE_NPCBODY_FROM_LADDER * Const.DISTANCE_NPCBODY_FROM_LADDER
                        && (ladderBottomPos - endPoint).sqrMagnitude < Const.DISTANCE_NPCBODY_FROM_LADDER * Const.DISTANCE_NPCBODY_FROM_LADDER)
                    {
                        return true;
                    }
                    else if ((ladderTopPos - endPoint).sqrMagnitude < Const.DISTANCE_NPCBODY_FROM_LADDER * Const.DISTANCE_NPCBODY_FROM_LADDER
                        && (ladderBottomPos - startPoint).sqrMagnitude < Const.DISTANCE_NPCBODY_FROM_LADDER * Const.DISTANCE_NPCBODY_FROM_LADDER)
                    {
                        return true;
                    }
                }
            }

            // Now check OffMeshLinks
            for (int i = 0; i < offMeshLinks.Length; i++)
            {
                // Make sure the link is valid
                OffMeshLink offMeshLink = offMeshLinks[i];
                if (offMeshLink != null)
                {
                    // Check if the link connects the stand and end point of the ladder
                    Vector3 startPosition = offMeshLink.startTransform.position;
                    Vector3 endPosition = offMeshLink.endTransform.position;
                    if ((ladderTopPos - startPosition).sqrMagnitude < Const.DISTANCE_NPCBODY_FROM_LADDER * Const.DISTANCE_NPCBODY_FROM_LADDER
                        && (ladderBottomPos - endPosition).sqrMagnitude < Const.DISTANCE_NPCBODY_FROM_LADDER * Const.DISTANCE_NPCBODY_FROM_LADDER)
                    {
                        return true;
                    }
                    else if ((ladderTopPos - endPosition).sqrMagnitude < Const.DISTANCE_NPCBODY_FROM_LADDER * Const.DISTANCE_NPCBODY_FROM_LADDER
                        && (ladderBottomPos - startPosition).sqrMagnitude < Const.DISTANCE_NPCBODY_FROM_LADDER * Const.DISTANCE_NPCBODY_FROM_LADDER)
                    {
                        return true;
                    }
                }
            }

            // No link here, lets create our own
            return false;
        }
        #pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Helper function that deletes all automatically generated links
        /// </summary>
        public static void ClearExistingAutoLinks()
        {
            // Find all helpers currently in the scene
            NavMeshLinkHelper[] existingHelpers = Object.FindObjectsByType<NavMeshLinkHelper>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < existingHelpers.Length; i++)
            {
                // We destroy the component. 
                // IMPORTANT: This will trigger the helper's OnDestroy(), 
                // which handles deleting the link GameObject.
                NavMeshLinkHelper? helper = existingHelpers[i];
                if (helper != null)
                    Object.DestroyImmediate(helper);
            }

            Plugin.LogDebug($"Cleared {existingHelpers.Length} existing auto-generated NavMeshLinks.");
        }

        #if DEBUG
        private static readonly List<GameObject> debugVisuals = new List<GameObject>();
        private static Material blueMat;
        private static Material redMat;
        private static Material orangeMat;

        public static void ClearDebugVisuals()
        {
            foreach (var go in debugVisuals)
            {
                if (go != null)
                    Object.Destroy(go);
            }
            debugVisuals.Clear();
        }

        public static IEnumerator VisualizeAllNavMeshLinks()
        {
            ClearDebugVisuals();

            NavMeshLink[] links = Object.FindObjectsOfType<NavMeshLink>();

            const int batchSize = 200;   // how many primitives per frame
            int processed = 0;

            foreach (var link in links)
            {
                Vector3 start = link.transform.TransformPoint(link.startPoint);
                Vector3 end = link.transform.TransformPoint(link.endPoint);

                // Container object
                var go = new GameObject($"DebugNavMeshLink_{debugVisuals.Count}");
                //go.transform.SetParent(surface.transform);
                //go.layer = surface.gameObject.layer;

                debugVisuals.Add(go);

                GameObject startSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                startSphere.name = "StartSphere";
                startSphere.transform.SetParent(go.transform);
                startSphere.transform.position = start;
                startSphere.transform.localScale = Vector3.one * 0.25f;
                startSphere.GetComponent<MeshRenderer>().material = blueMat;

                processed++;
                if (processed >= batchSize)
                {
                    processed = 0;
                    yield return null;
                }

                GameObject endSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                endSphere.name = "EndSphere";
                endSphere.transform.SetParent(go.transform);
                endSphere.transform.position = end;
                endSphere.transform.localScale = Vector3.one * 0.25f;
                endSphere.GetComponent<MeshRenderer>().material = redMat;

                processed++;
                if (processed >= batchSize)
                {
                    processed = 0;
                    yield return null;
                }

                GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cyl.name = "LinkCylinder";
                cyl.transform.SetParent(go.transform);

                Vector3 mid = (start + end) * 0.5f;
                cyl.transform.position = mid;

                float dist = Vector3.Distance(start, end);
                cyl.transform.localScale = new Vector3(0.12f, dist * 0.5f, 0.12f);
                cyl.transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start);
                cyl.GetComponent<MeshRenderer>().material = orangeMat;

                processed++;
                if (processed >= batchSize)
                {
                    processed = 0;
                    yield return null;
                }
            }
        }
        public static void DebugVisualNavMeshLinks()
        {
            RoundManager.Instance.StartCoroutine(VisualizeAllNavMeshLinks());
        }
        #endif
    }
}
