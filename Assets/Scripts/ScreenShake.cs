using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public IEnumerator Shake(float _duration)
    {
        gameObject.SetActive(true);

        yield return new WaitForSeconds(_duration);

        gameObject.SetActive(false);        
    }
}
