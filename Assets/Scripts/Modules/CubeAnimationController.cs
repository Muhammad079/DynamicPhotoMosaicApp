using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CubeAnimationController : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int initialCubeCount = 100;
    [SerializeField] private float highlightDuration = 2f;

    [Header("Formation Settings")]
    [SerializeField] private float sphereRadius = 5f;
    [SerializeField] private float gridDistance = 10f;
    [SerializeField] private float animationDuration = 2f;
    [SerializeField] private float rotationSpeed = 30f;

    private List<Transform> cubeTransforms = new List<Transform>();
    private Vector3[] spherePositions;
    private Vector3[] gridPositions;
    private Transform cameraTransform;
    private bool isAnimating = false;
    private bool isInSphereMode = true;
    private int gridWidth;
    private int gridHeight;
    private List<Vector3> cachedDirections;

    public Action<GameObject> UserPhotoAdded;
    public Action<Texture2D> ProcessColorsForReferencePhoto;
    public Action StateReady;
    private GameObject cubeContainer;
    private void Awake()
    {
        cameraTransform = Camera.main.transform;
        calculateGridSize();
        cachedDirections = new List<Vector3>(initialCubeCount);
        InitializeCubes();
        InvokeRepeating(nameof(ToggleAnimations), 5, 5);

    }
    void calculateGridSize()
    {
        gridWidth = Mathf.CeilToInt(Mathf.Sqrt(initialCubeCount));
        gridHeight = gridWidth;
    }
    private void Start()
    {
        EventsManager.Instance.AddToCollage += AddNewPhoto;
        ClaculateBothPositionforGridAndSphere();
        StartCoroutine(AnimateToSphere());
    }

    private void InitializeCubes()
    {
        cubeContainer = new GameObject("CubeContainer");
        cubeContainer.transform.parent = transform;

        for (int i = 0; i < initialCubeCount; i++)
        {
            GameObject cube = Instantiate(cubePrefab, UnityEngine.Random.insideUnitSphere * 10f, Quaternion.identity);
            cube.transform.parent = cubeContainer.transform;
            cubeTransforms.Add(cube.transform);
            UserPhotoAdded?.Invoke(cube);
        }
    }

    private void ClaculateBothPositionforGridAndSphere()
    {
        CalculateSpherePositions();
        CalculateGridPositions();
    }

    private void CalculateSpherePositions()
    {
        spherePositions = new Vector3[cubeTransforms.Count];
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < cubeTransforms.Count; i++)
        {
            float t = (float)i / (cubeTransforms.Count - 1);
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = sphereRadius * Mathf.Sin(inclination) * Mathf.Cos(azimuth);
        float y = sphereRadius * Mathf.Sin(inclination) * Mathf.Sin(azimuth);
        float z = sphereRadius * Mathf.Cos(inclination);

            spherePositions[i] = new Vector3(x, y, z);
            if (cachedDirections.Count > i)
            { cachedDirections[i] = spherePositions[i].normalized; }
            else
                cachedDirections.Add(spherePositions[i].normalized);
        }
    }

    private void CalculateGridPositions()
    {
        gridPositions = new Vector3[cubeTransforms.Count];
        float aspectRatio = Screen.width / (float)Screen.height;
        float viewHeight = 2.0f * gridDistance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
      float viewWidth = viewHeight * aspectRatio;

            float startX = -(viewWidth * 0.4f);
        float startY = -(viewHeight * 0.4f);
            float stepX = (viewWidth * 0.8f) / (gridWidth - 1);
        float stepY = (viewHeight * 0.8f) / (gridHeight - 1);

            Vector3 basePosition = cameraTransform.position + cameraTransform.forward * gridDistance;

        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                int index = i * gridWidth + j;
                if (index < cubeTransforms.Count)
                {
                    gridPositions[index] = basePosition +
                        cameraTransform.right * (startX + j * stepX) +
                        cameraTransform.up * (startY + i * stepY);
                }
            }
        }
    }

    private IEnumerator SHowNewCubeFirstPrioritize(Transform cubeTransform)
    {
        Vector3 frontPosition = cameraTransform.position + cameraTransform.forward * 3f;
        Quaternion originalRotation = cubeTransform.rotation;
        float elapsedTime = 0f;
        while (elapsedTime < highlightDuration)
        {
            cubeTransform.position = Vector3.Lerp(cubeTransform.position, frontPosition, elapsedTime / highlightDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cubeTransform.rotation = originalRotation;
        ClaculateBothPositionforGridAndSphere();
    }
    private void Update()
    {
        if (!isAnimating && isInSphereMode)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

            Vector3 cameraPos = cameraTransform.position;
            foreach (Transform cubeTransform in cubeTransforms)
            {
                cubeTransform.LookAt(cameraPos);
            }
        }

    }
    //public Texture2D newTexture;
    [ContextMenu("Add new photo")]
    public void AddNewPhoto(Texture2D newTexture )
    {
        AddNewCube(newTexture);
    }

    public void AddNewCube(Texture2D photo)
    {
        GameObject newCube = Instantiate(cubePrefab, cameraTransform.position + cameraTransform.forward * 3f, Quaternion.identity);
        newCube.transform.parent = cubeContainer.transform;
        cubeTransforms.Add(newCube.transform);

        Renderer renderer = newCube.GetComponent<Renderer>();
        renderer.material.mainTexture = photo;

        StartCoroutine(SHowNewCubeFirstPrioritize(newCube.transform));
        UserPhotoAdded?.Invoke(newCube);
        
        initialCubeCount++;
            
            calculateGridSize();
        StateReady?.Invoke();
        
        
                CalculateGridPositions(); // remove that later
            CalculateSpherePositions();  // remve that later
        ClaculateBothPositionforGridAndSphere();
                ToggleAnimations();
            ProcessColorsForReferencePhoto?.Invoke(photo);
    }

    public void ToggleAnimations()
    {
        StopAllCoroutines();
        if (isInSphereMode)
            StartCoroutine(AnimateToGrid());
        else
            StartCoroutine(AnimateToSphere());
    }
    private IEnumerator AnimateToGrid()
    {
        isAnimating = true;
        CalculateGridPositions();
        WaitForEndOfFrame waitForFrame = new WaitForEndOfFrame();

        Quaternion initialRotation = Quaternion.identity;

        for (float t = 0; t < animationDuration; t += Time.deltaTime)
        {
            float progress = t / animationDuration;
            for (int i = 0; i < cubeTransforms.Count; i++)
            {
                cubeTransforms[i].position = Vector3.Lerp(
                    cubeTransforms[i].position,
                    gridPositions[i],
                    progress
                );
                cubeTransforms[i].rotation = initialRotation; 
                cubeTransforms[i].localScale = new Vector3(1,-1,0.01f); 
            }
            yield return waitForFrame;
        }
        
        // hard code final pos
         
        for (int i = 0; i < cubeTransforms.Count; i++)
        {
            cubeTransforms[i].position = gridPositions[i];
            cubeTransforms[i].rotation = initialRotation;
        }

        isAnimating = false;
        isInSphereMode = false;
    }
    private IEnumerator AnimateToSphere()
    {
        isAnimating = true;
        WaitForEndOfFrame waitForFrame = new WaitForEndOfFrame();

        for (float t = 0; t < animationDuration; t += Time.deltaTime)
        {
            float progress = t / animationDuration;
            for (int i = 0; i < cubeTransforms.Count; i++)
            {
                cubeTransforms[i].position = Vector3.Lerp(cubeTransforms[i].position, spherePositions[i], progress);
                cubeTransforms[i].localScale = new Vector3(1, 1, 0.01f);
                cubeTransforms[i].LookAt(cameraTransform);
            }
            yield return waitForFrame;
        }

        isAnimating = false;
        isInSphereMode = true;
        StateReady?.Invoke();
    }

}
