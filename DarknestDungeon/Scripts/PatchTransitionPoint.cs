using UnityEngine;
using UnityEngine.Audio;

namespace DarknestDungeon.Scripts
{
    public class PatchTransitionPoint : MonoBehaviour
    {
        [Header("Door Type Gate Settings")]
        [Space(5f)]
        public bool isADoor;

        public bool dontWalkOutOfDoor;

        [Header("Gate Entry")]
        [Tooltip("The wait time before entering from this gate (not the target gate).")]
        public float entryDelay;

        public Vector2 entryOffset;

        public bool alwaysEnterRight;

        public bool alwaysEnterLeft;

        [Header("Force Hard Land (Top Gates Only)")]
        [Space(5f)]
        public bool hardLandOnExit;

        [Header("Destination Scene")]
        [Space(5f)]
        public string targetScene;

        public string entryPoint;

        [Header("Hazard Respawn")]
        [Space(5f)]
        public bool nonHazardGate;

        public PatchHazardRespawnMarker hazardRespawnMarker;

        [Header("Set Audio Snapshots")]
        [Space(5f)]
        public AudioMixerSnapshot atmosSnapshot;

        public AudioMixerSnapshot enviroSnapshot;

        public AudioMixerSnapshot actorSnapshot;

        public AudioMixerSnapshot musicSnapshot;

        public enum SceneLoadVisualizations
        {
            Default = 0,
            Custom = -1,
            Dream = 1,
            Colosseum = 2,
            GrimmDream = 3,
            ContinueFromSave = 4,
            GodsAndGlory = 5
        }

        [Header("Cosmetics")]
        public SceneLoadVisualizations sceneLoadVisualization = SceneLoadVisualizations.Default;

        public bool forceWaitFetch;

        private void Awake()
        {
            var tp = gameObject.AddComponent<TransitionPoint>();
            tp.isADoor = isADoor;
            tp.dontWalkOutOfDoor = dontWalkOutOfDoor;
            tp.entryDelay = entryDelay;
            tp.alwaysEnterRight = alwaysEnterRight;
            tp.alwaysEnterLeft = alwaysEnterLeft;
            tp.hardLandOnExit = hardLandOnExit;
            tp.targetScene = targetScene;
            tp.entryPoint = entryPoint;
            tp.entryOffset = entryOffset;
            tp.nonHazardGate = nonHazardGate;
            tp.respawnMarker = hazardRespawnMarker.GetHRM();
            tp.atmosSnapshot = atmosSnapshot;
            tp.enviroSnapshot = enviroSnapshot;
            tp.actorSnapshot = actorSnapshot;
            tp.musicSnapshot = musicSnapshot;
            tp.sceneLoadVisualization = (GameManager.SceneLoadVisualizations)sceneLoadVisualization;
            tp.forceWaitFetch = forceWaitFetch;
        }
    }
}
