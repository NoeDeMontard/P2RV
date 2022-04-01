using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class initialPosition : MonoBehaviour
{
    float tailleAvatar = 1.63f;
    GameObject cameraOffset;
    Vector3 cameraPos;
    Vector3 newOffset;
    Quaternion newAngleOffset;
    bool pas_change = true;
    float angle_y = 0;

    // Start is called before the first frame update
    void Start()
    {
        cameraOffset = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        //on attend que la position de la camera prenne une valeur differente de zero
        if (pas_change)
        {
            cameraPos = cameraOffset.transform.GetChild(0).gameObject.transform.position;
            angle_y = cameraOffset.transform.GetChild(0).gameObject.transform.rotation.eulerAngles.y;
            
            if (cameraPos != Vector3.zero)
            {
                pas_change = false;
                newAngleOffset = Quaternion.AngleAxis(-angle_y, Vector3.up);
                newOffset = new Vector3(-cameraPos.x, -cameraPos.y + tailleAvatar, -cameraPos.z);
                newOffset = newAngleOffset * newOffset;
                cameraOffset.transform.SetPositionAndRotation(newOffset, newAngleOffset);
            }
        }
        
    }
}
