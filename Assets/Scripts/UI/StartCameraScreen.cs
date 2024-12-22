using UnityEngine;
using UnityEngine.UI;

public class StartCameraScreen : AppScreen
{
    [SerializeField] private Button _startCamera_btn;
    [SerializeField] private CameraManager _cameraManager;
    

    private void Awake()
    {
        _startCamera_btn.onClick.AddListener(() =>
        {
            UIManager.Instance.ToggleUIScreen(UIManager.CaptureScreen, true);
            EventsManager.Instance.InvokeOnStartPhotoCapture();
        });
    }
}
