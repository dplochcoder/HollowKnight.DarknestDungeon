using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarknestDungeon.IC
{
    public class VoidShardsModule : ItemChanger.Modules.Module
    {
        public int numVoidShards = 0;

        public override void Initialize()
        {
            ModHooks.GetPlayerIntHook += OverrideGetInt;
            ModHooks.SetPlayerIntHook += OverrideSetInt;
        }

        public override void Unload()
        {
            ModHooks.GetPlayerIntHook -= OverrideGetInt;
            ModHooks.SetPlayerIntHook -= OverrideSetInt;
        }

        private int OverrideGetInt(string intName, int orig)
        {
            return intName switch
            {
                nameof(numVoidShards) => numVoidShards,
                _ => orig,
            };
        }

        private int OverrideSetInt(string intName, int newValue)
        {
            switch (intName)
            {
                case nameof(numVoidShards):
                    numVoidShards = newValue;
                    break;
            }
            return newValue;
        }
    }
}
