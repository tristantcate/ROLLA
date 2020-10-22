using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DecorationSpecifier", menuName = "Parallax/DecorationSpecifier", order = 1)]
public class DecorationSpecifier : ScriptableObject
{
    public Sprite[] m_randomSprites;

    [Space(10)]
    public float m_scaleAdjust;
    public float m_thickness;
    public int m_layerOrderOffset = 0;

    [Space(10)]
    public Vector2 m_minDistanceInBetween;
    public Vector2 m_maxDistanceInBetween;

    [Space(10)]
    public Vector2 m_decoAreaMin;
    public Vector2 m_decoAreaMax;

}