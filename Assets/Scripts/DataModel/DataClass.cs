using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataClass 
{
    public int GameLevel
    {
        get { return PlayerPrefs.GetInt("GameLevel", 1); }
        set { PlayerPrefs.SetInt("GameLevel", value); }
    }

    public bool IsReplayingTheGame = false;

    #region Colors
    private List<Color> m_ColorList = new List<Color>();
    public void SetColors()
    {
        m_ColorList.Clear();
        m_ColorList.Add(new Color(128f / 255f, 0, 0));
        m_ColorList.Add(new Color(170f / 255f, 110f / 255f, 40f / 255f));
        m_ColorList.Add(new Color(128f / 255f, 128f / 255f, 0));
        m_ColorList.Add(new Color(0, 128f / 255f, 128f / 255f));
        m_ColorList.Add(new Color(0, 0, 128f / 255f));
        m_ColorList.Add(new Color(230f / 255f, 25f / 255f, 75f / 255f));
        m_ColorList.Add(new Color(210f / 255f, 245f / 255f, 60f / 255f));
        m_ColorList.Add(new Color(60f / 255f, 180f / 255f, 75f / 255f));
        m_ColorList.Add(new Color(0, 130f / 255f, 200f / 255f));
        m_ColorList.Add(new Color(145f / 255f, 30f / 255f, 180f / 255f));
        //m_ColorList.Add(new Color(240f / 255f, 50f / 255f, 230f / 255f));
        m_ColorList.Add(new Color(230f / 255f, 190f / 255f, 255f / 255f));
        m_ColorList.Add(new Color(70f / 255f, 240f / 255f, 240f / 255f));
        m_ColorList.Add(new Color(170f / 255f, 255f / 255f, 195f / 255f));
    }

    private List<Color> m_ChoosenColorList = new List<Color>();
    public List<Color> ChoosenColorList
    {
        get { return m_ChoosenColorList; }
    }
    public void ChooseRandomColorList()
    {
        m_ChoosenColorList.Clear();
        int m_Level = GameLevel;
        int m_ColorCount = 1;
        if (m_Level == 1) 
        {
            m_ColorCount = 2;
        }
        else if (m_Level < 6)
        {
            m_ColorCount = 4;
        }
        else
        {
            m_ColorCount = 7;
        }

        Color m_RandomColor;
        for (int i = 0; i < m_ColorCount; i++)
        {
            m_RandomColor = m_ColorList[Random.Range(0, m_ColorList.Count)];
            while (m_ChoosenColorList.Contains(m_RandomColor))
            {
                m_RandomColor = m_ColorList[Random.Range(0, m_ColorList.Count)];
            }
            m_ChoosenColorList.Add(m_RandomColor);
        }
    }

    private Color m_PreviouslyChoosenColor = Color.black;
    public Color ChooseRandomColorForPlayer()
    {
        Color m_TempColor = ChoosenColorList[Random.Range(0, ChoosenColorList.Count)];
        while (m_PreviouslyChoosenColor == m_TempColor)
        {
            m_TempColor = ChoosenColorList[Random.Range(0, m_ChoosenColorList.Count)];
        }
        m_PreviouslyChoosenColor = m_TempColor;
        return m_TempColor;
    }
    #endregion Colors
}
