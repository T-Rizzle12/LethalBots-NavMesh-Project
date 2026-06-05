using System;
using System.Collections.Generic;
using System.Text;

namespace LethalBotsNavMeshProject.MoonNavMeshes
{
    public class EmbrionNavMesh : MoonNavMesh
    {
        internal EmbrionNavMesh(UnityEngine.GameObject prefab)
        {
            this.navPrefab = prefab;
        }

        public override bool IsPrefabEnabled()
        {
            return Plugin.Config.EnableEmbrionNav;
        }
    }
}
