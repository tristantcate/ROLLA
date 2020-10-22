using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DecorationCollection", menuName = "Parallax/DecorationCollection", order = 1)]
public class DecorationCollection : ScriptableObject
{
    public DecorationAssembly[] m_decorations;
    
}

[System.Serializable]

public class DecorationAssembly
{
    public DecorationSpecifier[] m_decorationSpecifiers;

    [Space(10)]
    public int m_spawnChance = 0;
}