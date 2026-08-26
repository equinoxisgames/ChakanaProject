using UnityEngine;
using Unity.Cinemachine; // Correcto para Unity 6

namespace PixelCrushers.DialogueSystem
{

#if USE_CINEMACHINE
    [AddComponentMenu("")]
    public class CinemachineCameraPriorityOnDialogueEvent : ActOnDialogueEvent
    {
        [Tooltip("The Cinemachine camera whose priority to control.")]
        // CORRECCIÓN: En Unity 6 usamos CinemachineCamera directamente
        public CinemachineCamera virtualCamera;

        [Tooltip("Set the camera to this priority when the start event occurs.")]
        public int onStart = 99;

        [Tooltip("Set the camera to this priority when the end event occurs.")]
        public int onEnd = 0;

        public override void TryStartActions(Transform actor)
        {
            if (virtualCamera == null) return;
            // En Cinemachine 3, Priority sigue siendo un int accesible directamente
            virtualCamera.Priority = onStart;
        }

        public override void TryEndActions(Transform actor)
        {
            if (virtualCamera == null) return;
            virtualCamera.Priority = onEnd;
        }
    }

#else
    [AddComponentMenu("")] 
    public class CinemachineCameraPriorityOnDialogueEvent : ActOnDialogueEvent
    {
        public override void TryStartActions(Transform actor) { }
        public override void TryEndActions(Transform actor) { }
    }
#endif
}