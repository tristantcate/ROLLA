using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TrackGuessButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Track m_track;
    private Sprite m_trackSprite;
    private Image m_trackImageComponent;

    [SerializeField] private Guesser c_guesser;

    private void Awake()
    {
        m_trackImageComponent = transform.GetChild(0).GetComponentInChildren<Image>();
    }

    public void SetTrack(Track a_track)
    {
        m_track = a_track;
        m_trackSprite = a_track.GetComponent<SpriteRenderer>().sprite;
        m_trackImageComponent.sprite = m_trackSprite;

        if (a_track.GetComponent<SpriteRenderer>().flipX)
            m_trackImageComponent.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else m_trackImageComponent.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    public Track GetTrack()
    {
        return m_track;
    }

    public Sprite GetSprite() => m_trackSprite;

    public void OnPointerClick(PointerEventData eventData)
    {
        c_guesser.GuessTrack(m_track);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        c_guesser.HighLightPreview(m_trackSprite, m_track.GetComponent<SpriteRenderer>().flipX);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        c_guesser.UnHighLightPreview();
    }

    
}
