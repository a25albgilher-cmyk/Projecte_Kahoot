using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardMotion : MonoBehaviour, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
    bool pressed;
    Vector3 rest;
    Button button;
    void Awake(){rest=transform.localScale;button=GetComponent<Button>();}
    void Update(){
        float scale = button.interactable && pressed ? 0.965f : 1f;
        transform.localScale=Vector3.Lerp(transform.localScale,rest*scale,1-Mathf.Exp(-18*Time.unscaledDeltaTime));
    }
    void OnDisable(){pressed=false;transform.localScale=rest;}
    public void OnPointerExit(PointerEventData e){pressed=false;}
    public void OnPointerDown(PointerEventData e){pressed=true;}
    public void OnPointerUp(PointerEventData e){pressed=false;}
}
