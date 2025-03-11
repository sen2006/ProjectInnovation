using UnityEngine;

[RequireComponent (typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    Camera cam;
    Matrix4x4 camMatrix;
    [SerializeField] public bool flipped {  get; private set; }
    void Start()
    {
        cam = GetComponent<Camera>();
        camMatrix = cam.projectionMatrix;
    }

    // Update is called once per frame
    void OnPreCull()
    {
        Vector3 scale = new Vector3 (1, flipped ? -1 : 1, 1);
        cam.projectionMatrix = camMatrix * Matrix4x4.Scale(scale);
    }

    /// <summary>
    /// sets the flipped state
    /// </summary>
    /// <returns>the old state</returns>
    public bool setFlipped(bool state)
    {
        bool oldState = flipped;
        flipped = state;
        return oldState;    
    }
}
