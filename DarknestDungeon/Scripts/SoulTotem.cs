using DarknestDungeon.UnityExtensions;
using ItemChanger.Deployers;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SoulTotem : DeployerMonobehaviour<SoulTotemDeployer>
    {
        public enum SoulTotemSubtype
        {
            A,
            B,
            C,
            D,
            E,
            F,
            G,
            Palace,
            PathOfPain
        }

        public SoulTotemSubtype TotemType;

        private static readonly Dictionary<SoulTotemSubtype, Vector2> editorOffsets = new()
        {
            {SoulTotemSubtype.A, new(0, 0.65f)},
            {SoulTotemSubtype.B, new(0, 0.4f)},
            {SoulTotemSubtype.C, new(-0.1f, 0.61f)},
            {SoulTotemSubtype.D, new(0, 1.1f)},
            {SoulTotemSubtype.E, new(0, 1.1f)},
            {SoulTotemSubtype.F, new(0.2f, 0.9f)},
            {SoulTotemSubtype.G, new(0, 0.6f)},
            {SoulTotemSubtype.Palace, new(0, 0.8f)},
            {SoulTotemSubtype.PathOfPain, new(0, 0.5f)}
        };

        public override void ModifyDeployer(ref SoulTotemDeployer deployer) => deployer = deployer with
        {
            X = deployer.X + editorOffsets[TotemType].x,
            Y = deployer.Y + editorOffsets[TotemType].y,
            SoulTotemSubtype = (ItemChanger.SoulTotemSubtype)TotemType
        };

        public override void ModifyDeployment(GameObject obj)
        {
            obj.transform.SetPositionZ(0.01f);
            obj.GetComponent<SpriteRenderer>().flipX = gameObject.GetComponent<SpriteRenderer>().flipX;
        }
    }
}
