using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guesser : MonoBehaviour
{
    [SerializeField] private ScoreSystem c_scoreSystem;
    [SerializeField] private GameObject m_buttonBar;
    [SerializeField] private TrackGuessButton[] m_trackGuessButtons; //SPAWN DYNAMICALLY, DISGUSTING

    private Track trackToGuess;
    private SpriteRenderer trackToGuessSR;
    [SerializeField] private GameObject[] m_guessTracks;

    [SerializeField] private GameObject m_spritePreview;
    private SpriteRenderer m_spritePreviewSR;

    private bool GameOver;
    public bool GetGameOver() => GameOver;
    public void UnsetButtonBar() => m_buttonBar.SetActive(false);

    private void Start()
    {
        m_buttonBar.SetActive(false);
        m_spritePreviewSR = m_spritePreview.GetComponent<SpriteRenderer>();
    }

    public void SetToGuessTrack(Track a_track)
    {
        Debug.Log("Guessing " + a_track.name);
        m_buttonBar.SetActive(true);
        trackToGuess = a_track;
        trackToGuess.m_destroyCartOnEnter = true;

        trackToGuessSR = trackToGuess.GetComponent<SpriteRenderer>();
        trackToGuessSR.enabled = false;

        PopulateButtonsRandomly();
    }

    private void PopulateButtonsRandomly()
    {
        if (GameOver) return;
        int buttonAmount = 3; //TEMP
        List<GameObject> guessTracksLeftToPopulate = new List<GameObject>();
        foreach (GameObject guessTrack in m_guessTracks) guessTracksLeftToPopulate.Add(guessTrack);

        while(buttonAmount > 0)
        {
            int randomPick = Random.Range(0, buttonAmount);
            GameObject trackObj = guessTracksLeftToPopulate[randomPick];
            guessTracksLeftToPopulate.RemoveAt(randomPick);
            m_trackGuessButtons[--buttonAmount].SetTrack(trackObj.GetComponent<Track>());
        }

        m_spritePreview.transform.position = trackToGuess.transform.position;
    }

    public void HighLightPreview(Sprite a_sprite, bool a_flipX)
    {
        m_spritePreviewSR.sprite = a_sprite;
        m_spritePreviewSR.flipX = a_flipX;
    }

    public void UnHighLightPreview()
    {
        if(!GameOver)
            m_spritePreviewSR.sprite = null;
    }

    public void GuessTrack(Track a_guessedTrack)
    {

        if(trackToGuess.name == string.Format("{0}(Clone)", a_guessedTrack.name))
        {
            trackToGuessSR.enabled = true;
            trackToGuess.m_destroyCartOnEnter = false;
            UnHighLightPreview();

            c_scoreSystem.AddScore(1);
        }
        else
        {
            GameOver = true;

            m_spritePreviewSR.color = Color.white;
            trackToGuess.m_destroyCartOnEnter = true;
        }

        m_buttonBar.SetActive(false);
    }
}
