using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

namespace Plateformer
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Plateformer/Input/InputReader")]
    public class InputReader : ScriptableObject, IPlayerActions
    {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { };
        public event UnityAction EnableMouseControlCamera = delegate { };
        public event UnityAction DisableMouseControlCamera = delegate { };

        public event UnityAction<bool> Jump = delegate { };

        private PlayerInputActions inputActions;

    
        public Vector3 Direction
        {
            get
            {
                if (inputActions != null)
                    return (Vector3)inputActions.Player.Move.ReadValue<Vector2>();
                return Vector3.zero;
            }
        }

        private void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
                inputActions.Player.SetCallbacks(this);
            }

           
            inputActions.Enable();
        }

        public void EnablePlayerActions()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {

            inputActions?.Disable();
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            
            if (context.performed)
                Debug.Log("Fire !");
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Jump.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Jump.Invoke(false);
                    break;
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        private bool IsDeviceMouse(InputAction.CallbackContext context) =>
            context.control.device.name == "Mouse";

        public void OnMove(InputAction.CallbackContext context)
        {
            Move.Invoke(context.ReadValue<Vector2>());
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log("Run started");
            else if (context.canceled)
                Debug.Log("Run stopped");
        }

        public void OnMouseControlCamera(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    EnableMouseControlCamera.Invoke();
                    break;

                case InputActionPhase.Canceled:
                    DisableMouseControlCamera.Invoke();
                    break;
            }
        }
    }
}
