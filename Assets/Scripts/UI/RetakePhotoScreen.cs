using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RetakePhotoScreen : AppScreen
{
    [SerializeField] private Button _retakePhoto_btn;
    [SerializeField] private Button _savePhoto_btn;
    [SerializeField] private CameraManager _cameraManager;

    public void Awake()
    {
        _retakePhoto_btn.onClick.AddListener(() => {
            UIManager.Instance.ToggleUIScreen(UIManager.CaptureScreen, true);
            EventsManager.Instance.InvokeOnRetakePhoto();
        });
        _savePhoto_btn.onClick.AddListener(() =>
        {
            _cameraManager.SavePhoto();
            UIManager.Instance.ToggleUIScreen(UIManager.StartCameraScreen, true);
            // start the animation
            EventsManager.Instance.InvokeOnPhotoSaved();
            // photosupdated invoke
        });
    }
}
