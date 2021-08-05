using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] float intensity; //value of 1 for best experience ... i think
    [SerializeField] float smooth;
    public bool isMine;

    private Quaternion origin_rotation;
    void Start()
    {
        origin_rotation = transform.localRotation;
    }

   
    private void Update()
    {
        UpdateSway();
    }

    private void UpdateSway()
    {
        //controls
        float xMouse = Input.GetAxis("Mouse X");
        float yMouse = Input.GetAxis("Mouse Y");

        if(!isMine)
        {
            xMouse = 0;
            yMouse = 0;
        }

        //target rotation
        Quaternion x_adj = Quaternion.AngleAxis(-xMouse * intensity, Vector3.up);
        Quaternion y_adj = Quaternion.AngleAxis(yMouse * intensity, Vector3.right);
        Quaternion targetRotation = origin_rotation * x_adj * y_adj;

        //rotate towards target rotation
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * smooth);
    }
}
