using UnityEngine;
using System.Collections.Generic;

public class MosaicFeature : MonoBehaviour
{
    public Texture2D referencePhoto;
    public List<Texture2D> userPhotos;
    public List<GameObject> cubes = new List<GameObject>();
    private Color[] referencePhotoPixels;
    public CubeAnimationController CubeAnimationController;

    private Dictionary<int, Color> ReferencePhotoPixelColor;
    public bool useBestMatch = false; 

    private void Awake()
    {
        CubeAnimationController.UserPhotoAdded += OnUserPhotoAdded;
        CubeAnimationController.StateReady += OnStateReady;
        CubeAnimationController.ProcessColorsForReferencePhoto += ProcessMosaic;
        
        ReferencePhotoPixelColor = new Dictionary<int, Color>();
        ComputeThePhotoInitialLy();
    }

    private void Start()
    {
        ProcessMosaic();
    }

    private void ProcessMosaic(Texture2D texture2D = null)
    {
        if (texture2D)
            userPhotos.Add(texture2D);
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(cubes.Count));
        referencePhoto = ResizeTexture(referencePhoto, gridSize, gridSize);
        referencePhotoPixels = referencePhoto.GetPixels();
        MapTexturesAndColorsToAllCubesWhenGrid(gridSize);
    }

    private void ComputeThePhotoInitialLy()
    {
        for (int i = 0; i < userPhotos.Count; i++)
        {
            ReferencePhotoPixelColor[i] = GetPixelColorfromTecture(userPhotos[i], 0, 0, userPhotos[i].width, userPhotos[i].height);
        }
    }

    private Texture2D ResizeTexture(Texture2D original, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(original, rt);

        Texture2D resized = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        resized.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        resized.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return resized;
    }

    private void MapTexturesAndColorsToAllCubesWhenGrid(int gridSize)
    {
        int cubeWitdh = Mathf.Max(1, referencePhoto.width / gridSize);
        int cubeHeight = Mathf.Max(1, referencePhoto.height / gridSize);

        for (int i = 0; i < cubes.Count; i++)
        {
            int x = i % gridSize;
            int y = i / gridSize;

            if (x * cubeWitdh >= referencePhoto.width || y * cubeHeight >= referencePhoto.height)
            {
                continue; // pass for now
            }

            Color avgColor = GetColorfromRefrence(x * cubeWitdh, y * cubeHeight, cubeWitdh, cubeHeight);
            //avgColor = new Color(avgColor.r, avgColor.g, avgColor.b, 0.01f);
            int photoIndex = useBestMatch ? GetPhotoThatMatchestheColor(avgColor) : (i % userPhotos.Count);

            Renderer cubeRenderer = cubes[i].GetComponent<Renderer>();
            if (cubeRenderer != null)
            {
                cubeRenderer.material.mainTexture = userPhotos[photoIndex];
                cubeRenderer.material.color = avgColor;
            }
        }
    }

    private Color GetColorfromRefrence(int ReferenceOriginX, int ReferenceOrigiY, int Referencewidth, int referenceHeight)
    {
        Color sum = Color.black;
        int count = 0;

        for (int y = ReferenceOrigiY; y < ReferenceOrigiY + referenceHeight && y < referencePhoto.height; y++)
        {
          for (int x = ReferenceOriginX; x < ReferenceOriginX + Referencewidth && x < referencePhoto.width; x++)
          {
                    int index = y * referencePhoto.width + x;
              sum += referencePhotoPixels[index];
                count++;
          }
        }
        return count > 0 ? sum / count : Color.black;
    }



    private void OnUserPhotoAdded(GameObject newCube)
    {
        cubes.Add(newCube);
    }


    private Color GetPixelColorfromTecture(Texture2D texture, int PhotoXorigin, int PhotoYOrigin, int UserPhotowidth, int UserphotoHeight)
    {
        Color[] pixels = texture.GetPixels(PhotoXorigin, PhotoYOrigin, UserPhotowidth, UserphotoHeight);
            Color sum = Color.black;

        foreach (Color pixel in pixels)
        {
                sum += pixel;
        }
            return pixels.Length > 0 ? sum / pixels.Length : Color.black;
    }

    private int GetPhotoThatMatchestheColor(Color targetColor)
    {
        float closestDistance = float.MaxValue;
        int bestMatchIndex = 0;

        foreach (var kvp in ReferencePhotoPixelColor)
        {
            float distance = Vector3.Distance(new Vector3(kvp.Value.r, kvp.Value.g, kvp.Value.b),
        new Vector3(targetColor.r, targetColor.g, targetColor.b));
        if (distance < closestDistance)
            {
                closestDistance = distance;
                bestMatchIndex = kvp.Key;
            }
        }
        return bestMatchIndex;
    }

    private void OnStateReady()
    {
        ProcessMosaic();
    }

}
