using System.Collections;
using UnityEngine;

public class BaseView<T> : SingletonMonoBehaviour<T> where T : MonoBehaviour
{
    public float FadeInDuration = 0.3f;
    public float FadeOutDuration = 0.3f;


    // Buffers
    private float m_StartTime;
    private float m_Duration;
    private bool m_InTransition;
    private bool m_InOrOut;
    private CanvasGroup m_Group;

    protected bool m_Visible;

    protected virtual void Awake()
    {
        // Init
        m_Group = GetComponent<CanvasGroup>();
        m_Visible = false;
        m_Group.alpha = 0.0f;
        m_Group.interactable = false;
        m_Group.blocksRaycasts = false;

        GameController.Instance.onGamePhaseChanged += OnGamePhaseChanged;
        GameController.Instance.onGameThemeChanged += OnGameThemeChanged;
    }

    protected virtual void OnGamePhaseChanged(GameState _GamePhase) { }

    protected virtual void OnGameThemeChanged() { }

    public void Transition(bool _InOrOut)
    {
        m_Visible = _InOrOut;

        m_StartTime = Time.time;
        m_InTransition = true;
        m_InOrOut = _InOrOut;
        m_Duration = _InOrOut ? FadeInDuration : FadeOutDuration;
        m_Group.interactable = false;
        m_Group.blocksRaycasts = false;

        StartCoroutine(FadeInOrOut());
    }

    private IEnumerator FadeInOrOut()
    {
        while(m_InTransition)
        {
            yield return null;
            float time = Time.time - m_StartTime;
            float percent = time / m_Duration;

            if (percent < 1.0f)
            {
                m_Group.alpha = m_InOrOut ? percent : (1.0f - percent);
            }
            else
            {
                m_InTransition = false;
                m_Group.alpha = m_InOrOut ? 1.0f : 0.0f;
                m_Group.interactable = m_InOrOut;
                m_Group.blocksRaycasts = m_InOrOut;
            }
        }
    }
}
