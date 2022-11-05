using ItemChanger;

namespace DarknestDungeon.IC
{
    // A custom module to hook deployers from deployers.json.
    //
    // We do it this way to support patching existing saves, in order to fix bugs.
    // If we used ItemChanger deployers, we'd have to fix them at save creation time.
    public class DeployersModule : ItemChanger.Modules.Module
    {
        public override void Initialize()
        {
            foreach (var map in Data.Deployers.Data.Values)
            {
                foreach (var deployer in map.Values)
                {
                    Events.AddSceneChangeEdit(deployer.SceneName, deployer.OnSceneChange);
                }
            }
        }

        public override void Unload()
        {
            foreach (var map in Data.Deployers.Data.Values)
            {
                foreach (var deployer in map.Values)
                {
                    Events.RemoveSceneChangeEdit(deployer.SceneName, deployer.OnSceneChange);
                }
            }
        }
    }
}
