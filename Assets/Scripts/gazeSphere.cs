using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class gazeSphere : MonoBehaviour
{
    //private readonly Tobii.XR.TobiiSocialEyeData _socialEyeData = new Tobii.XR.TobiiSocialEyeData();
    protected float maxDistance = 10;
    protected Vector3 moyenneGaze = new Vector3(0,0,0);
    protected const int nbFramesMax = 5;
    protected uint nbFrame = 0;
    protected Vector3[] dataGaze= new Vector3[nbFramesMax];


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
    

    // Update is called once per frame
    void Update()
    {
        //_socialEyeData.Tick();
        //var worldGazePoint = _socialEyeData.WorldGazePoint;
        //gameObject.transform.SetPositionAndRotation(worldGazePoint, Quaternion.identity);
        
        

        var gazeRay = Tobii.XR.TobiiXR.GetEyeTrackingData(Tobii.XR.TobiiXR_TrackingSpace.World).GazeRay;

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
        
    }

}