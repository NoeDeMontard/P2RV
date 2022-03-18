using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handControl : MonoBehaviour
{
    // Cinématique inverse
    public Transform positionDeReposDeLaMain;
    

    public Transform positionDeLaMain;

    // Start is called before the first frame update
    void Start()
    {
        
        positionDeLaMain = positionDeReposDeLaMain;
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetChild(1).SetPositionAndRotation(positionDeLaMain.position, positionDeLaMain.rotation);
    }

    public void ResetHandPosition()
    {
        positionDeLaMain = positionDeReposDeLaMain;
    }
    public void SetHandPosition(Transform position)
    {
        positionDeLaMain = position;
    }
}
