using DarknestDungeon.UnityExtensions;
using SFCore.Utils;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class TinkEffect : global::TinkEffect
    {
        private static readonly MonobehaviourPatcher<global::TinkEffect> Patcher = new(
            Preloader.Instance.Goam.GetComponent<global::TinkEffect>(),
            "blockEffect");

        private void Awake()
        {
            Patcher.Patch(this);
            this.SetAttr<global::TinkEffect, BoxCollider2D>("boxCollider", gameObject.GetComponent<BoxCollider2D>());
        }
    }
}
