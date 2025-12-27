using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Animator screenCover;
    [SerializeField] GameObject btnGroup, mainGroup, settingsGroup;
    [SerializeField] GameObject pcControls, controllerControls;
    public UnityEngine.InputSystem.PlayerInput playerInput;

    [SerializeField] RectTransform settingsBtn, exitBtn;

    void Start()
    {
        OpenMain();

        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            // Executable
            settingsBtn.anchoredPosition3D = new Vector3(0,-77,0);
            exitBtn.anchoredPosition3D = new Vector3(0,-231,0);
        } else
            {
                // WebGL
                exitBtn.anchoredPosition3D = new Vector3(0,-77,0);
                settingsBtn.gameObject.SetActive(false);
                exitBtn.anchoredPosition3D = settingsBtn.anchoredPosition3D;
            }

        Application.targetFrameRate = 75;
        btnGroup.SetActive(true);
    }

    void OnControlsChanged(UnityEngine.InputSystem.PlayerInput input)
    {
        bool usingController = input.currentControlScheme == "Gamepad";
        pcControls.SetActive(!usingController);
        controllerControls.SetActive(usingController);
    }

    void LoadLevel() { SceneManager.LoadScene(1); }

    public void PlayGame()
    {
        btnGroup.SetActive(false);
        screenCover.Play("toBlack");

        // give illusion of loading
        Invoke("LoadLevel", 3);
    }

    public void OpenMain()
    {
        mainGroup.SetActive(true);
        settingsGroup.SetActive(false);
    }

    public void OpenSettings()
    {
        mainGroup.SetActive(false);
        settingsGroup.SetActive(true);
    }

    public void ExitGame() { Application.Quit(); }
}//EndScript