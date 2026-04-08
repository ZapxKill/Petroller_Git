using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpirementLogFunction : MonoBehaviour
{
    public RoleType role;
    public TrackingState currentTrackingState;
    int triggerCount = 0;

    public void SimpleRecord()
    {
        triggerCount++;
        RecordCSVWriter.CSV_WriteByCase(role.ToString() + "_"+ currentTrackingState.ToString(), true, true, triggerCount.ToString());
    }
}

[System.Serializable]
public enum RoleType
{
    Observer,
    Participant
}

[System.Serializable]
public enum TrackingState
{
    Tracking,
    Lost
}
