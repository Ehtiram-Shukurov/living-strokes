using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRDrawing : MonoBehaviour
{
    public Transform brushTransform;
    public Material paintMaterial;
    public Color brushColor = new Color(1f, 0.5f, 0f, 1f);
    public float brushWidth = 0.01f;

    private GameObject currentStrokeObj;
    private Mesh frontMesh, backMesh;
    private List<Vector3> frontVerts = new List<Vector3>();
    private List<int> frontIndices = new List<int>();
    private List<Vector3> backVerts = new List<Vector3>();
    private List<int> backIndices = new List<int>();
    private bool isDrawing;
    private int strokeCount;
    public bool isMenuOpen = false;
    public Transform artworkParent;
    public GameObject leftControllerVisual;
    public GameObject rightControllerVisual;

    InputDevice rightController;
    GestureRecorder recorder;

    void Start()
    {
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        recorder = GetComponent<GestureRecorder>();
    }

    void Update()
    {
        if (!rightController.isValid)
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed);

        if (triggerPressed && !isDrawing && !recorder.IsRecording && !isMenuOpen) StartStroke();
        else if (!triggerPressed && isDrawing) EndStroke();

        if (isDrawing) UpdateStroke();
    }

    void StartStroke()
    {
        isDrawing = true;
        currentStrokeObj = new GameObject("Stroke " + strokeCount);
        if (artworkParent != null)
            currentStrokeObj.transform.SetParent(artworkParent, false);

        var frontObj = new GameObject("Front", typeof(MeshFilter), typeof(MeshRenderer));
        frontObj.transform.SetParent(currentStrokeObj.transform, false);
        var frontMat = new Material(paintMaterial);
        frontMat.color = brushColor;
        frontMat.SetColor("_BaseColor", brushColor);
        frontObj.GetComponent<MeshRenderer>().material = frontMat;
        frontMesh = frontObj.GetComponent<MeshFilter>().mesh;
        frontMesh.MarkDynamic();

        var backObj = new GameObject("Back", typeof(MeshFilter), typeof(MeshRenderer));
        backObj.transform.SetParent(currentStrokeObj.transform, false);
        backObj.GetComponent<MeshRenderer>().material = frontMat;
        backMesh = backObj.GetComponent<MeshFilter>().mesh;
        backMesh.MarkDynamic();

        frontVerts.Clear(); frontIndices.Clear();
        backVerts.Clear(); backIndices.Clear();
    }

    void UpdateStroke()
    {
        Vector3 left = brushTransform.position + brushTransform.right * -brushWidth;
        Vector3 right = brushTransform.position + brushTransform.right * brushWidth;

        frontVerts.Add(left); frontVerts.Add(right);
        backVerts.Add(left); backVerts.Add(right);

        if (frontVerts.Count >= 4)
        {
            int v0 = frontVerts.Count - 4, v1 = v0 + 1, v2 = v0 + 2, v3 = v0 + 3;

            frontIndices.AddRange(new[] { v0, v2, v3, v0, v3, v1 });
            backIndices.AddRange(new[] { v0, v3, v2, v0, v1, v3 });

            frontMesh.Clear();
            frontMesh.SetVertices(frontVerts);
            frontMesh.SetIndices(frontIndices, MeshTopology.Triangles, 0);
            frontMesh.RecalculateNormals();

            backMesh.Clear();
            backMesh.SetVertices(backVerts);
            backMesh.SetIndices(backIndices, MeshTopology.Triangles, 0);
            backMesh.RecalculateNormals();
        }
    }

    void EndStroke()
    {
        isDrawing = false;
        strokeCount++;
        recorder.StartRecording(currentStrokeObj);
    }
    public void DeleteAll()
    {
        if (artworkParent == null) return;
        foreach (Transform child in artworkParent)
            Destroy(child.gameObject);
        strokeCount = 0;
    }
    

    public void OnPassthroughToggled(bool passthroughOn)
    {
        leftControllerVisual.SetActive(!passthroughOn);
        rightControllerVisual.SetActive(!passthroughOn);
    }
}