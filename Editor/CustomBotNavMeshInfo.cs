using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalBotsNavMeshProject.Editor
{
    public class CustomBotNavMeshInfo : MonoBehaviour
    {
        public struct CustomBotNavMeshInfoData
        {
            public string filePath { get; internal set; } = string.Empty;
            public string assetName { get; internal set; } = string.Empty;
            public bool IsPrefabEnabled { get; set; }

            public CustomBotNavMeshInfoData()
            {
                IsPrefabEnabled = true;
            }
        }

        [Tooltip("The name SelectableLevel.PlanetName.SelectableLevel.sceneName of the level that this navmesh is for. \n Its recommended you use selectableLevel if possible.")]
        [SerializeField]
        private string levelName = string.Empty;

        [Tooltip("The level that this navmesh is for. If this is null, levelName will be used instead.")]
        [SerializeField]
        private SelectableLevel? selectableLevel;

        [Tooltip("The root GameObject of the navmesh prefab.")]
        [SerializeField]
        private GameObject botNavMeshRoot = null!;

        public string GetLevelName()
        {
            if (selectableLevel != null)
            {
                return $"{selectableLevel.PlanetName}.{selectableLevel.sceneName}";
            }
            return levelName;
        }

        public GameObject GetBotNavMeshRoot()
        {
            return botNavMeshRoot;
        }
    }
}
