using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace LethalBotsNavMeshProject.MoonNavMeshes
{
    /// <summary>
    /// An abstract class used to represent a navmesh prefab with NavMesh improvements for the bots
    /// </summary>
    public abstract class MoonNavMesh
    {
        protected GameObject? navPrefab;

        /// <summary>
        /// Should this prefab be used?
        /// </summary>
        /// <remarks>
        /// This exists so you could use a config option for example to block the prefab from spawning!
        /// </remarks>
        /// <returns><see langword="true"/> if we should use the prefab; otherwise <see langword="false"/></returns>
        public virtual bool IsPrefabEnabled()
        {
            return true;
        }

        /// <summary>
        /// Returns the <see cref="navPrefab"/> associated with this object.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObject? GetNavPrefab()
        {
            return navPrefab;
        }
    }
}
