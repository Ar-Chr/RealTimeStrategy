using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class QuitGameButton : MonoBehaviour
{
    [SerializeField] private Button quitButton;

    private void Start()
    {
#if UNITY_EDITOR
        quitButton.onClick.AddListener(() => EditorApplication.isPlaying = false);
#else        
        quitButton.onClick.AddListener(Application.Quit); 
#endif
    }
}
