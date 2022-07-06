using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OtherCameraControl : MonoBehaviour
{
    private CameraControlActions cameraActions;
    private InputAction movement;

    public float moveSpeed;
    public float zoomSpeed;
    public float rotateSpeed;

    private Vector2 m_Rotation;
    private Vector3 m_Zoom;

    // Start is called before the first frame update
    private void Awake()
    {
        cameraActions = new CameraControlActions();
    }

    // Update is called once per frame
    void Update()
    {
        var look = cameraActions.Camera.RotateCamera.ReadValue<Vector2>();
        var move = cameraActions.Camera.Movement.ReadValue<Vector2>();
        var zoom = cameraActions.Camera.ZoomCamera.ReadValue<Vector2>();
        Look(look);
        Move(move);
        Zoom(zoom);
    }

    public void OnEnable()
    {
        cameraActions.Enable();
    }

    public void OnDisable()
    {
        cameraActions.Disable();
    }

    private void Look(Vector2 rotate)
    {
        if (!Mouse.current.rightButton.isPressed)
            return;
        if (rotate.sqrMagnitude < 0.01)
            return;
        var scaledRotateSpeed = rotateSpeed * Time.deltaTime;
        m_Rotation.y += rotate.x * scaledRotateSpeed;
        m_Rotation.x = Mathf.Clamp(m_Rotation.x - rotate.y * scaledRotateSpeed, -89, 89);
        transform.localEulerAngles = m_Rotation;
    }

    private void Move(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01)
            return;
        var scaledMoveSpeed = moveSpeed * Time.deltaTime;
        var move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(direction.x, 0, direction.y);
        transform.position += move * scaledMoveSpeed;
    }

    private void Zoom(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01)
            return;
        var scaledZoomSpeed = zoomSpeed * Time.deltaTime;
        var zoom = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z) * new Vector3(0, 0, direction.normalized.y);
        transform.position += zoom * scaledZoomSpeed;
    }
}