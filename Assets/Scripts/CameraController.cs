using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{

    private CameraControlActions cameraActions;

    public bool enableKeyboardControls = true; //키보드 동작 여부
    public bool enableMouseControls = true; //마우스 동작 여부
    public bool enablePanning = true; //패닝 동작 여부
    public bool enableHorizontalRotation = true;//세로 회전 동작여부
    public bool enableVerticalRotation = true;//가로 회전 동작여부
    public bool enableZooming = true;// 줌 여부
    public bool enableBoosting = true;//키입력시 빠른동작 여부

    public float easeSpeed = 8;

    private Transform xRotRig;
    private Camera cam;

    public float movementSpeed = 20;//패닝 스피드 값
    public float boostSpeedMultiplier = 2; //부스터 배율

    public KeyCode boostKey = KeyCode.LeftShift;

    private Vector3 intendedPosition;
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;

    public float rotationSpeed = 180;
    public float minVerticalRotation = -35;
    public float maxVerticalRotation = 40;
    public bool clampVerticalRotation = true;

    private Vector3 mouseRotateStartPosition;
    private Vector3 mouseRotateCurrentPosition;

    private float xRotation;
    private float yRotation;

    public float zoomSpeed = 6;

    public float minZoom = 4;

    public float maxZoom = 32;

    private Vector3 intendedZoom;

    [Header("카메라 가두기")]
    public float minPosX = -500;
    public float maxPosX = 500;
    public float minPosY = -10;
    public float maxPosY = 180;
    public float minPosZ = -400;
    public float maxPosZ = 400;

    private void Awake()
    {
        cameraActions = new CameraControlActions();
        xRotRig = transform.GetChild(0);
        cam = xRotRig.GetChild(0).GetComponent<Camera>();
    }

    private void Start()
    {
        intendedPosition = transform.position;
        intendedZoom = cam.transform.localPosition;
        xRotation = xRotRig.transform.eulerAngles.x;
        yRotation = transform.eulerAngles.y;
    }

    private void LateUpdate()
    {
        var rotate = cameraActions.Camera.RotateCamera.ReadValue<Vector2>();
        var move = cameraActions.Camera.Movement.ReadValue<Vector2>();
        var zoom = cameraActions.Camera.ZoomCamera.ReadValue<Vector2>();

        if (enableMouseControls)
            HandleMouseInput(rotate, zoom);
        if (enableKeyboardControls)
            HandleKeyboardInput();

        HandleMovement();
        CameraClamp();
    }

    private void OnDrawGizmos()
    {
        Vector3 p1 = new Vector3(minPosX, 0, maxPosZ);
        Vector3 p2 = new Vector3(maxPosX, 0, maxPosZ);
        Vector3 p3 = new Vector3(maxPosX, 0, minPosZ);
        Vector3 p4 = new Vector3(minPosX, 0, minPosZ);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }

    private void OnEnable()
    {
        cameraActions.Enable();
    }

    private void OnDisable()
    {
        cameraActions.Disable();
    }

    public void CameraClamp()
    {
        float _x = Mathf.Clamp(transform.position.x, minPosX, maxPosX);
        float _z = Mathf.Clamp(transform.position.z, minPosZ, maxPosZ);
        transform.position = new Vector3(_x, transform.position.y, _z);
    }

    private void HandleMouseInput(Vector2 rotate, Vector2 zoom)
    {
        // Mouse drag panning
        if (enablePanning)
        {
            if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
            {
                var plane = new Plane(Vector3.up, Vector3.zero);
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                if (plane.Raycast(ray, out var hit))
                    dragStartPosition = ray.GetPoint(hit);
            }

            if (Input.GetMouseButton((int)MouseButton.LeftMouse))
            {
                var plane = new Plane(Vector3.up, Vector3.zero);
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                if (!plane.Raycast(ray, out var hit))
                    return;
                                
                dragCurrentPosition = ray.GetPoint(hit);
                intendedPosition = transform.position + (dragStartPosition - dragCurrentPosition).normalized;
            }
        }

        // Mouse scroll
        if (enableZooming && zoom.sqrMagnitude != 0)
            intendedZoom += new Vector3(0, -1, 1) * (zoom.normalized.y * zoomSpeed);

        // Mouse rotation
        if (Mouse.current.rightButton.isPressed && rotate.sqrMagnitude > 0.01)
        {
            if (enableVerticalRotation)
                xRotation += -rotate.y / 4f;
            if (enableHorizontalRotation)
                yRotation -= -rotate.x / 4f;
        }
    }

    private void HandleKeyboardInput()
    {
        // Panning
        if (enablePanning)
        {
            var hor = Input.GetAxisRaw("Horizontal");
            var ver = Input.GetAxisRaw("Vertical");
            var speed = movementSpeed;

            if (enableBoosting && Input.GetKey(boostKey)) speed *= boostSpeedMultiplier;
            if (ver != 0) intendedPosition += transform.forward * (Mathf.Sign(ver) * speed * Time.deltaTime);
            if (hor != 0) intendedPosition += transform.right * (Mathf.Sign(hor) * speed * Time.deltaTime);
        }

        // Rotation
        if (enableHorizontalRotation && Input.GetKey(KeyCode.Q)) yRotation -= rotationSpeed * Time.deltaTime;
        if (enableHorizontalRotation && Input.GetKey(KeyCode.E)) yRotation += rotationSpeed * Time.deltaTime;

        if (enableVerticalRotation && Input.GetKey(KeyCode.R)) xRotation += rotationSpeed * Time.deltaTime;
        if (enableVerticalRotation && Input.GetKey(KeyCode.F)) xRotation -= rotationSpeed * Time.deltaTime;

    }

    private void HandleMovement()
    {
        // Panning
        transform.position = Vector3.Lerp(transform.position, intendedPosition, Time.deltaTime * easeSpeed);

        // Rotation
        if (clampVerticalRotation) 
            xRotation = Mathf.Clamp(xRotation, minVerticalRotation, maxVerticalRotation);

        xRotRig.localRotation = Quaternion.Slerp(xRotRig.localRotation, Quaternion.Euler(xRotation, 0, 0), Time.deltaTime * easeSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yRotation, 0), Time.deltaTime * easeSpeed);

        // Zooming
        intendedZoom.z = -(intendedZoom.y = Mathf.Clamp(intendedZoom.y, minZoom, maxZoom));
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, intendedZoom, Time.deltaTime * easeSpeed);
    }
}