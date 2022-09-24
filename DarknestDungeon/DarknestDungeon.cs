using Modding;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DarknestDungeon
{
    public class DarknestDungeon : Mod
    {
        public DarknestDungeon() : base("Darknest Dungeon") { }

        public override string GetVersion() => Version.Instance;
    }
}