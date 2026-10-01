using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Input
{
    public sealed class PrototypeInput : MonoBehaviour
    {
        private InputActionMap actions;
        public InputAction Move { get; private set; }
        public InputAction Orbit { get; private set; }
        public InputAction Look { get; private set; }
        public InputAction Scroll { get; private set; }
        public event Action<string> PanelRequested;
        public bool BlockGameplay { get; set; }

        private void Awake()
        {
            actions = new InputActionMap("Prototype");
            Move = actions.AddAction("Move", InputActionType.Value);
            Move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Orbit = actions.AddAction("Orbit", InputActionType.Button, "<Mouse>/rightButton");
            Look = actions.AddAction("Look", InputActionType.PassThrough, "<Mouse>/delta");
            Scroll = actions.AddAction("Hotbar", InputActionType.PassThrough, "<Mouse>/scroll");
            BindPanel("Shop", "<Keyboard>/b");
            BindPanel("Upgrades", "<Keyboard>/u");
            BindPanel("Troops", "<Keyboard>/t");
            BindPanel("Equipment", "<Keyboard>/i");
            BindPanel("Pause", "<Keyboard>/escape");
        }

        private void BindPanel(string name, string binding)
        {
            var action = actions.AddAction(name, InputActionType.Button, binding);
            action.performed += _ => PanelRequested?.Invoke(name);
        }

        private void OnEnable() => actions?.Enable();
        private void OnDisable() => actions?.Disable();
        private void OnDestroy() => actions?.Dispose();
    }
}
