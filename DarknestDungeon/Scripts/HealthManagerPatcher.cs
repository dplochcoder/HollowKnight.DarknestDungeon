using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class HealthManagerPatcher : MonoBehaviour
    {
        private record HMFieldInfo
        {
            public FieldInfo field;
            public GameObject prefab;
        }

        private static readonly List<HMFieldInfo> fieldInfos = new();

        private void Awake()
        {
            var hm = gameObject.GetComponent<HealthManager>();
            if (hm == null)
            {
                Destroy(this);
                return;
            }

            Load();
            fieldInfos.ForEach(fi => fi.field.SetValue(hm, fi.prefab));
            Destroy(this);
        }

        private static void Load()
        {
            if (fieldInfos.Count > 0) return;

            DarknestDungeon.Log($"HealthManagerPatcher: Loading fields");
            var prefab = Preloader.Instance.DemoEnemy.GetComponent<HealthManager>();
            foreach (var fi in typeof(HealthManager).GetRuntimeFields().Where(f => f.Name.EndsWith("Prefab")).ToList())
            {
                DarknestDungeon.Log($"HealthManagerPatcher: {fi.Name}");
                fieldInfos.Add(new()
                {
                    field = fi,
                    prefab = fi.GetValue(prefab) as GameObject
                });
            }
        }
    }
}
