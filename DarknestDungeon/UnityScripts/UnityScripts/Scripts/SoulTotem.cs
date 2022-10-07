﻿using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class SoulTotem : MonoBehaviour
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
    }
}
