using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MenuRoute : MonoBehaviour {
    public void Open(string sceneName) {
        if (!Application.CanStreamedLevelBeLoaded(sceneName)) {
            Debug.LogError("Escena ausente en Build Settings: " + sceneName);
            return;
        }
        ProjecteKahoot.FromMenu(sceneName);
    }
}
