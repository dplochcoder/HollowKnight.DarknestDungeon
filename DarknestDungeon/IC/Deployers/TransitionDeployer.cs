using DarknestDungeon.UnityExtensions;
using ItemChanger;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DarknestDungeon.IC.Deployers
{
    class TransitionReparent : MonoBehaviour
    {
        public void Update()
        {
            var parent = GameObject.Find("_Transition Gates") ?? new("_Transition Gates");
            gameObject.SetParent(parent);
            Destroy(this);
        }
    }

    public record TransitionDeployer : Deployer
    {
        public float width;
        public float height;

        public string Gate;
        public string TargetScene;
        public string TargetGate;

        public bool alwaysEnterLeft;
        public bool alwaysEnterRight;
        public float entryOffsetX;
        public float entryOffsetY;

        public override GameObject Instantiate()
        {
            var gate = Object.Instantiate(Preloader.Instance.TransitionGate);
            gate.name = Gate;

            var tp = gate.GetComponent<TransitionPoint>();
            tp.targetScene = TargetScene;
            tp.entryPoint = TargetGate;
            tp.alwaysEnterLeft = alwaysEnterLeft;
            tp.alwaysEnterRight = alwaysEnterRight;
            tp.entryOffset = new(entryOffsetX, entryOffsetY);

            var bc2d = gate.GetComponent<BoxCollider2D>();
            bc2d.size = new(width, height);

            gate.AddComponent<TransitionReparent>();
            return gate;
        }

        public override GameObject Deploy()
        {
            var obj = base.Deploy();
            obj.name = Gate;
            return obj;
        }
    }
}
