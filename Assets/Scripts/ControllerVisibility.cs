using UnityEngine;

public class ControllerVisibility : MonoBehaviour
{
    [SerializeField] private GameObject leftControllerVisual;
    [SerializeField] private GameObject rightControllerVisual;

    public void OnPassthroughToggled(bool passthroughOn)
    {
        leftControllerVisual.SetActive(!passthroughOn);
        rightControllerVisual.SetActive(!passthroughOn);
    }
}