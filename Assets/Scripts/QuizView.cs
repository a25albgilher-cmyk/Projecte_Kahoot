using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuizView {
    public readonly Color ink = Hex("#17132C"), panel = Hex("#372C53"), mint = Hex("#CAF576");
    public readonly Color[] tiles = {Hex("#325CBA"),Hex("#A33B65"),Hex("#167162"),Hex("#7046AE")};
    public RectTransform body;
    public ScrollRect scroll;
    public GameObject canvas;
    readonly List<Selectable> controls=new List<Selectable>();
    readonly Font font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    GameObject previous;
    public static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex,out var c);return c; }
    static RectTransform Box(string name,Transform parent) {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent,false);return r;
    }
    static void Stretch(RectTransform r,float inset=0) {
        r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(inset,inset);r.offsetMax=new Vector2(-inset,-inset);
    }
    static void Paint(RectTransform r,Color c) {r.gameObject.AddComponent<Image>().color=c;}
    readonly Dictionary<Transform,Dictionary<string,int>> counters=new Dictionary<Transform,Dictionary<string,int>>();
    readonly HashSet<SceneControl> used=new HashSet<SceneControl>();
    readonly bool fixedMenu;
    public RectTransform meter;
    static T Get<T>(GameObject obj) where T:Component => obj.GetComponent<T>()??obj.AddComponent<T>();
    RectTransform Slot(Transform parent,string type,string name,out bool authored) {
        if(!counters.TryGetValue(parent,out var seq))counters[parent]=seq=new Dictionary<string,int>();
        seq.TryGetValue(type,out int number);seq[type]=number+1;
        string id=type+(number+1);
        foreach(Transform child in parent){
            var marker=child.GetComponent<SceneControl>();
            if(marker==null||marker.slot!=id)continue;
            used.Add(marker);if(!fixedMenu)child.SetAsLastSibling();child.gameObject.SetActive(true);authored=true;return (RectTransform)child;
        }
        authored=false;var r=Box(name,parent);var dynamicSlot=r.gameObject.AddComponent<SceneControl>();dynamicSlot.slot=id;
        used.Add(dynamicSlot);return r;
    }
    // Slots omitted by the current data are hidden (e.g. an optional third answer).
    public void Finish(){
        foreach(var slot in body.GetComponentsInChildren<SceneControl>(true))
            if(!used.Contains(slot))slot.gameObject.SetActive(false);
    }
    public QuizView(string heading) {
        canvas=GameObject.Find("Canvas");
        var shell=canvas.transform.Find("Escenario");
        shell.Find("Cabecera/Titulo").GetComponent<Text>().text=heading;
        meter=shell.Find("Cabecera/Reloj/Relleno") as RectTransform;
        var frame=shell.Find("FormularioScroll");
        body=(RectTransform)frame.Find("Viewport/Content");
        fixedMenu=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="MenuPrincipal";
        var scale=Get<CanvasScaler>(canvas);scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scale.referenceResolution=new Vector2(1000,750);scale.matchWidthOrHeight=.5f;
        Get<GraphicRaycaster>(canvas);
        scroll=Get<ScrollRect>(frame.gameObject);scroll.horizontal=false;scroll.vertical=!fixedMenu;
        scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=36;
        scroll.viewport=(RectTransform)frame.Find("Viewport");scroll.content=body;
        Get<RectMask2D>(scroll.viewport.gameObject);
        if(!fixedMenu){
            var rows=Get<VerticalLayoutGroup>(body.gameObject);rows.spacing=12;rows.padding=new RectOffset(8,8,5,18);
            rows.childForceExpandHeight=false;rows.childControlHeight=true;rows.childControlWidth=true;rows.childForceExpandWidth=true;
            Get<ContentSizeFitter>(body.gameObject).verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        }
    }
    Text Raw(RectTransform r,string value,int size,Color color) {
        var t=Get<Text>(r.gameObject);t.font=font;t.fontSize=size;t.text=value;t.color=color;
        t.supportRichText=false;t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;return t;
    }
    Text Label(Transform parent,string value,int size,Color color,float height) {
        var r=Slot(parent,"L","Texto",out var authored);
        var t=r.GetComponent<Text>();if(t==null)t=Raw(r,value,size,color);else {t.text=value;if(t.font==null)t.font=font;}
        Get<LayoutElement>(r.gameObject).minHeight=height;return t;
    }
    public Text Label(string value,int size=22) => Label(body,value,size,Color.white,36);
    public RectTransform Row(string name) {
        var row=Slot(body,"R",name,out _);
        var layout=Get<HorizontalLayoutGroup>(row.gameObject);
        layout.spacing=10;layout.childControlWidth=true;layout.childControlHeight=true;
        layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
        return row;
    }
    public Button Button(string value,Action click,Color? tint=null,bool enabled=true,Transform parent=null,float height=54) {
        var r=Slot(parent==null?body:parent,"B",value,out bool authored);
        if(!authored)Paint(r,tint??panel);
        var element=Get<LayoutElement>(r.gameObject);element.minHeight=height;element.flexibleWidth=1;
        var b=Get<Button>(r.gameObject);b.onClick.RemoveAllListeners();b.targetGraphic=r.GetComponent<Image>();b.interactable=enabled;
        var label=r.Find("Etiqueta") as RectTransform;
        if(label==null){label=Box("Etiqueta",r);Stretch(label,14);}
        var t=label.GetComponent<Text>();if(t==null)t=Raw(label,value,20,Color.white);else {t.text=value;if(t.font==null)t.font=font;}t.resizeTextForBestFit=true;t.resizeTextMinSize=12;t.resizeTextMaxSize=fixedMenu?30:22;
        var colors=b.colors;colors.selectedColor=colors.normalColor;colors.highlightedColor=colors.normalColor;colors.pressedColor=colors.normalColor;
        colors.colorMultiplier=1.25f;b.colors=colors;if(!fixedMenu || b.onClick.GetPersistentEventCount()==0)b.onClick.AddListener(()=>click());controls.Add(b);return b;
    }
    public InputField Field(string label,string value,Action<string> change,bool numeric=false) {
        Label(label,16);var r=Slot(body,"F",label,out bool authored);if(!authored)Paint(r,Color.white);
        Get<LayoutElement>(r.gameObject).minHeight=48;
        var input=Get<InputField>(r.gameObject);input.onValueChanged.RemoveAllListeners();input.targetGraphic=r.GetComponent<Image>();
        var text=r.Find("Valor") as RectTransform;if(text==null){text=Box("Valor",r);Stretch(text,9);}var t=Raw(text,"",20,ink);input.textComponent=t;
        input.contentType=numeric?InputField.ContentType.IntegerNumber:InputField.ContentType.Standard;
        input.SetTextWithoutNotify(value);input.onValueChanged.AddListener(s=>change(s));controls.Add(input);return input;
    }
    public void Focus(Selectable item=null) {
        var choice=item??controls.FirstOrDefault(s=>s!=null && s.IsInteractable());
        if(choice!=null && EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(choice.gameObject);
    }
    public int SelectionIndex() {
        var current=EventSystem.current.currentSelectedGameObject;
        return controls.FindIndex(s=>s!=null && s.gameObject==current);
    }
    public void FocusIndex(int index) {
        if(controls.Count==0)return;
        int i=Mathf.Clamp(index,0,controls.Count-1);
        while(i>0 && !controls[i].IsInteractable())i--;
        Focus(controls[i]);
    }
    public void Navigation() {
        var es=EventSystem.current;if(es==null)return;
        var current=es.currentSelectedGameObject;
        if(Input.GetKeyDown(KeyCode.Tab)) {
            var live=controls.Where(s=>s!=null && s.IsInteractable()).ToList();
            if(live.Count>0) {
                int i=live.FindIndex(s=>s.gameObject==current),delta=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)?-1:1;
                Focus(live[(i+delta+live.Count)%live.Count]);current=es.currentSelectedGameObject;
            }
        }
        if(current==null || current==previous)return;
        if(previous!=null && previous.TryGetComponent<Outline>(out var oldOutline))oldOutline.enabled=false;
        previous=current;if(!current.transform.IsChildOf(body))return;
        var outline=current.GetComponent<Outline>()??current.AddComponent<Outline>();
        outline.effectColor=mint;outline.effectDistance=new Vector2(2,-2);outline.enabled=true;
        if(fixedMenu)return;
        Canvas.ForceUpdateCanvases();
        var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport,current.transform);
        float shift=bounds.min.y<scroll.viewport.rect.yMin?scroll.viewport.rect.yMin-bounds.min.y:
            Mathf.Min(0,scroll.viewport.rect.yMax-bounds.max.y);
        var pos=body.anchoredPosition;pos.y=Mathf.Clamp(pos.y+shift,0,Mathf.Max(0,body.rect.height-scroll.viewport.rect.height));body.anchoredPosition=pos;
    }
}
