using PurenailCore.ModUtil;
using UnityEngine;

namespace DarknestDungeon
{
    public class Preloader : PurenailCore.ModUtil.Preloader
    {
        public static readonly Preloader Instance = new();

        [Preload("Abyss_17", "Quake Floor")]
        public GameObject DiveFloor { get; private set; }

        [Preload("Tutorial_01", "_Props/Health Cocoon")]
        public GameObject Lifeblood2 { get; private set; }

        [Preload("Fungus3_30", "Health Cocoon")]
        public GameObject Lifeblood3 { get; private set; }

        [Preload("Town", "_Managers/PlayMaker Unity 2D")]
        public GameObject PlayMaker { get; private set; }

        [Preload("Town", "_SceneManager")]
        public GameObject SceneManager { get; private set; }

        [Preload("Abyss_15", "TileMap")]
        public GameObject TileMap { get; private set; }

        [Preload("Abyss_06_Core", "_Transition Gates/bot1")]
        public GameObject TransitionGate { get; private set; }
    }
}
