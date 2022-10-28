using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.UnityExtensions
{
    public static class Extensions
    {
        public static void SetParent(this GameObject self, GameObject parent) => self.transform.SetParent(parent.transform);

        public static T GetOrAddComponent<T>(this GameObject self) where T : MonoBehaviour => self.GetComponent<T>() ?? self.AddComponent<T>();

        public record GMHookReference
        {
            public GameManager.ResetSemiPersistentState Hook;
            public bool Unregistered = false;

            public void Unhook()
            {
                if (Unregistered) return;

                Unregistered = true;
                GameManager.instance.ResetSemiPersistentObjects -= Hook;
            }
        }

        public static GMHookReference AddSelfDeletingHook(this GameManager self, GameManager.ResetSemiPersistentState hook)
        {
            GMHookReference hookRef = new();
            hookRef.Hook = () =>
            {
                hook();

                self.ResetSemiPersistentObjects -= hookRef.Hook;
                hookRef.Unregistered = true;
            };

            self.ResetSemiPersistentObjects += hookRef.Hook;
            return hookRef;
        }
    }
}
