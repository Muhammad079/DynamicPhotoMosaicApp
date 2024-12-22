using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private AppScreen[] Screens;

    public static UIManager Instance;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void ToggleUIScreen(string _ScreenName, bool ToggleStatus)
    {
        foreach (var item in Screens)
        {
            if (item.ScreenName == _ScreenName && !item.isOpen)
                item.MainPanel.SetActive(ToggleStatus);
            else
            {
                item.MainPanel.SetActive(!ToggleStatus);
            }
        }
    }

    public static string RetakeScreen = "PhotoRetakeScreen";
    public static string CaptureScreen = "CaptureScreen";
    public static string StartCameraScreen = "StartCameraScreen";

}
