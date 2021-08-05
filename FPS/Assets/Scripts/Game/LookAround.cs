using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LookAround : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform player;
    [SerializeField] Transform cam;
    [SerializeField] Transform weapon;

    [SerializeField] float xSensitivity;
    [SerializeField] float ySensitivity;
    [SerializeField] float maxAngle;

    private Quaternion camCenter;
    public static bool cursorLocked = true;
    
    void Start()
    {
        camCenter = cam.localRotation;   //original rotation    
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        SetY();
        SetX();
        UpdateCursorLock();
    }

    void SetY()
    {
        float yLook = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
        Quaternion adj = Quaternion.AngleAxis(yLook, -Vector3.right);
        Quaternion delta = cam.localRotation * adj;

        if (Quaternion.Angle(camCenter, delta) <= maxAngle)
        {
            cam.localRotation = delta;
        }
        weapon.rotation = cam.rotation;
    }

    void SetX()
    {
        float xLook = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
        Quaternion adj = Quaternion.AngleAxis(xLook, Vector3.up);
        Quaternion delta = player.localRotation * adj;
        player.localRotation = delta;
    }

    void UpdateCursorLock()
    {
        if(cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLocked = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLocked = true;
            }
        }
    }
}
