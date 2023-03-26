using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Michsky.UI.Dark
{

    public class PressKeyEvent : MonoBehaviour
    {
        // Settings
        public InputAction hotkey;

        // Events
        public UnityEvent onPressEvent;
        string escena;

        void Start()
        {
            hotkey.Enable();
        }

        void Update()
        {
            //escena = SceneManager.GetActiveScene().name;

            //if (escena == "00- Main Menu 0")
            //{
                if (hotkey.triggered)
                onPressEvent.Invoke();
            //}
        }
    }
}