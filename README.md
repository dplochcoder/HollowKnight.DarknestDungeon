# HollowKnight.DarknestDungeon

Extra resources for the Darknest Dungeon plando

## Build process

  1. Bootstrap with a standard C# project build to generate DLLs
  1. Run DataUpdater console app as part of solution build, which copies the DLLs into Unity/Assets/Assemblies
  1. Open the DarknestDungeon/Unity project in Unity; Build Asset Bundles
  1. Do the standard C# project build again to embed AssetBundles in the mod DLL

Asset bundles and DLLs are not stored in git. You will need to add the SFCoreUnity.dll
dependency into Unity/Assets/Assemblies first (and rename it to SFCore.dll);
this only needs to be done once, or if there's an update to SFCore.'
