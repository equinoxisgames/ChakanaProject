using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Michsky.UI.Dark
{
    public class GamepadChecker : MonoBehaviour
    {
        // Resources
        public MainPanelManager defaultPanelManager;
        public List<MainPanelManager> panelManagers = new List<MainPanelManager>();

        // Settings
        [Tooltip("Always update input device. If you turn off this feature, you won't able to change the input device after start, but it might increase the performance.")]
        public bool alwaysUpdate = true;
        public bool affectCursor = true;
        public InputAction gamepadHotkey;

        // Core
        [Tooltip("Objects in this list will be enabled when the gamepad is un-plugged.")]
        public List<GameObject> keyboardObjects = new List<GameObject>();
        [Tooltip("Objects in this list will be enabled when the gamepad is plugged.")]
        public List<GameObject> gamepadObjects = new List<GameObject>();
        [Tooltip("Buttons in this list will be prepared for the current input device.")]
        public List<Button> buttons = new List<Button>();

        // Helpers
        Vector3 cursorPos;
        Vector3 lastCursorPos;
        Navigation customNav = new Navigation();

        [HideInInspector] public bool gamepadConnected;
        [HideInInspector] public bool gamepadEnabled;
        [HideInInspector] public bool keyboardEnabled;

        [HideInInspector] public float hAxis;
        [HideInInspector] public float vAxis;

        void Start()
        {
            if (alwaysUpdate == false) { this.enabled = false; }
            else { this.enabled = true; }

            gamepadHotkey.Enable();

            if (Gamepad.current == null)
            {
                gamepadConnected = false;
                SwitchToKeyboard();
            }
            else
            {
                gamepadConnected = true;
                SwitchToGamepad();
            }
        }

        void Update()
        {
            if (Gamepad.current == null)
                gamepadConnected = false;
            else
            {
                gamepadConnected = true;
                hAxis = Gamepad.current.rightStick.x.ReadValue();
                vAxis = Gamepad.current.rightStick.y.ReadValue();
            }

            cursorPos = Mouse.current.position.ReadValue();

            if (gamepadConnected == true && gamepadEnabled == true
                && keyboardEnabled == false && cursorPos != lastCursorPos)
                SwitchToKeyboard();
            else if (gamepadConnected == true && gamepadEnabled == false
                && keyboardEnabled == true && gamepadHotkey.triggered)
                SwitchToGamepad();
            else if (gamepadConnected == false && keyboardEnabled == false)
                SwitchToKeyboard();
        }

        public void SwitchToGamepad()
        {
            for (int i = 0; i < keyboardObjects.Count; i++)
            {
                if (keyboardObjects[i] == null)
                    continue;
                keyboardObjects[i].SetActive(false);
            }

            for (int i = 0; i < gamepadObjects.Count; i++)
            {
                if (gamepadObjects[i] == null)
                    continue;
                gamepadObjects[i].SetActive(true);
            }

            // Rebuild síncrono inmediato (cubre escenas simples)
            ForceRebuildLayouts(gamepadObjects);

            // Rebuild diferido como seguro (cubre escenas con layouts anidados o animaciones)
            StartCoroutine(RebuildAllLayoutsDelayed(gamepadObjects));

            customNav.mode = Navigation.Mode.Automatic;

            for (int i = 0; i < buttons.Count; i++)
                if (buttons[i] != null)
                    buttons[i].navigation = customNav;

            gamepadEnabled = true;
            keyboardEnabled = false;
            lastCursorPos = Mouse.current.position.ReadValue();

            if (affectCursor == true)
                Cursor.visible = false;

            if (defaultPanelManager != null)
            {
                defaultPanelManager.gamepadEnabled = true;
                SelectUIObject(defaultPanelManager.panels[defaultPanelManager.currentPanelIndex].defaultSelected);
            }

            for (int i = 0; i < panelManagers.Count; i++)
            {
                if (panelManagers[i] == null)
                    continue;
                panelManagers[i].gamepadEnabled = true;
            }
        }

        public void SwitchToKeyboard()
        {
            for (int i = 0; i < gamepadObjects.Count; i++)
            {
                if (gamepadObjects[i] == null)
                    continue;
                gamepadObjects[i].SetActive(false);
            }

            for (int i = 0; i < keyboardObjects.Count; i++)
            {
                if (keyboardObjects[i] == null)
                    continue;
                keyboardObjects[i].SetActive(true);
            }

            // Rebuild síncrono inmediato (cubre escenas simples)
            ForceRebuildLayouts(keyboardObjects);

            // Rebuild diferido como seguro (cubre escenas con layouts anidados o animaciones)
            StartCoroutine(RebuildAllLayoutsDelayed(keyboardObjects));

            customNav.mode = Navigation.Mode.None;
            for (int i = 0; i < buttons.Count; i++)
                if (buttons[i] != null)
                    buttons[i].navigation = customNav;

            gamepadEnabled = false;
            keyboardEnabled = true;

            if (affectCursor == true)
                Cursor.visible = true;
            if (defaultPanelManager != null)
                defaultPanelManager.gamepadEnabled = false;
        }

        /// <summary>
        /// Rebuild síncrono e inmediato de todos los LayoutGroups en la jerarquía.
        /// Recorre de hijo a padre para respetar el orden correcto de recálculo.
        /// </summary>
        private void ForceRebuildLayouts(List<GameObject> objects)
        {
            HashSet<RectTransform> rebuilt = new HashSet<RectTransform>();

            foreach (GameObject obj in objects)
            {
                if (obj == null) continue;

                // Primero el propio objeto
                RectTransform selfRT = obj.GetComponent<RectTransform>();
                if (selfRT != null && !rebuilt.Contains(selfRT))
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(selfRT);
                    rebuilt.Add(selfRT);
                }

                // Luego sube por los padres buscando LayoutGroups
                Transform parent = obj.transform.parent;
                while (parent != null)
                {
                    RectTransform parentRT = parent.GetComponent<RectTransform>();
                    LayoutGroup lg = parent.GetComponent<LayoutGroup>();

                    if (parentRT != null && lg != null && !rebuilt.Contains(parentRT))
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
                        rebuilt.Add(parentRT);
                    }

                    parent = parent.parent;
                }
            }

            Debug.Log($"[GamepadChecker] ForceRebuildLayouts (síncrono) — layouts reconstruidos: {rebuilt.Count}");
        }

        /// <summary>
        /// Rebuild diferido: espera 2 frames y repite el rebuild.
        /// Cubre casos donde el Canvas o animaciones interfieren con el layout inmediato.
        /// </summary>
        private IEnumerator RebuildAllLayoutsDelayed(List<GameObject> objects)
        {
            yield return null;
            yield return null;

            HashSet<RectTransform> rebuilt = new HashSet<RectTransform>();

            foreach (GameObject obj in objects)
            {
                if (obj == null) continue;

                RectTransform selfRT = obj.GetComponent<RectTransform>();
                if (selfRT != null && !rebuilt.Contains(selfRT))
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(selfRT);
                    rebuilt.Add(selfRT);
                }

                Transform parent = obj.transform.parent;
                while (parent != null)
                {
                    RectTransform parentRT = parent.GetComponent<RectTransform>();
                    LayoutGroup lg = parent.GetComponent<LayoutGroup>();

                    if (parentRT != null && lg != null && !rebuilt.Contains(parentRT))
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
                        rebuilt.Add(parentRT);
                    }

                    parent = parent.parent;
                }
            }

            Debug.Log($"[GamepadChecker] RebuildAllLayoutsDelayed (diferido) — layouts reconstruidos: {rebuilt.Count}");
        }

        public void SelectUIObject(GameObject tempObj)
        {
            if (gamepadEnabled == false)
                return;

            EventSystem.current.SetSelectedGameObject(tempObj.gameObject);
        }

        public void SelectDefaultPanelObject()
        {
            if (gamepadEnabled == false)
                return;

            EventSystem.current.SetSelectedGameObject(defaultPanelManager.panels[defaultPanelManager.currentPanelIndex].defaultSelected);
        }
    }
}