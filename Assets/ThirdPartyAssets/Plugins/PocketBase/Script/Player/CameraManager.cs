using System;
using System.Collections;
using Cinemachine;
using UnityEngine;


namespace Plateformer
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Reference ")]
        [SerializeField] InputReader input;
        [SerializeField] CinemachineFreeLook freeLookCam;

        [Header("Setting")]

        [SerializeField, Range(0.5f, 3f)] float speedMultiplier = 1f;

        bool isRMBPressed;


        bool cameraMovementLock;

        void OnEnable()
        {
            input.Look += OnLook;
            input.EnableMouseControlCamera += OnEnableMouseControlCamera;
            input.DisableMouseControlCamera += OnDisableMouseControlCamera;
        }

        void OnDisable()
        {
            input.Look -= OnLook;
            input.EnableMouseControlCamera -= OnEnableMouseControlCamera;
            input.DisableMouseControlCamera -= OnDisableMouseControlCamera;
        }

        private void OnLook(Vector2 cameraMovement, bool IsDeviceMouse)
        {
            if (cameraMovementLock) return;
            if (IsDeviceMouse && !isRMBPressed) return;
            float deviceMultiplier = IsDeviceMouse ? Time.fixedDeltaTime : Time.deltaTime;

            freeLookCam.m_XAxis.m_InputAxisValue = cameraMovement.x * speedMultiplier * deviceMultiplier;
            freeLookCam.m_YAxis.m_InputAxisValue = cameraMovement.y * speedMultiplier * deviceMultiplier;
        }

        void OnEnableMouseControlCamera()
        {
            isRMBPressed = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            StartCoroutine(DisableMouseForFram());   
        }

        IEnumerator DisableMouseForFram()
        {
            cameraMovementLock = true;
            yield return new WaitForEndOfFrame();
            cameraMovementLock = false;
        }

        void OnDisableMouseControlCamera()
        {
            isRMBPressed = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            freeLookCam.m_XAxis.m_InputAxisValue = 0f;
            freeLookCam.m_YAxis.m_InputAxisValue = 0f;

        }

       
    }
}

