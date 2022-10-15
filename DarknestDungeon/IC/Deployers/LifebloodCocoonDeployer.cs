using ItemChanger;
using System;
using System.Net.Http.Headers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DarknestDungeon.IC.Deployers
{
    public record LifebloodCocoonDeployer : Deployer
    {
        public enum LifebloodType
        {
            TWO_LIFEBLOOD,
            THREE_LIFEBLOOD
        }

        public LifebloodType lifebloodType;
        public string id;

        private GameObject Template()
        {
            return lifebloodType switch
            {
                LifebloodType.TWO_LIFEBLOOD => Preloader.Instance.Lifeblood2,
                LifebloodType.THREE_LIFEBLOOD => Preloader.Instance.Lifeblood3,
                _ => throw new ArgumentException($"Invalid LifebloodType: {lifebloodType}"),
            };
        }

        public override GameObject Instantiate()
        {
            var obj = Object.Instantiate(Template());
            var pbd = obj.GetComponent<PersistentBoolItem>().persistentBoolData;
            pbd.sceneName = SceneName;
            pbd.id = id;
            return obj;
        }
    }
}
