using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DarknestDungeon.UnityExtensions
{
    public class MonobehaviourPatcher<M> where M : MonoBehaviour
    {
        private record Field
        {
            public FieldInfo fi;
            public object value;
        }

        private readonly List<Field> fields = new();

        public MonobehaviourPatcher(M prefab, params string[] fieldNames)
        {
            foreach (var name in fieldNames)
            {
                var fi = prefab.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                fields.Add(new()
                {
                    fi = fi,
                    value = fi.GetValue(prefab)
                });
            }
        }

        public void Patch(M component) => fields.ForEach(f => f.fi.SetValue(component, f.value));
    }
}
