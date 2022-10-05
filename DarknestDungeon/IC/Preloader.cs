using PurenailCore.ModUtil;
using UnityEngine;

namespace DarknestDungeon.IC
{
    public class Preloader : PurenailCore.ModUtil.Preloader
    {
        public static readonly Preloader Instance = new();

        [Preload("Abyss_06_Core", "_SceneManager")]
        public GameObject SceneManager { get; private set; }

        [Preload("Abyss_06_Core", "PlayMaker Unity 2D")]
        public GameObject PlayMaker { get; private set; }
    }
}
