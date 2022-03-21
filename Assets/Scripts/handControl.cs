using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handControl : MonoBehaviour
{
    // Cinématique inverse
    public Transform positionDeReposDeLaMainGauche;
    public Transform positionDeReposDeLaMainDroite;

    private void Update()
    {
        transform.GetChild(2).SetPositionAndRotation(positionDeReposDeLaMainDroite.position, positionDeReposDeLaMainDroite.rotation);
    }
    public void ResetHandPosition()
    {
        transform.GetChild(1).SetPositionAndRotation(positionDeReposDeLaMainGauche.position, positionDeReposDeLaMainGauche.rotation);
        //transform.GetChild(2).SetPositionAndRotation(positionDeReposDeLaMainDroite.position, Quaternion.identity);
    }
    public void SetHandPosition(Transform position)
    {
        transform.GetChild(1).position = position.position;
        
        //transform.GetChild(2).SetPositionAndRotation(positionDeReposDeLaMainDroite.position, Quaternion.identity);

    }
}
