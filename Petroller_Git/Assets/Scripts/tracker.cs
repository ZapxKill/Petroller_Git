using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tracker : MonoBehaviour
{
    [SerializeField] ControllerFollower controllerFollower;
    [SerializeField] private Transform VrTarget;
    [SerializeField] private Transform PcTarget;
    private Transform target;
    [SerializeField] private Vector3 rotationOffset;
    // Start is called before the first frame update
    void Start()
    {
        if (controllerFollower.PcMode)
        {
            target = PcTarget;
        }
        else
        {
            target = VrTarget;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - target.position);
        transform.Rotate(rotationOffset);
    }
}
