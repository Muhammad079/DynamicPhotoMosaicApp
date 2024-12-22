using UnityEngine;
using UnityEngine.UI;

public class CapturePhotoScreen : AppScreen
{
    [SerializeField] private Button _capturePhoto_btn;
    [SerializeField] private CameraManager _cameraManager;


    private void Awake()
    {
        _capturePhoto_btn.onClick.AddListener(() =>
        {
            _cameraManager.CapturePhoto();
            UIManager.Instance.ToggleUIScreen(UIManager.RetakeScreen, true);
        });
    }
}
