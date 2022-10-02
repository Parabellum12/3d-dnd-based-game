using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class General_2D_Camera_Handler_Script : MonoBehaviour
{
    /*
     * general code created for 2d cameras
     */
    public bool HorizontalMovement = true;
    public bool VerticalMovement = true;
    public bool ShiftForFasterMovement = true;

    public float baseMoveSpeed = 5f;
    public float shiftMoveSpeedMultiplier = 2f;

    public bool MousePan = true;
    public int MouseButtonToPanWith = 2;
    public bool AllowSecondaryMouseButtonPan = true;
    public int SecondaryMouseButonTOPanWith = 1;

    public bool WASDPan = true;
    public bool ArrowKeyPan = true;

    public bool CamZoom = true;
    public bool CamZoomViaButtons = true;
    public bool CamZoomDoesAccel = true;
    public float CamZoomAccelRateWaitTime = 3f;
    public float CamZoomAccelRate = 5f;
    public float maxZoomRate = 20;
    public float TimeLimitBetweenScrollsForZoomAccel = .1f;
    public float minCameraZoom = 50f;
    public float maxCameraZoom = 100f;


    public bool lockMoveIfOverUi = true;
    public bool lockZoomIfOverUi = true;
    public bool lockMovement = false;

    [SerializeField] float yPos = -10;

    [SerializeField] Camera cam;


    int horizontalMove;
    int verticalMove;

    // Update is called once per frame
    void Update()
    {
        horizontalMove = 0;
        verticalMove = 0;
        if (WASDPan)
        {
            if (Input.GetKey(KeyCode.A))
            {
                horizontalMove--;
            }
            if (Input.GetKey(KeyCode.D))
            {
                horizontalMove++;
            }

            if (Input.GetKey(KeyCode.W))
            {
                verticalMove++;
            }
            if (Input.GetKey(KeyCode.S))
            {
                verticalMove--;
            }
        }
        if (ArrowKeyPan)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                horizontalMove--;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                horizontalMove++;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                verticalMove++;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                verticalMove--;
            }
        }
        if (!HorizontalMovement)
        {
            horizontalMove = 0;
        }
        if (!VerticalMovement)
        {
            verticalMove = 0;
        }

        verticalMove = Mathf.Clamp(verticalMove, -1, 1);
        horizontalMove = Mathf.Clamp(horizontalMove, -1, 1);




        handleScroll();
        handleKeyMove();
        handleCameraPan();
    }


    bool buttonsHeldForZoomIn()
    {
        return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.Equals);
    }
    bool buttonsHeldForZoomOut()
    {
        return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.Minus);
    }


    float currentZoomChangeSpeed = 1;
    bool zooming = false;
    bool lockZoom = false;
    float TimeLastZoom = 0;
    float timeStartZoom = 0;
    void handleScroll()
    {
        if (lockMovement || !CamZoom || (lockZoomIfOverUi && UtilClass.IsPointerOverUIElement(LayerMask.NameToLayer("UI"))) || !UtilClass.IsPointerOverUIElement(LayerMask.NameToLayer("mouseOverCanvas")))
        {
            return;
        }


        if (Input.mouseScrollDelta.y != 0 || buttonsHeldForZoomIn() || buttonsHeldForZoomOut())
        {
            lockZoom = false;
            TimeLastZoom = Time.realtimeSinceStartup;
        }

        if ((Time.realtimeSinceStartup - TimeLastZoom < TimeLimitBetweenScrollsForZoomAccel || buttonsHeldForZoomIn() || buttonsHeldForZoomOut()) && !lockZoom)
        {
            if (!zooming)
            {
                timeStartZoom = Time.realtimeSinceStartup;
                zooming = true;
            }
            if (Time.realtimeSinceStartup - timeStartZoom > CamZoomAccelRateWaitTime && zooming && Input.mouseScrollDelta.y != 0)
            {
                //start increasing zoom speed
                currentZoomChangeSpeed += CamZoomAccelRate * Time.deltaTime;
            }
        }
        else
        {
            timeStartZoom = 0;
            TimeLastZoom = 0;
            zooming = false;
            currentZoomChangeSpeed = 1;
            lockZoom = true;
        }


        currentZoomChangeSpeed = Mathf.Clamp(currentZoomChangeSpeed, 0, maxZoomRate);

        Vector2 mouseScrollDelta = Input.mouseScrollDelta;
        if (buttonsHeldForZoomIn())
        {
            mouseScrollDelta.y = 1;
        }
        if (buttonsHeldForZoomOut())
        {
            mouseScrollDelta.y -= 1;
        }
        cam.orthographicSize -= mouseScrollDelta.y * currentZoomChangeSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minCameraZoom, maxCameraZoom);
    }


    void handleKeyMove()
    {
        if (lockMovement || lockMoveIfOverUi && UtilClass.IsPointerOverUIElement(LayerMask.NameToLayer("UI")) || !UtilClass.IsPointerOverUIElement(LayerMask.NameToLayer("mouseOverCanvas")))
        {
            return;
        }
        horizontalMove = Mathf.Clamp(horizontalMove, -1, 1);
        verticalMove = Mathf.Clamp(verticalMove, -1, 1);
        float moveSpeedMulti = 1;
        if (ShiftForFasterMovement && Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeedMulti = shiftMoveSpeedMultiplier;
        }
        cam.transform.position = new Vector3(cam.transform.position.x + (horizontalMove * (baseMoveSpeed * moveSpeedMulti) * Time.deltaTime),
                                                cam.transform.position.y + (verticalMove * (baseMoveSpeed * moveSpeedMulti) * Time.deltaTime),
                                                yPos);
    }

    Vector3 originalMousePos;
    void handleCameraPan()
    {
        if (lockMovement || !MousePan || (lockMoveIfOverUi && UtilClass.IsPointerOverUIElement(LayerMask.NameToLayer("UI"))) || !UtilClass.IsPointerOverUIElement(LayerMask.NameToLayer("mouseOverCanvas")))
        {
            return;
        }

        if (Input.GetMouseButtonDown(MouseButtonToPanWith) || Input.GetMouseButtonDown(SecondaryMouseButonTOPanWith))
        {
            originalMousePos = UtilClass.getMouseWorldPosition();
        }
        else if (Input.GetMouseButton(MouseButtonToPanWith) || Input.GetMouseButton(SecondaryMouseButonTOPanWith))
        {
            Vector2 newMousePos = UtilClass.getMouseWorldPosition();
            float horizotalDist = newMousePos.x - originalMousePos.x;
            float verticalDist = newMousePos.y - originalMousePos.y;

            Vector3 newPos = new Vector3(cam.transform.position.x - horizotalDist,
                                            cam.transform.position.y - verticalDist,
                                            yPos);
            cam.transform.position = newPos;


            originalMousePos = UtilClass.getMouseWorldPosition();
        }
    }
}
