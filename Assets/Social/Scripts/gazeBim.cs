using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class gazeBim : MonoBehaviour
{
    private readonly Tobii.XR.TobiiSocialEyeData _socialEyeData = new Tobii.XR.TobiiSocialEyeData();
    protected Vector3[] points = new Vector3[2]; 


    // Update is called once per frame
    void Update()
    {
        _socialEyeData.Tick();
        var gazeRay =Tobii.XR.TobiiXR.GetEyeTrackingData(Tobii.XR.TobiiXR_TrackingSpace.World).GazeRay;
        if (!gazeRay.IsValid)
        {
            points[0] = Camera.main.transform.position;
            points[1] = gazeRay.Direction * 10+ gazeRay.Origin;
            GetComponent<LineRenderer>().SetPositions(points);
        }

        
    }
}
