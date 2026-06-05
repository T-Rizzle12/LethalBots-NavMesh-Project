using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalBotsNavMeshProject.MoonNavMeshes
{
    public class VowNavMesh : MoonNavMesh
    {
        internal VowNavMesh(GameObject prefab)
        {
            this.navPrefab = prefab;
        }

        public override bool IsPrefabEnabled()
        {
            return Plugin.Config.EnableVowNav;
        }
    }
}
