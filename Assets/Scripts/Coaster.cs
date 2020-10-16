using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coaster : MonoBehaviour
{
    [SerializeField] private GameObject[] trackPrefabs;
    private List<Track> m_trackPieces = new List<Track>();

    [SerializeField] private int m_loadTrackAmount = 20;
    Vector3 prevLastTrackPartPos = Vector3.zero;

    private int m_currentTracksSinceLastLoop = 0;
    [SerializeField] private int m_loopEveryTrackAmount = 15;
    private bool m_firstLoopBuilt;
    private int m_nextRandomGameplayPiece;

    [SerializeField] private GameObject[] loopPrefabs;
    [SerializeField] private GameObject[] bigLoopPrefabs;
    private Queue<GameObject> buildQueue = new Queue<GameObject>();

    private bool coasterGameOver = false;

    public Track GetTrackByID(int a_ID) => m_trackPieces[a_ID];
    public Track GetNextTrack(Track a_currentTrack)
    {
        for (int i = 0; i < m_trackPieces.Count; i++)
        {
            if (a_currentTrack == m_trackPieces[i]) return m_trackPieces[i + 1];
        }

        Debug.LogAssertion("NO NEXT TRACK FOUND");
        return null;
    }

    [SerializeField] private Guesser c_guesser;

    private void Awake()
    {
        StartCoroutine(BuildCoaster());
    }

    private IEnumerator BuildCoaster()
    {
        for (int i = 0; i < m_loadTrackAmount; i++)
        {
            GameObject randomTrackPiece = trackPrefabs[Random.Range(0, trackPrefabs.Length)];

            GameObject trackPieceInstance = Instantiate(randomTrackPiece, transform);
            Vector3 placementPos = prevLastTrackPartPos - trackPieceInstance.GetComponent<Track>().GetTrackMovePartByID(0).transform.position;
            trackPieceInstance.transform.position = placementPos;

            m_trackPieces.Add(trackPieceInstance.GetComponent<Track>());

            TrackMovePoint lastPartInTrack = trackPieceInstance.GetComponent<Track>().GetLastTrackMovePoint();
            prevLastTrackPartPos = lastPartInTrack.transform.position;
        }
        yield return null;
    }

    public void BuildNewTrackAndCullPrevious()
    {
        if (m_currentTracksSinceLastLoop < m_loopEveryTrackAmount && buildQueue.Count == 0)
        {
            BuildTrack();
            m_currentTracksSinceLastLoop++;
        }
        else
        {
            if (buildQueue.Count == 0)
            {
                GameObject[] loopVariantPrefabs;

                int whatLoop = Random.Range(0, 1);
                if (whatLoop == 0) loopVariantPrefabs = loopPrefabs;
                else loopVariantPrefabs = bigLoopPrefabs;

                foreach (GameObject loopPrefab in loopPrefabs)
                {
                    buildQueue.Enqueue(loopPrefab);
                }
            }

            if (m_firstLoopBuilt) m_nextRandomGameplayPiece = GetRandomTrackOffset(3, 7);
            m_firstLoopBuilt = true;

            BuildTrack(buildQueue.Dequeue());

            m_currentTracksSinceLastLoop = 0;
        }

        Destroy(m_trackPieces[0].gameObject);
        m_trackPieces.RemoveAt(0);

        //RandomGameplayPart
        if (m_firstLoopBuilt && m_currentTracksSinceLastLoop == m_nextRandomGameplayPiece && buildQueue.Count == 0)
        {
            coasterGameOver = c_guesser.GetGameOver();
            if (!coasterGameOver)
                SendGameplayTrackToGuess();
            else
            {
                c_guesser.UnsetButtonBar();
            }
        }

    }

    private void BuildTrack(GameObject a_trackToBuild = null)
    {
        GameObject trackPieceToBuild = a_trackToBuild;
        if (a_trackToBuild == null)
        {
            trackPieceToBuild = trackPrefabs[Random.Range(0, trackPrefabs.Length)];
        }

        GameObject trackPieceInstance = Instantiate(trackPieceToBuild, transform);
        Vector3 placementPos = prevLastTrackPartPos - trackPieceInstance.GetComponent<Track>().GetTrackMovePartByID(0).transform.position;
        trackPieceInstance.transform.position = placementPos;

        m_trackPieces.Add(trackPieceInstance.GetComponent<Track>());

        TrackMovePoint lastPartInTrack = trackPieceInstance.GetComponent<Track>().GetLastTrackMovePoint();
        prevLastTrackPartPos = lastPartInTrack.transform.position;

    }

    private int GetRandomTrackOffset(int a_min, int a_max)
    {
        return Random.Range(a_min, a_max);
    }

    private void SendGameplayTrackToGuess()
    {
        Track trackToSend = m_trackPieces[m_trackPieces.Count - 1];
        c_guesser.SetToGuessTrack(trackToSend);
    }
}
