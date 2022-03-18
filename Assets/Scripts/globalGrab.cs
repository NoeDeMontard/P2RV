using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.G2OM;

public class globalGrab : MonoBehaviour
{

    public RaycastHit looked;
    public GameObject grabed;
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

    /*// Cinématique inverse
    public Transform positionDeReposDeLaMain;
    public Quaternion positionDeReposDeLaMainDecalage;*/
    protected handControl handControl;

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

    // Start is called before the first frame update
    void Start()
    {
        handControl = GetComponent<handControl>();
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
        grab = grab || (eyesClosed && grabed==null);

        ungrab = Tobii.XR.ControllerManager.Instance.GetButtonPressUp(Tobii.XR.ControllerButton.Trigger);
        ungrab = ungrab || (eyesClosed && grabed!=null);

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

        //looked = null;
        if (gazeRay.IsValid)
        {
            Ray ray = new Ray(Camera.main.transform.position, gazeRay.Direction);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                // sauvegarde de l'objet regardé
                looked = hit;
                // rapprocher l'objet s'il est derriere un mur
                if (!hit.transform.gameObject.CompareTag("grabbable"))
                {
                    maxobjectDistance = hit.distance;
                    if (maxobjectDistance < 5 * minobjectDistance)
                        maxobjectDistance = 5 * minobjectDistance;
                }
            }
        }

        if (grab)
        {
            if (looked.transform!=null && looked.transform.gameObject.CompareTag("grabbable"))
            {
                objectDistance = looked.distance;
                grabed = looked.transform.gameObject;
                
                // Code permettant de highlight l'objet grabbed 
                _originalColor = grabed.GetComponent<Renderer>().material.GetColor(_emissionColor);
                var newColor = Color.Lerp(_originalColor, highlightColor, 1.0f);
                grabed.GetComponent<Renderer>().material.SetColor(_emissionColor, newColor);
            }

            blinkingTime = 0;
            
            Debug.Log("Grabbed");
        } 
        else if (ungrab)
        {
            // On redonne la couleur initiale à l'objet avant de le relacher
            grabed.GetComponent<Renderer>().material.SetColor(_emissionColor, _originalColor);
            grabed = null;
            Debug.Log("Ungrabbed");

        }

        if (grabed != null)
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
                Ray ray = new Ray(Camera.main.transform.position, gazeRay.Direction);

                // On stablisise l'objet attrapé par une moyenne
                dataGaze[nbFrame] = ray.GetPoint(objectDistance);
                moyenneGaze = moyenne(dataGaze);

                grabed.transform.SetPositionAndRotation(moyenneGaze, Quaternion.identity);

                nbFrame++;
                if (nbFrame == nbFramesMax)
                {
                    nbFrame = 0;
                }
            }

            // Cinématique inverse pour la main
            handControl.SetHandPosition(grabed.transform);
            //transform.GetChild(1).SetPositionAndRotation(grabed.transform.position, Quaternion.identity);
        }
        else // Rien n'est attrapé
        {
            handControl.ResetHandPosition();
            //transform.GetChild(1).SetPositionAndRotation(positionDeReposDeLaMain.TransformPoint(-4,9,-15), positionDeReposDeLaMain.rotation);
        }
    }
}
