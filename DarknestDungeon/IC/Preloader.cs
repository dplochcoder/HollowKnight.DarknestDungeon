using PurenailCore.ModUtil;
using UnityEngine;

namespace DarknestDungeon.IC
{
    public class Preloader : PurenailCore.ModUtil.Preloader
    {
        public static readonly Preloader Instance = new();

        [Preload("Town", "_SceneManager")]
        public GameObject SceneManager { get; private set; }

        [Preload("Town", "_Managers/PlayMaker Unity 2D")]
        public GameObject PlayMaker { get; private set; }

        [Preload("Abyss_06_Core", "_Transition Gates/bot1")]
        public GameObject TransitionGate { get; private set; }

        [Preload("Abyss_17", "Quake Floor")]
        public GameObject DiveFloor { get; private set; }

        [Preload("Abyss_15", "TileMap")]
        public GameObject TileMap { get; private set; }
    }
}
