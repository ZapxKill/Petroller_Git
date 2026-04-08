using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerFollower : MonoBehaviour
{
    public OVRInput.Controller targetController;
    public bool PcMode = false;
    public bool useController = false;
    [SerializeField] Camera PcCamera;
    [SerializeField] Camera VrCamera;
    public bool autoInitializeModel;
    public Transform modelTransform;
    public Vector3 model_initialPos, model_initialRot;
    private Vector3 lastKnownPosition;
    private Quaternion lastKnownRotation;

    void Start()
    {
        if (modelTransform == null)
            modelTransform = this.transform.GetChild(0).transform;
        if(autoInitializeModel)
            modelTransform.position = model_initialPos;
            modelTransform.rotation = Quaternion.Euler(model_initialRot);
            lastKnownPosition = transform.position;
        if (PcMode)
        {
            PcCamera.enabled = true;
            VrCamera.targetDisplay = 2;
        }
        else
        {
            PcCamera.enabled = false;
        }
    }
    void Update()
    {
        transform.rotation = OVRInput.GetLocalControllerRotation(targetController);
        lastKnownRotation = transform.rotation;
        if (OVRInput.GetControllerPositionTracked(targetController))
        {
            if (!PcMode)
            {
                transform.position = OVRInput.GetLocalControllerPosition(targetController);
            }
            
            lastKnownPosition = transform.position;
            
        }
        else
        {
            transform.position = lastKnownPosition;
            transform.rotation = lastKnownRotation;
        }
    }
}
