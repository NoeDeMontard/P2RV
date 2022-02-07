using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.G2OM;

public class gazeGrab : MonoBehaviour, IGazeFocusable
{
    public GameObject eye;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private bool grabed = false;
    private bool focused = false;

    private float alpha = 10;
    private Transform oldParent;

    // Update is called once per frame
    void Update()
    {
        if (focused && Tobii.XR.ControllerManager.Instance.GetButtonPressDown(Tobii.XR.ControllerButton.Trigger))
        {
            grabed = true;
            oldParent = gameObject.transform.parent;
            gameObject.transform.parent = eye.transform;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            Debug.Log("Grabbed");
        } 
        else if (focused && Tobii.XR.ControllerManager.Instance.GetButtonPressUp(Tobii.XR.ControllerButton.Trigger))
        {
            grabed = false;
            gameObject.transform.parent = oldParent;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            Debug.Log("Ungrabbed");
        }

        if (grabed)
        {
            if (Tobii.XR.ControllerManager.Instance.GetButtonTouch(Tobii.XR.ControllerButton.TouchpadTouch))
            {
                float change = Tobii.XR.ControllerManager.Instance.GetTouchpadAxis().y;
                change *= alpha;
                gameObject.transform.Translate(0, change, 0);
            }
        }
    }
    public void GazeFocusChanged(bool hasFocus)
    {
        focused = hasFocus;
    }
}
