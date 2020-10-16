using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public bool m_isLoopTrack;
    public bool m_destroyCartOnEnter = false;
    private TrackMovePoint[] trackMovePoints;
    private void Awake()
    {
        m_destroyCartOnEnter = false;
        trackMovePoints = GetComponentsInChildren<TrackMovePoint>();
        if (trackMovePoints.Length <= 1) Debug.LogAssertion("TRACK CONTAINS 1 OR LESS MOVEPARTS. PLEASE ADD 2+ SO THE TRAINS KNOW WHERE TO GO.");
    }

    public TrackMovePoint GetTrackMovePartByID(int a_ID)
    {
        return trackMovePoints[a_ID];
    }
    public int GetTrackMovePointAmount() => transform.childCount;
    public TrackMovePoint GetLastTrackMovePoint() => trackMovePoints[trackMovePoints.Length - 1];
    public Vector3 GetCurrentTrackPointPosition(TrackMovePoint a_currentTrackMovePoint)
    {
        foreach (TrackMovePoint trackMovePoint in trackMovePoints)  
            if (trackMovePoint == a_currentTrackMovePoint)
                return a_currentTrackMovePoint.transform.position;

        Debug.LogError("CURRENT TRACK NOT FOUND");
        return Vector3.zero;
    }

    public TrackMovePoint GetNextTrackMovePoint(TrackMovePoint a_currentTrackMovePoint)
    {
        if(IsLastPoint(a_currentTrackMovePoint))
        {
            Debug.LogError("Last PArt REached, MIGHT WANNA CHECK SOMEWHERE ELSE INSTEAD");
            return null;
        }


        for(int i = 0; i < trackMovePoints.Length - 1; i++)
        {
            if(a_currentTrackMovePoint == trackMovePoints[i])
            {
                return trackMovePoints[i + 1];
            }
        }

        Debug.LogError("NO PArt found YA DINGUS");
        Debug.Assert(false);
        return null;
    }

    public bool IsLastPoint(TrackMovePoint a_currentTrackMovePoint)
    {
        if (a_currentTrackMovePoint == trackMovePoints[trackMovePoints.Length - 1])
            return true;
        else return false;
    }

}
