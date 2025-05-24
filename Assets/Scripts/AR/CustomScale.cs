using UnityEngine;

public class CustomScale : MonoBehaviour
{
    [Tooltip("The scale factor to apply to this prefab when spawned")]
    public float scaleFactor = 1.0f;

    [Tooltip("If true, this scale will override any other scaling logic")]
    public bool overrideDefaultScaling = true;
} 