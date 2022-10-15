using ItemChanger;
using System;
using System.Net.Http.Headers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DarknestDungeon.IC.Deployers
{
    public record LifebloodCocoonDeployer : PersistentBoolDeployer
    {
        public enum LifebloodType
        {
            TWO_LIFEBLOOD,
            THREE_LIFEBLOOD
        }

        public LifebloodType lifebloodType;

        protected override GameObject Template()
        {
            return lifebloodType switch
            {
                LifebloodType.TWO_LIFEBLOOD => Preloader.Instance.Lifeblood2,
                LifebloodType.THREE_LIFEBLOOD => Preloader.Instance.Lifeblood3,
                _ => throw new ArgumentException($"Invalid LifebloodType: {lifebloodType}"),
            };
        }
    }
}
