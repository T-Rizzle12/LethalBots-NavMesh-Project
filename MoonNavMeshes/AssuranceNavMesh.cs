using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalBotsNavMeshProject.MoonNavMeshes
{
    public class AssuranceNavMesh : MoonNavMesh
    {
        internal AssuranceNavMesh(GameObject prefab)
        {
            this.navPrefab = prefab;
        }

        public override bool IsPrefabEnabled()
        {
            return Plugin.Config.EnableAssuranceNav;
        }
    }
}
