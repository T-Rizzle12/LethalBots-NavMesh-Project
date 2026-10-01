# Changelog

## 2.0.0 - 2026-10-01
This update includes some changes to the backend of the mod to use NavMeshLib.<br/>

- Added support to allow custom NavMesh objects that only affect the bot when it's driving a cruiser
- Added NavMesh improvements for Dine and Rend
- Updated the PrefabManager to allow moon makers to add their own custom NavMesh changes for the bots. <br/> 
We check for an assetbundle with the extension .lethalbotsnavmesh and find any GameObject with the component CustomBotNavMeshInfo. <br/>
The Lethal Bots NavMesh project will automatically load and unload these bundles at runtime. <br/>
- Fixed some edge cases with OffMeshLinks and auto ladder generation that could cause it to fail.

## 1.3.0 - 2026-07-22
NavMeshLinks will now automatically be generated for all ladders on a level.<br/>
This works for all custom moon and custom interiors.<br/>
ONLY BOTS CAN USE THE AUTO GENERATED LINKS.<br/>
This can be disabled as desired.

## 1.2.2 - 2026-07-17
Fixed debug logs not respecting the value of EnableDebugLog

## 1.2.1 - 2026-07-3
- Made a minor optimization when rebaking NavMeshSurfaces to use async rebaking when possible

## 1.2.0 - 2026-06-24
- Added NavMesh improvements for Titan
- Added support for custom NavMeshSurfaces attached to NavPrefabs

## 1.1.0 - 2026-06-15
- Added NavMesh improvements for Artifice

## 1.0.0 - 2026-06-5
- Initial release