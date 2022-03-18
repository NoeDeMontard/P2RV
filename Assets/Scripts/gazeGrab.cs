using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.G2OM;

public class gazeGrab : MonoBehaviour, IGazeFocusable
{

    protected float maxDistance = 10;
    protected float minmaxDistance = 0.1F;
    protected float maxmaxDistance = 100;
    protected Vector3 moyenneGaze = new Vector3(0, 0, 0);
    protected const int nbFramesMax = 5;
    protected uint nbFrame = 0;
    protected Vector3[] dataGaze = new Vector3[nbFramesMax];


    Vector3 moyenne(Vector3[] data)
    {
        Vector3 moy = new Vector3(0, 0, 0);
        for (int i = 0; i < data.Length; i++)
        {
            moy.x += data[i].x;
            moy.y += data[i].y;
            moy.z += data[i].z;
        }
        moy /= data.Length;
        return moy;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    private bool grabed = false;
    private bool focused = false;

    private float alpha = 0.01F;

    private int blinkingTime = 0;
    private int maxBlinkingTime = 20;
    private bool bloque = false;

    // Update is called once per frame
    void Update()
    {

        Physics.gravity = new Vector3(0, -1, 0);
        var gazeRay = Tobii.XR.TobiiXR.GetEyeTrackingData(Tobii.XR.TobiiXR_TrackingSpace.World).GazeRay;
        var localGazeData = Tobii.XR.TobiiXR.GetEyeTrackingData(Tobii.XR.TobiiXR_TrackingSpace.Local);
        bool IsLeftEyeBlinking = localGazeData.IsLeftEyeBlinking;
        bool IsRightEyeBlinking = localGazeData.IsRightEyeBlinking;

        if (IsLeftEyeBlinking || IsRightEyeBlinking)
        {
            blinkingTime++;
        }
        else
        {
            blinkingTime = 0;
        }
        bool eyeClosed = (blinkingTime >= maxBlinkingTime);
        bool eyesOpened = !(IsLeftEyeBlinking || IsRightEyeBlinking); // Une moyenne aussi ?

        

        bool grab = Tobii.XR.ControllerManager.Instance.GetButtonPressDown(Tobii.XR.ControllerButton.Trigger);

        grab = grab || (eyeClosed && !grabed);
        grab = grab && focused;


        bool ungrab = Tobii.XR.ControllerManager.Instance.GetButtonPressUp(Tobii.XR.ControllerButton.Trigger);
        ungrab = ungrab || (eyeClosed && grabed);

        grab = grab && !bloque;
        ungrab = ungrab && !bloque;

        if (eyesOpened)
        {
            bloque = false;
            Debug.Log("Bloque false");
        }
        if (grab || ungrab)
        //if (eyeClosed) // Résoud peut être des trucs ?
        {
            bloque = true;
            Debug.LogWarning("Bloque true");

        }
        Debug.Log("Eyes " + IsLeftEyeBlinking + " " + IsRightEyeBlinking);

        if (grab)
        {
            grabed = true;

            if (gazeRay.IsValid)
            {
                Ray ray = new Ray(Camera.main.transform.position, gazeRay.Direction);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maxDistance))
                {
                    maxDistance = hit.distance;
                }
            }

            blinkingTime = 0;
            
            Debug.Log("Grabbed");
        } 
        else if (ungrab)
        {
            grabed = false;
            Debug.Log("Ungrabbed");
        }

        if (grabed)
        {
            // Modify position
            if (gazeRay.IsValid)
            {
                Ray ray = new Ray(Camera.main.transform.position, gazeRay.Direction);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maxDistance))
                {
                    dataGaze[nbFrame] = hit.point;


                    moyenneGaze = moyenne(dataGaze);
                    gameObject.transform.SetPositionAndRotation(moyenneGaze, Quaternion.identity);

                    nbFrame++;
                    if (nbFrame == nbFramesMax)
                    {
                        nbFrame = 0;
                    }
                }
                else
                {
                    gameObject.transform.SetPositionAndRotation(ray.GetPoint(maxDistance), Quaternion.identity);
                }
            }


            //TODO
            if (Tobii.XR.ControllerManager.Instance.GetButtonPress(Tobii.XR.ControllerButton.TouchpadTouch))
            {
                float change = Tobii.XR.ControllerManager.Instance.GetTouchpadAxis().y;
                Debug.Log("change : " + change);
                change *= alpha;
                maxDistance += change;
                if (maxDistance > maxmaxDistance)
                    maxDistance = maxmaxDistance;
                else if (maxDistance < minmaxDistance)
                    maxDistance = minmaxDistance;
            }
        }
    }
    public void GazeFocusChanged(bool hasFocus)
    {
        focused = hasFocus;
    }
}
