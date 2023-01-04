using UnityEngine;

namespace DarknestDungeon.Scripts
{
    internal class LightningField : MonoBehaviour
    {
        public enum CellType
        {
            Open,
            BottomLeft,
            BottomRight,
            Closed
        }

        // Compiled field.
        public CellType[,] cellTypes;

        // Editor fields.
        public int SlopeFactor;
        public int Width;
        public int Height;
    }
}
