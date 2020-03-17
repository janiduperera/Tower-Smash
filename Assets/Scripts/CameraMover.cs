using System.Collections;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    //Buffer
    private float m_GameReadyCameraEndPosY;
    private float m_MoveSpeed = 5;

    //Cache
    private Transform m_Transform;
    private Transform m_CameraRotater;
    private Camera m_Camera;

    private Coroutine m_ZoomOutCameraCoroutine;


    // Start is called before the first frame update
    void Start()
    {
        m_Transform = transform;
        m_CameraRotater = m_Transform.parent;
        GameController.Instance.onGamePhaseChanged += OnGamePhaseChanged;
        GameController.Instance.onHighTowerTargetAchived += OnHighTowerTargetAchived;
    }

    private void OnGamePhaseChanged(GameState _GamePhase)
    {
        switch (_GamePhase)
        {
            case GameState.Loading:
            case GameState.MainMenu:
                m_Camera = GameController.Instance.GetCamera;
                if(m_ZoomOutCameraCoroutine != null)
                    StopCoroutine(m_ZoomOutCameraCoroutine);
                ResetCamera();
                break;
            case GameState.GameReady:
                StartCoroutine(MoveCameraUP());
                break;
            case GameState.Exit:
            case GameState.Fail:
                m_ZoomOutCameraCoroutine = StartCoroutine(ZoomOutCamera());
                break;
        }
    }

    private void OnHighTowerTargetAchived()
    {
        StartCoroutine(MoveCameraDOWN());
    }

    IEnumerator MoveCameraUP()
    {
        m_GameReadyCameraEndPosY = GameController.Instance.CylinderTowerHeightY - 5;
        while (m_Transform.position.y < m_GameReadyCameraEndPosY)
        {
            m_Transform.Translate(Vector3.up * Time.deltaTime * m_MoveSpeed);
            m_CameraRotater.Rotate(-Vector3.up * Time.deltaTime * m_MoveSpeed * 20);
            yield return null;
        }
        GameController.Instance.ChangePhase(GameState.GameSet);
    }

    IEnumerator MoveCameraDOWN()
    {
        float m_LastPosY = m_Transform.position.y - 2;
        if (m_LastPosY < 5)
            m_LastPosY = 5;
        while (m_Transform.position.y > m_LastPosY)
        {
            m_Transform.Translate(Vector3.down * Time.deltaTime * m_MoveSpeed);
            yield return null;
        }
    }

    IEnumerator ZoomOutCamera()
    {
        while (m_Camera.fieldOfView < 130)
        {
            m_Camera.fieldOfView += 0.5f;
            yield return null;
        }

        while (true)
        {
            m_CameraRotater.Rotate(-Vector3.up * Time.deltaTime * m_MoveSpeed * 2);
            yield return null;
        }
    }

    private void ResetCamera()
    {
        m_CameraRotater.rotation = Quaternion.identity;
        m_Transform.localPosition = new Vector3(0, 5, -12);
        m_Camera.fieldOfView = 90;
    }
}
