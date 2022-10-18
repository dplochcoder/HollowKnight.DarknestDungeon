using SFCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.IC
{
    // Makes lifeblood cocoons respawn when they exist in the same room as a bench.
    public class FixLifebloodCocoonsModule : ItemChanger.Modules.Module
    {
        public override void Initialize() => On.HealthCocoon.OnTriggerEnter2D += FixLifebloodReset;

        public override void Unload() => On.HealthCocoon.OnTriggerEnter2D -= FixLifebloodReset;

        private void FixLifebloodReset(On.HealthCocoon.orig_OnTriggerEnter2D orig, HealthCocoon self, Collider2D collision)
        {
            bool before = self.GetAttr<HealthCocoon, bool>("activated");
            orig(self, collision);
            bool after = self.GetAttr<HealthCocoon, bool>("activated");

            if (!before && after)
            {
                // Self-deleting hook hack.
                List<GameManager.ResetSemiPersistentState> list = new();
                GameManager.ResetSemiPersistentState hook = () =>
                {
                    self.SetAttr("activated", false);
                    GameManager.instance.ResetSemiPersistentObjects -= list[0];
                };
                list.Add(hook);

                GameManager.instance.ResetSemiPersistentObjects += hook;
            }
        }
    }
}
