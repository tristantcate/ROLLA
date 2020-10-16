using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollerCart : MonoBehaviour
{
    private Roller c_roller;

    private Track currentTrack;
    private TrackMovePoint currentTrackPoint;
    private RollerCart cartToFollow = null;
    private int followDelay = 60;

    public List<Vector3> positionToFollow = new List<Vector3>();
    public List<float> rotationToFollow = new List<float>();

    [SerializeField] private GameObject explosionPrefab;
    private bool gameOver = false;

    [SerializeField] private float GameOverMenuDelay = 1.0f;
    private GameOverSystem c_gameOverSystem;

    private Rigidbody2D m_rigidbody;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    public void OnInstantiate(Roller a_roller, RollerCart a_cartToFollow, int a_followDelay, GameOverSystem a_gameOverSystem = null)
    {
        c_roller = a_roller;
        cartToFollow = a_cartToFollow;
        followDelay = a_followDelay;
        c_gameOverSystem = a_gameOverSystem;
    }

    private void Update()
    {
        if (gameOver) return;

        if (cartToFollow == null)
        {
            Vector3 newUp = Vector3.up;

            Vector3 movement = c_roller.GetRollerMovement(transform.position, ref currentTrack, ref currentTrackPoint, ref newUp);
            transform.position = movement;
            transform.up = newUp;

            if(currentTrack.m_destroyCartOnEnter)
            {
                StartCoroutine(DestroyCart());
            }

        }
        else
        {
            if(cartToFollow.positionToFollow.Count > followDelay)
            {
                transform.position = cartToFollow.positionToFollow[0];
                cartToFollow.positionToFollow.RemoveAt(0);

                transform.rotation = Quaternion.Euler(0, 0, cartToFollow.rotationToFollow[0]);
                cartToFollow.rotationToFollow.RemoveAt(0);
            }

            if (cartToFollow.gameOver) StartCoroutine(FollowerGameOver());
        }

        positionToFollow.Add(transform.position);
        rotationToFollow.Add(transform.eulerAngles.z);
    }

    private IEnumerator FollowerGameOver()
    {
        for (int i = 0; i < followDelay; i++)
        {
            transform.position = cartToFollow.positionToFollow[i];
            transform.rotation = Quaternion.Euler(0, 0, cartToFollow.rotationToFollow[i]);
            yield return null;
            yield return null;
        }

        StartCoroutine(DestroyCart());
    }

    private IEnumerator DestroyCart()
    {
        gameOver = true;

        Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), 2.0f);
        yield return new WaitForSeconds(0.33f);
        transform.GetComponentInChildren<SpriteRenderer>().enabled = false;

        if(cartToFollow == null)
        {
            yield return new WaitForSeconds(GameOverMenuDelay);
            c_gameOverSystem.SpawnGameOverScreen();
        }
    }
}
