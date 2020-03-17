using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : SingletonMonoBehaviour<GameController>
{

    void Start()
    {
        m_MainCamera = Camera.main;
        ChangePhase(GameState.Loading);
    }

    private void Update()
    {
        InputHandling();
    }

    #region Input 
    private Camera m_MainCamera;
    public Camera GetCamera
    {
        get { return m_MainCamera; }
    }

    private bool m_MouseClicked = false;
    private Ray m_Ray;
    private RaycastHit m_RayCastHitInfo;

    public Transform CameraRotaterTransform;
    public float RotationSensitivity = 30;


    private void InputHandling()
    {
        if(m_BallShot || CurrentGameState != GameState.GameMode)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_MouseClicked = true;
        }
        else { 
            if (Input.GetMouseButtonUp(0))
            {
                m_Ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(m_Ray, out m_RayCastHitInfo, 50))
                {
                    if (m_RayCastHitInfo.collider.tag == "Cylinder")
                    {
                        if (m_MouseClicked)
                        {
                            m_MouseClicked = false;
                            ShootBall(m_RayCastHitInfo.collider.gameObject.GetComponent<BaseCylinder>(), m_RayCastHitInfo.point);
                        }
                    }
                }
            }
            else if (Input.GetMouseButton(0) && System.Math.Abs(Input.GetAxis("Mouse X")) > Mathf.Epsilon)
            {
                m_MouseClicked = false;
                CameraRotaterTransform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * RotationSensitivity * Time.deltaTime, Space.World);
            }
        }
    }
    #endregion Input

    #region Game Play
    //Tower properties
    private float m_CylinderTowerHeightY;
    public float CylinderTowerHeightY
    {
        set { m_CylinderTowerHeightY = value; }
        get { return m_CylinderTowerHeightY; }
    }

    private Dictionary<int, List<GameObject>> m_CylinderDicList = new Dictionary<int, List<GameObject>>();
    public void AddCylinders(int _stage, List<GameObject> _cylinderList)
    {
        m_CylinderDicList.Add(_stage, _cylinderList);
    }

    public void ResetCylinderList()
    {
        m_CylinderDicList.Clear();
    }

    public int ConsideredCylinderCountForLevel()
    {
        return (int)((m_CylinderDicList[0].Count * m_CylinderDicList.Count) / 100f * 90);
    }

    private int m_PreviousBlackStageCount = 0;
    private int m_BlackStageCount = 0;
    private float m_TargetCylinderCount = 0;
    public float TargetCylinderCountToAchive
    {
        get { return (m_TargetCylinderCount / 100f) * GetTargetFactor(); }
    }

    private float GetTargetFactor()
    {
        float m_Factor = 70;
        if(SingletonGeneric<DataClass>.Instance.GameLevel == 1)
        {
            return m_Factor;
        }
        else
        {
            m_Factor = 50;
        }
        return m_Factor;
    }

    IEnumerator MakeCylindersReady()
    {
        if (m_BlackStageCount - 8 > 6)
        {
            m_PreviousBlackStageCount = m_BlackStageCount;
            m_BlackStageCount = m_BlackStageCount - 8;
            m_TargetCylinderCount = m_CylinderDicList[0].Count * (m_PreviousBlackStageCount - m_BlackStageCount);

            for (int i = 0; i < m_BlackStageCount; i++)
            {
                List<GameObject> m_TempCylinderList = m_CylinderDicList[i];
                foreach (GameObject m_Cylinder in m_TempCylinderList)
                {
                    m_Cylinder.GetComponent<BaseCylinder>().BlockThisCylinder();
                }
                yield return null;
            }
        }

        MakeBall();
        while (m_CurrentGameState != GameState.GameSet)
            yield return null;
        ChangePhase(GameState.GameMode);
    }

    public void UnBlockTheCylinders()
    {
        StartCoroutine(UnblockCylinders());
    }

    IEnumerator UnblockCylinders()
    {
        m_PreviousBlackStageCount = m_BlackStageCount;
        //if (m_BlackStageCount - 8 > 6)
        //{
        //    m_BlackStageCount = m_BlackStageCount - 8;
        //}
        //else
        //{
        //    m_BlackStageCount = 0;

        //}

        if (m_BlackStageCount - 2 > 0)
        {
            m_BlackStageCount = m_BlackStageCount - 2;
        }
        else
        {
            m_BlackStageCount = 0;

        }

        m_TargetCylinderCount = m_CylinderDicList[0].Count * (m_PreviousBlackStageCount - m_BlackStageCount);

        for (int i = m_PreviousBlackStageCount - 1; i >= m_BlackStageCount; i--)
        {
            List<GameObject> m_TempCylinderList = m_CylinderDicList[i];
            foreach (GameObject m_Cylinder in m_TempCylinderList)
            {
                m_Cylinder.GetComponent<BaseCylinder>().UnBlockThisCylinder();
            }
            yield return null;
        }

        if (onHighTowerTargetAchived != null)
            onHighTowerTargetAchived.Invoke();
    }


    //Shoot Player ball
    private bool m_BallShot = false;
    private GameObject m_Ball;
    private void MakeBall()
    {
        m_Ball = GetBallFromPlayerBallPool();
        m_Ball.SetActive(true);
        m_Ball.transform.SetParent(CameraRotaterTransform);
        m_CurrentThemeColor = SingletonGeneric<DataClass>.Instance.ChooseRandomColorForPlayer();
        m_Ball.GetComponent<Renderer>().material.SetColor("_Color", m_CurrentThemeColor);
        m_Ball.transform.localPosition = new Vector3(m_MainCamera.transform.parent.localPosition.x, m_MainCamera.transform.parent.localPosition.y - 4, m_MainCamera.transform.parent.localPosition.z + 5);

        if (onGameThemeChanged != null)
            onGameThemeChanged.Invoke();
    }

    private void ShootBall(BaseCylinder _choosenCylinder, Vector3 _targetPosition)
    {
        m_BallShot = true;

        m_Ball.GetComponent<PlayerBall>().ShootTheBall(_choosenCylinder, _targetPosition);

        StartCoroutine(ActivateBallShootingAfterDelay());
    }

    IEnumerator ActivateBallShootingAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        m_BallShot = false;
        MakeBall();
    }

    public void DestroyBall()
    {
        if(m_Ball)
        {
            m_Ball.SetActive(false);
        }
    }

    //Player Ball Object pool
    [Header("Player Ball")]
    public GameObject PlayerBallPrefab;
    private Queue<GameObject> m_PlayerBallQueue = new Queue<GameObject>();
    private GameObject GetBallFromPlayerBallPool()
    {
        if(m_PlayerBallQueue.Count > 0)
        {
            return m_PlayerBallQueue.Dequeue();
        }
        else
        {
            return Instantiate(PlayerBallPrefab);
        }
    }

    public void AddToPlayerBallPool(GameObject _playerBall)
    {
        m_PlayerBallQueue.Enqueue(_playerBall);
    }


    //Explosion Particle Object pool
    [Header("Explosion Particle")]
    public GameObject ExplosionParticlePrefab;
    private Queue<GameObject> m_ExplosionPartileQueue = new Queue<GameObject>();
    public GameObject GetExplosionParticleFromPool()
    {
        if (m_ExplosionPartileQueue.Count > 0)
        {
            return m_ExplosionPartileQueue.Dequeue();
        }
        else
        {
            return Instantiate(ExplosionParticlePrefab);
        }
    }

    public void AddExplosionParticleToThePool(GameObject _explosion)
    {
        m_ExplosionPartileQueue.Enqueue(_explosion);
    }

    //Water Splash Particle Object Pool
    [Header("Water Splash Particle")]
    public GameObject WaterSplashParticlePrefab;
    private Queue<GameObject> m_WaterSplashPartileQueue = new Queue<GameObject>();
    public GameObject GetWaterSplashParticleFromPool()
    {
        if (m_WaterSplashPartileQueue.Count > 0)
        {
            return m_WaterSplashPartileQueue.Dequeue();
        }
        else
        {
            return Instantiate(WaterSplashParticlePrefab);
        }
    }

    public void AddWaterSplashParticleToThePool(GameObject _waterSplash)
    {
        m_WaterSplashPartileQueue.Enqueue(_waterSplash);
    }
    #endregion Game Play

    #region GameState
    private GameState m_CurrentGameState = GameState.Loading;
    public GameState CurrentGameState
    {
        get { return m_CurrentGameState; }
    }

    private Color m_CurrentThemeColor;
    public Color CurrentThemeColor
    {
        get { return m_CurrentThemeColor; }
    }

    public delegate void OnGamePhaseChanged(GameState _GamePhase);
    public event OnGamePhaseChanged onGamePhaseChanged;

    public delegate void OnGameThemeChanged();
    public event OnGameThemeChanged onGameThemeChanged;

    public delegate void OnHighTowerTargetAchieved();
    public event OnHighTowerTargetAchieved onHighTowerTargetAchived;

    public void ChangePhase(GameState _GamePhase)
    {

        switch (_GamePhase)
        {
            case GameState.Loading:
                SingletonGeneric<DataClass>.Instance.SetColors();
                break;
            case GameState.MainMenu:

                break;
            case GameState.GameReady:
                break;
            case GameState.GameSet:
                foreach (List<GameObject> m_CylinderList in m_CylinderDicList.Values)
                {
                    foreach (GameObject m_Cylinder in m_CylinderList)
                    {
                        m_Cylinder.GetComponent<Rigidbody>().isKinematic = false;
                    }
                }
                m_BlackStageCount = m_CylinderDicList.Count;
                m_PreviousBlackStageCount = m_BlackStageCount;

                StartCoroutine(MakeCylindersReady());
                break;
            case GameState.GameMode:
                break;
            case GameState.Exit:
                break;
           
        }

        m_CurrentGameState = _GamePhase;

        if (onGamePhaseChanged != null)
            onGamePhaseChanged.Invoke(_GamePhase);
    }
    #endregion GameState


}
