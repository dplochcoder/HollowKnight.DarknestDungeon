using UnityEngine;

namespace DarknestDungeon.UnityExtensions
{
    public static class GameObjectExtensions
    {
        public static void SetParent(this GameObject self, GameObject parent) => self.transform.SetParent(parent.transform);

        public static T GetOrAddComponent<T>(this GameObject self) where T : MonoBehaviour => self.GetComponent<T>() ?? self.AddComponent<T>();
    }
}
