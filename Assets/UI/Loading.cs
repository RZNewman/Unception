using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Image loadingImage;
    public Sprite wheel;
    public Sprite anchor;
    
    public enum LoadingType
    {
        None,
        Loading,
        Landed
    }

    public void setType(LoadingType type)
    {
        Spinner spin = loadingImage.GetComponent<Spinner>();
        loadingImage.gameObject.SetActive(type != LoadingType.None);
        switch (type)
        {
            case LoadingType.Loading:
                loadingImage.sprite = wheel;
                spin.rotationSpeed = 300;
                break;
            case LoadingType.Landed:
                loadingImage.sprite = anchor;
                spin.rotationSpeed = 0;
                break;

        }
        spin.reset();
    }
}
