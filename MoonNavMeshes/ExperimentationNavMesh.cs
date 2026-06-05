using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalBotsNavMeshProject.MoonNavMeshes
{
    public class ExperimentationNavMesh : MoonNavMesh
    {
        internal ExperimentationNavMesh(GameObject prefab)
        {
            this.navPrefab = prefab;
        }

        public override bool IsPrefabEnabled()
        {
            return Plugin.Config.EnableExperimentationNav;
        }
    }
}
