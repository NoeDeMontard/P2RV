using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gazeMove : MonoBehaviour
{

    protected float maxDistance = 100;

    public int blinkingTime = 0;
    private int maxBlinkingTime = 50;

    private float rot_angle = 0;
    private Quaternion new_rotation = Quaternion.identity;

    public Vector3 telep_point;
    private bool can_teleport;

    private float rot_speed;
    private float rot_acc = 0.005f;
    private float max_speed= 1.0f;
    private float initial_speed = 0.1f;

    private GameObject TPzone;


    // Start is called before the first frame update
    void Start()
    {
        telep_point = transform.position;
        can_teleport = false;

        rot_speed = initial_speed;

        TPzone = transform.Find("TPzone").gameObject; 
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<handControl>().ResetHandPosition();
        var gazeRay = Tobii.XR.TobiiXR.GetEyeTrackingData(Tobii.XR.TobiiXR_TrackingSpace.World).GazeRay;
        var localGazeData = Tobii.XR.TobiiXR.GetEyeTrackingData(Tobii.XR.TobiiXR_TrackingSpace.Local);
        bool IsLeftEyeBlinking = localGazeData.IsLeftEyeBlinking;
        bool IsRightEyeBlinking = localGazeData.IsRightEyeBlinking;

        bool opened = gazeRay.IsValid;

        bool leftClosed = opened && IsLeftEyeBlinking;
        bool rightClosed = opened && IsRightEyeBlinking;

        if (!opened)
        {
            blinkingTime++;
        }
        else
        {
            blinkingTime = 0;

            Ray ray = new Ray(Camera.main.transform.position, gazeRay.Direction);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                if (hit.transform.CompareTag("ground"))
                {
                    can_teleport = true;
                    telep_point = hit.point;
                }
                else
                {
                    can_teleport = false;
                }
            }
            else
            {
                can_teleport = false;
            }
        }

        // Affichage d'une zone de TP
        if (can_teleport)
        {
            TPzone.transform.SetPositionAndRotation(telep_point, Quaternion.identity);
            TPzone.SetActive(true);
        }
        else
        {
            TPzone.SetActive(false);
        }

        //Modidier la rotation de l'avatar en foncton de l'oeil fermé
        if (leftClosed)
        {
            rot_angle = -rot_speed;
            rot_speed += rot_acc;
            if (rot_speed > max_speed)
                rot_speed = max_speed;
        }
        else if (rightClosed)
        {
            rot_angle = rot_speed;
            rot_speed += rot_acc;
            if (rot_speed > max_speed)
                rot_speed = max_speed;
        }
        else
        {
            rot_speed = initial_speed;
            rot_angle = 0;
        }

        // Considerer les deux yeux fermés quand ils le sont assez longtemps
        bool eyesClosed = (blinkingTime >= maxBlinkingTime);

        //Ordre de teleportation via la manette
        bool telep_order = Tobii.XR.ControllerManager.Instance.GetButtonPressDown(Tobii.XR.ControllerButton.Trigger);

        //Ordre de teleportation via la manette ou les yeux fermés
        telep_order = telep_order || eyesClosed;

        //Modification de la position de l'avatar ou de son orientation
        if (telep_order && can_teleport)
        {
            transform.SetPositionAndRotation(telep_point, transform.rotation);
        }
        else
        {
            new_rotation = transform.rotation * Quaternion.AngleAxis(rot_angle, new Vector3(0, 1, 0));
            transform.SetPositionAndRotation(transform.position, new_rotation);
        }

    }
}
