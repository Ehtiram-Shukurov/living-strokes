using UnityEngine;

public class HandMenuState : MonoBehaviour
{
    public VRDrawing drawing;
    public GameObject menuFollowObject;

    void Update()
    {
        if (drawing != null && menuFollowObject != null)
            drawing.isMenuOpen = menuFollowObject.activeSelf;
    }
}