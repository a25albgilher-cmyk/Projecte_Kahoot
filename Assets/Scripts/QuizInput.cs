using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

[DefaultExecutionOrder(-200)]
[RequireComponent(typeof(EventSystem))]
public sealed class QuizInput : MonoBehaviour {
    void Awake() {
        var module = gameObject.AddComponent<InputSystemUIInputModule>();
        if (module.actionsAsset == null) module.AssignDefaultActions();
    }
}
