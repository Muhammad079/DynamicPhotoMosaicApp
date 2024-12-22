using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Renderer _camImg;
    [SerializeField] private float _transitionTime;
    [SerializeField] private Vector3 _photoRendererTargetSize;

    private WebCamTexture _webCamTexture;
    private Texture2D _capturedPhoto;
    private int _index;
    private bool isCameraStarted = false;

    private void Start()
    {
        EventsManager.Instance.OnStartPhotoCapture += StartCamera;
        EventsManager.Instance.OnRetakePhoto += RetakePhoto;
        EventsManager.Instance.OnPhotoSaved += ResetPhotoRenderer;
    }

    public void StartCamera()
    {
        StopCamera();
        if (isCameraStarted)
        {
            _webCamTexture.Stop();
        }
        _webCamTexture = new WebCamTexture();
        _camImg.material.mainTexture = _webCamTexture;
        _webCamTexture.Play();
        AnimatePhotoRenderer();
        isCameraStarted = true;
    }

    public void StopCamera()
    {
        if (_webCamTexture != null && _webCamTexture.isPlaying)
        {
            _webCamTexture.Stop();
        }
    }

    public void CapturePhoto()
    {
        if (_webCamTexture != null && _webCamTexture.isPlaying)
        {
            _webCamTexture.Pause();

            _capturedPhoto = new Texture2D(_webCamTexture.width, _webCamTexture.height, TextureFormat.RGB24, false);
            _capturedPhoto.SetPixels(_webCamTexture.GetPixels());
            _capturedPhoto.Apply();

            _camImg.material.mainTexture = _capturedPhoto;
        }
        else
        {
            Debug.LogError("Camera is not started or is not playing.");
        }
    }

    public void RetakePhoto()
    {
        StopCamera();
        StartCamera();
    }

    public void SavePhoto()
    {
        if (_capturedPhoto != null)
        {
            string path = Application.dataPath + "/Photos/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fileName = "UserPhoto_" + ++_index +DateTime.Now.ToString("yyyymmdd_hhmmss") + ".jpg";
            File.WriteAllBytes(Path.Combine(path, fileName), _capturedPhoto.EncodeToJPG());
            EventsManager.Instance.AddToCollage(_capturedPhoto);
        }
        else
        {
            Debug.LogError("No photo captured to save.");
        }
    }

    public void AnimatePhotoRenderer()
    {
        StopAllCoroutines();
        StartCoroutine(OnAnimatePhotoRenderer(_photoRendererTargetSize));
    }

    public void ResetPhotoRenderer()
    {
        StopAllCoroutines();
        StartCoroutine(OnAnimatePhotoRenderer(Vector3.zero));
    }

    private IEnumerator OnAnimatePhotoRenderer(Vector3 target)
    {
        float elapsedTime = 0;
        while (elapsedTime < _transitionTime)
        {
            elapsedTime += Time.deltaTime;
            _camImg.transform.localScale = Vector3.Lerp(_camImg.transform.localScale, target, elapsedTime / _transitionTime);
            yield return null;
        }
        _camImg.transform.localScale = target;
    }
}
