using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : SingletonMonoBehaviour<LevelGenerator>
{
    //Referances
    public GameObject CylinderPrefab;

    //Cache
    private Transform m_MyPlatformTransform;

    private int m_NumberOfCylindersPerStage = 19;
    private float m_CylinderPosRadius = 3;

    private void Awake()
    {
        //PlayerPrefs.DeleteAll();
        m_MyPlatformTransform = transform;

        GameController.Instance.onGamePhaseChanged += OnGamePhaseChanged;
    }

    private void OnGamePhaseChanged(GameState _GamePhase) 
    { 
        switch(_GamePhase)
        {
            case GameState.Loading:
                GenerateLevel(); 
                break;
        }
    }

    private void GenerateLevel()
    {
        SingletonGeneric<DataClass>.Instance.ChooseRandomColorList();
        List<Color> m_ChoosenColorList = SingletonGeneric<DataClass>.Instance.ChoosenColorList;

        int m_GameLevel = SingletonGeneric<DataClass>.Instance.GameLevel;
        int m_Stages = 1;
        if (m_GameLevel == 1)
        {
            m_Stages = 10;
        }
        else if (m_GameLevel < 6)
        {
            m_Stages = 15;
        }
        else
        {
            m_Stages = 20;
        }

        
        float m_TopPositionY = 0;
        bool m_AddOffSet = false;
        GameObject m_TempCylinder;
        float val;
        for (int i = 0; i < m_Stages; i++)
        {
            List<GameObject> m_TempCylinderList = new List<GameObject>();

            for (int j = 0; j < m_NumberOfCylindersPerStage; j++)
            {
                if(m_AddOffSet)
                {
                    val = j + 0.5f;
                }
                else
                {
                    val = j;
                }
                float m_Angle = val * Mathf.PI * 2 / m_NumberOfCylindersPerStage;
                Vector3 m_Pos = new Vector3(Mathf.Cos(m_Angle), 0, Mathf.Sin(m_Angle)) * m_CylinderPosRadius;
                m_Pos.y = i;
                m_TempCylinder = Instantiate(CylinderPrefab);


                SetupColorOnCylinders(m_TempCylinder, m_Pos, m_ChoosenColorList[Random.Range(0, m_ChoosenColorList.Count)]);

                m_TempCylinderList.Add(m_TempCylinder);
            }

            m_TopPositionY += 1;
            m_AddOffSet = !m_AddOffSet;

            GameController.Instance.AddCylinders(i, m_TempCylinderList);
        }

        GameController.Instance.CylinderTowerHeightY = m_TopPositionY;
        GameController.Instance.ChangePhase(GameState.MainMenu);
    }

    private void SetupColorOnCylinders(GameObject _cylinder, Vector3 _pos, Color _color)
    {
        _cylinder.transform.position = _pos;
        _cylinder.transform.rotation = Quaternion.identity;
        _cylinder.SetActive(true);
        _cylinder.transform.SetParent(m_MyPlatformTransform);

        _cylinder.GetComponent<BaseCylinder>().SetColorForCylinder(_color);
    }
}
