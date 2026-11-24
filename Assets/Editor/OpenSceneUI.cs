using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OpenSceneUI {
    [MenuItem("ProjecteKahoot/Ver interfaz en Hierarchy")]
    public static void Reveal(){
        var target=GameObject.Find("Canvas/Escenario/FormularioScroll/Viewport/Content");
        if(target==null)return;
        EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        var item=target.transform.childCount>0?target.transform.GetChild(0).gameObject:target;
        Selection.activeGameObject=item;
        EditorGUIUtility.PingObject(item);
        if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.FrameSelected();
    }

    [InitializeOnLoadMethod]
    static void OnFirstImport(){
        EditorApplication.delayCall+=()=>{
            if(EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool("Pulso.SceneOpened",false))return;
            SessionState.SetBool("Pulso.SceneOpened",true);
            var scene=SceneManager.GetActiveScene();
            if(!string.IsNullOrEmpty(scene.path)||scene.isDirty)return;
            EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity");
            Reveal();
        };
    }
}
