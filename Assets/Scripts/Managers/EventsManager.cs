using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    public Action OnPhotoSaved;
    public Action OnStartPhotoCapture;
    public Action OnRetakePhoto;
    public Action CameraPrepared;
    public Action<Texture2D> AddToCollage;

    public static EventsManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void InvokeOnStartPhotoCapture()
    {
        OnStartPhotoCapture?.Invoke();
    }
    public void InvokeOnPhotoSaved()
    {
        OnPhotoSaved?.Invoke();
    }
    public void InvokeOnRetakePhoto()
    {
        OnRetakePhoto?.Invoke();
    }
    public void InvokeOnCameraPrepared()
    {
        CameraPrepared?.Invoke();
    }
    public void InvokeOnAddToCollage(Texture2D texture)
    {
        AddToCollage?.Invoke(texture);
    }

}
