using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.G2OM;

public class globalGrab : MonoBehaviour
{

    public RaycastHit looked;
    public GameObject grabedObject;
    private bool staygrab = false; // Si l'objet restera attrapé d'ici le prochain Update
    private bool grab = false;
    private bool ungrab = false;

    // Distances, positions, leurs bornes
    protected float maxDistance = 10;
    protected float objectDistance = 10;
    protected float minobjectDistance = 0.1F;
    protected float maxobjectDistance = 100;

    // De quoi faire une moyenne pour stabiliser les objets
    protected Vector3 moyenneGaze = new Vector3(0, 0, 0);
    protected const int nbFramesMax = 5;
    protected uint nbFrame = 0;
    protected Vector3[] dataGaze = new Vector3[nbFramesMax];

    // Paramètres pour le highlighting 
    private Color _originalColor;
    private static readonly int _emissionColor = Shader.PropertyToID("_EmissionColor");
    private Color highlightColor = Color.red;
    private GameObject highlightedObject;
    private bool removeHighlight;

    // Coefficients de rapprochements
    private float coefBlinking;
    private float initial_coefBlinking = 0.0025f;
    private float max_coefBlinking = 0.01f;
    private float acc_coefBlinking = 0.0001f;
    private float coefTouchpad = 0.01F;

    // Clignements des yeux
    private int blinkingTime = 0;
    private int maxBlinkingTime = 50;
    private bool leftClosed = false;
    private bool rightClosed = false;
    private bool opened = false;

    

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

        Physics.gravity = new Vector3(0, -1, 0);
        var gazeRay = Tobii.XR.TobiiXR.GetEyeTrackingData(Tobii.XR.TobiiXR_TrackingSpace.World).GazeRay;
        var localGazeData = Tobii.XR.TobiiXR.GetEyeTrackingData(Tobii.XR.TobiiXR_TrackingSpace.Local);

        opened = gazeRay.IsValid;

        leftClosed = opened && localGazeData.IsLeftEyeBlinking;
        rightClosed = opened && localGazeData.IsRightEyeBlinking;

        if (!staygrab && !opened)
        {
            blinkingTime++;
        }
        else
        {
            blinkingTime = 0;
        }
        bool eyesClosed = (blinkingTime >= maxBlinkingTime);

        grab = Tobii.XR.ControllerManager.Instance.GetButtonPressDown(Tobii.XR.ControllerButton.Trigger);
        grab = grab || (eyesClosed && grabedObject==null);

        ungrab = Tobii.XR.ControllerManager.Instance.GetButtonPressUp(Tobii.XR.ControllerButton.Trigger);
        ungrab = ungrab || (eyesClosed && grabedObject!=null);

        grab = grab && !staygrab;
        ungrab = ungrab && !staygrab;

        if (opened)
        {
            staygrab = false;
            Debug.Log("staygrab false");
        }
        if (grab || ungrab)
        {
            staygrab = true;
            Debug.LogWarning("staygrab true");

        }

        Ray ray = new Ray();
        if (gazeRay.IsValid)
        {
            ray = new Ray(Camera.main.transform.position, gazeRay.Direction);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                // sauvegarde de l'objet regardé
                looked = hit;
                // rapprocher l'objet s'il est derriere un mur
                if (!hit.transform.gameObject.CompareTag("grabbable"))
                {
                    //enlever le highlight de l'objet precedement vu
                    removeHighlight = true;
                    //sauvegarde de la nouvelle distance max
                    maxobjectDistance = hit.distance;
                    if (maxobjectDistance < 5 * minobjectDistance)
                        maxobjectDistance = 5 * minobjectDistance;
                }
                else if(highlightedObject == null)
                {
                    // Code permettant de highlight l'objet grabbable vu s'il n'est pas attrapé
                    highlightedObject = hit.transform.gameObject;
                    if (highlightedObject != grabedObject)
                    {
                        _originalColor = highlightedObject.GetComponent<MeshRenderer>().material.GetColor(_emissionColor);
                        var newColor = Color.Lerp(_originalColor, highlightColor, 1.0f);
                        highlightedObject.GetComponent<MeshRenderer>().material.SetColor(_emissionColor, newColor);

                    }

                }
            }
            else
                removeHighlight = true;
        }

        if (grab)
        {
            if (looked.transform!=null && looked.transform.gameObject.CompareTag("grabbable"))
            {
                objectDistance = looked.distance;
                grabedObject = looked.transform.gameObject;
                //on demande d'arreter de highlight l'objet attrapé
                removeHighlight = true;
            }

            blinkingTime = 0;
            
            Debug.Log("Grabbed");
        } 
        else if (ungrab)
        {
            //on relache l'objet
            grabedObject = null;
            Debug.Log("Ungrabbed");

        }

        // On redonne la couleur initiale à l'objet
        if (removeHighlight && highlightedObject != null)
        {
            highlightedObject.GetComponent<MeshRenderer>().material.SetColor(_emissionColor, _originalColor);
            highlightedObject = null;
            removeHighlight = false;
        }

        if (grabedObject != null)
        {
            // Rapprocher l'objet avec la manette
            if (Tobii.XR.ControllerManager.Instance.GetButtonPress(Tobii.XR.ControllerButton.TouchpadTouch))
            {
                float change = Tobii.XR.ControllerManager.Instance.GetTouchpadAxis().y;     
                objectDistance += change * coefTouchpad;
            }
            // si aucun oeil est fermé, on reinitialise la vitesse de rapprochement
            else if(!leftClosed && !rightClosed)
            {
                coefBlinking = initial_coefBlinking;
            }
            // si on ferme un oeil
            else
            {
                // acceleraiton du rapprochement de l'objet
                coefBlinking += acc_coefBlinking;
                if (coefBlinking > max_coefBlinking)
                    coefBlinking = max_coefBlinking;

                if (leftClosed)
                    objectDistance -= coefBlinking;
                else if (rightClosed)
                    objectDistance += coefBlinking;
            }

            // Limiter la distance à laquelle se trouve l'objet
            if (objectDistance > maxobjectDistance)
                objectDistance = maxobjectDistance;
            else if (objectDistance < minobjectDistance)
                objectDistance = minobjectDistance;

            // Changement de la position
            if (gazeRay.IsValid)
            {
                //Ray ray = new Ray(Camera.main.transform.position, gazeRay.Direction);

                // On stablisise l'objet attrapé par une moyenne
                dataGaze[nbFrame] = ray.GetPoint(objectDistance);
                moyenneGaze = moyenne(dataGaze);

                grabedObject.transform.SetPositionAndRotation(moyenneGaze, Quaternion.identity);

                nbFrame++;
                if (nbFrame == nbFramesMax)
                {
                    nbFrame = 0;
                }
            }

            // Cinématique inverse pour la main
            GetComponent<handControl>().SetHandPosition(grabedObject.transform);
            //transform.GetChild(1).SetPositionAndRotation(grabedObject.transform.position, Quaternion.identity);
        }
        else // Rien n'est attrapé
        {
            GetComponent<handControl>().ResetHandPosition();
            //transform.GetChild(1).SetPositionAndRotation(positionDeReposDeLaMain.position/*.TransformPoint(-4,9,-15)*/, positionDeReposDeLaMain.rotation);
        }
    }
}
