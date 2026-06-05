using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalBotsNavMeshProject.MoonNavMeshes
{
    internal class OffenceNavMesh : MoonNavMesh
    {
        internal OffenceNavMesh(GameObject prefab)
        {
            this.navPrefab = prefab;
        }

        public override bool IsPrefabEnabled()
        {
            return Plugin.Config.EnableOffenceNav;
        }
    }
}
