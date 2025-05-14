using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Wwise_UI_Sound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{

    public AK.Wwise.Event UI_Menu;
    public AK.Wwise.Event UI_Whoosh;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Menu.Post(gameObject);
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        UI_Whoosh.Post(gameObject);
    }

}


