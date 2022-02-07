using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class gazeSphere : MonoBehaviour
{
    private readonly Tobii.XR.TobiiSocialEyeData _socialEyeData = new Tobii.XR.TobiiSocialEyeData();

    // Update is called once per frame
    void Update()
    {
        _socialEyeData.Tick();
        var worldGazePoint = _socialEyeData.WorldGazePoint;
        gameObject.transform.SetPositionAndRotation(worldGazePoint, Quaternion.identity);
    }
}