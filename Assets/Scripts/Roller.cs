using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roller : MonoBehaviour
{
    [SerializeField] private float m_speed;

    [Header("Carts")]
    [SerializeField] private GameObject rollerCartPrefab;
    [Space(10)]
    [SerializeField] private int m_cartAmount = 3;
    [SerializeField] private float m_cartDistance = 1.0f;
    [SerializeField] private int m_followDelay;
    

    [Header("CoasterRef (TEMP)")]
    [SerializeField] private Coaster c_coaster;
    [SerializeField] private GameOverSystem c_gameOverSystem;
    [SerializeField] private ScoreSystem c_scoreSystem;

    [Header ("Camera")]
    [SerializeField] private Camera c_camera;
    [SerializeField] private Vector3 m_cameraOffset;
    [SerializeField] private float m_removeTrackOffset;

    private List<Transform> m_cartTransforms = new List<Transform>();

    private bool rollerBuilt;

    private void Awake()
    {
        Time.timeScale = 1.0f;
    }

    private void Start()
    {
        StartCoroutine(SpawnCarts());
    }

    private void Update()
    {
    }

    private void LateUpdate()
    {
        if (!rollerBuilt) return;
        c_camera.transform.position = CalculateCameraPosition();
        c_camera.transform.position += Vector3.forward * -10.0f;

        if(m_cartTransforms[0].transform.position.x > c_coaster.GetTrackByID(0).transform.position.x + m_removeTrackOffset)
        {
            c_coaster.BuildNewTrackAndCullPrevious();
        }
    }

    private IEnumerator SpawnCarts()
    {
        for (int cartID = 0; cartID < m_cartAmount; cartID++)
        {
            GameObject cart = Instantiate(rollerCartPrefab, transform);
            cart.transform.position = c_coaster.GetTrackByID(0).GetTrackMovePartByID(0).transform.position;
            cart.name = string.Format("CART[{0}]", cartID);

            if (cartID == 0) cart.GetComponent<RollerCart>().OnInstantiate(this, null, 0, c_gameOverSystem);
            else cart.GetComponent<RollerCart>().OnInstantiate(this, m_cartTransforms[cartID-1].GetComponent<RollerCart>(), m_followDelay);

            m_cartTransforms.Add(cart.transform);
            yield return null;
        }

        rollerBuilt = true;
    }

    public Vector3 GetRollerMovement(Vector3 a_currentPosition, ref Track a_currentTrack, ref TrackMovePoint a_currentTrackMovePoint, ref Vector3 a_up)
    {
        if (a_currentTrack == null) a_currentTrack = c_coaster.GetTrackByID(0);
        if (a_currentTrackMovePoint == null) a_currentTrackMovePoint = a_currentTrack.GetTrackMovePartByID(0);

        Vector3 prevPointPos = a_currentPosition;
        Vector3 nextPointPos = a_currentTrackMovePoint.transform.position;

        float pointDistanceMetric = Vector3.Distance(prevPointPos, nextPointPos);
        float speedToLerpWith = m_speed * Time.deltaTime;

        while (pointDistanceMetric < speedToLerpWith)
        {
            if (a_currentTrack.IsLastPoint(a_currentTrackMovePoint))
            {
                a_currentTrack = c_coaster.GetNextTrack(a_currentTrack);
                a_currentTrackMovePoint = a_currentTrack.GetTrackMovePartByID(0);
            }
            else
            {
                a_currentTrackMovePoint = a_currentTrack.GetNextTrackMovePoint(a_currentTrackMovePoint);
            }

            prevPointPos = nextPointPos;
            nextPointPos = a_currentTrackMovePoint.transform.position;

            pointDistanceMetric = Vector3.Distance(prevPointPos, nextPointPos);
            speedToLerpWith -= pointDistanceMetric;
        }



        //We've decided the starting and next position, as well as where the player should be (speedToLerpWith),(pointDistanceMetric is max)
        a_up = Vector3.Cross((nextPointPos - prevPointPos).normalized, Vector3.back);
        return Vector3.Lerp(prevPointPos, nextPointPos, speedToLerpWith / pointDistanceMetric);

    }



    private Vector3 CalculateCameraPosition()
    {
        Vector3 endResult = Vector3.zero;
        foreach(Transform cartTrans in m_cartTransforms)
        {
            endResult += cartTrans.position;
        }

        endResult /= m_cartTransforms.Count;
        endResult += m_cameraOffset;
        endResult += Vector3.forward * -10.0f;

        return endResult;
    }

}
