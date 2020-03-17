using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProgressView : BaseView<ProgressView>
{
    public Image[] ColoredImages;
    public Slider ProgressBar;
    public Text PercentageText;
    public Text CurrentLevelText;
    public Text NextLevelText;
    public Text TimerText;

    private float m_FullAmount;
    private float m_CurrentAmount; 
    private float m_Percentage;

    private float m_TempAmount;
    private bool m_LevelFinish = false;

    private int m_TargetToAchive;
    private int m_GameTime;
    private int m_MustDestroyCylinderAmount = 5;
    private TimeSpan m_TimeSpan;
    private Coroutine m_GameTimerCoroutine;

    protected override void OnGamePhaseChanged(GameState _GamePhase)
    {
        base.OnGamePhaseChanged(_GamePhase);

        switch (_GamePhase)
        {
            case GameState.GameMode:
                m_TempAmount = 0;
                InitiateProgressValues();
                Transition(true);
                m_GameTimerCoroutine = StartCoroutine(GameTimer());
                break;
            case GameState.Exit:
                Transition(false);
                break;
        }
    }

    protected override void OnGameThemeChanged()
    {
        base.OnGameThemeChanged();
        Color m_ThemeColor = GameController.Instance.CurrentThemeColor;
        foreach (Image m_Img in ColoredImages)
        {
            m_Img.color = m_ThemeColor;
        }
        NextLevelText.color = m_ThemeColor;

    }

    private void InitiateProgressValues()
    {
        m_LevelFinish = false;

        CurrentLevelText.text = SingletonGeneric<DataClass>.Instance.GameLevel + "";
        NextLevelText.text = (SingletonGeneric<DataClass>.Instance.GameLevel + 1) + "";

        m_FullAmount = GameController.Instance.ConsideredCylinderCountForLevel();
        m_CurrentAmount = 0;
        m_TargetToAchive = 0;

        ProgressBar.minValue = 0;
        ProgressBar.maxValue = m_FullAmount;

        m_GameTime = (int)m_FullAmount / m_MustDestroyCylinderAmount; // 5 means, Must destroy 5 cylinders in 1 second. 
        SetGameTimeText();

        m_Percentage = 0;
        SetPercentageText();
    }


    IEnumerator GameTimer()
    {
        while (m_GameTime > 0)
        {
            yield return new WaitForSeconds(1);

            m_GameTime = m_GameTime - 1;
            SetGameTimeText();
        }

        if(m_GameTime <= 0)
        {
            m_LevelFinish = true;
            TimerText.text = "00:00:00";
            GameController.Instance.ChangePhase(GameState.Fail);
        }
    }

    private void SetGameTimeText()
    {
        m_TimeSpan = TimeSpan.FromSeconds(m_GameTime);
        TimerText.text = m_TimeSpan.ToString(@"hh\:mm\:ss");
    }

    public void UpdateProgressBar()
    {
        if (m_LevelFinish) return;

        m_CurrentAmount++;
        m_TargetToAchive++;


        m_TempAmount = m_CurrentAmount / m_FullAmount;
        m_Percentage = m_TempAmount * 100f;
        SetPercentageText();

        if (m_TempAmount >= 1)
        {
            m_LevelFinish = true;
            if(m_GameTimerCoroutine != null)
            {
                StopCoroutine(m_GameTimerCoroutine);
            }
            NextLevelText.gameObject.transform.parent.gameObject.GetComponent<Image>().color = NextLevelText.color;
            NextLevelText.color = Color.white;
            StartCoroutine(ChangePhaseAfterDelay());
        }
        else
        {
            if (GameController.Instance.TargetCylinderCountToAchive < m_TargetToAchive)
            {
                m_TargetToAchive = 0;
                GameController.Instance.UnBlockTheCylinders();
            }
        }
    }

    IEnumerator ChangePhaseAfterDelay()
    {
        yield return new WaitForSeconds(2);
        GameController.Instance.ChangePhase(GameState.Exit);
        SingletonGeneric<DataClass>.Instance.GameLevel++;
    }

    private void SetPercentageText()
    {
        //ProgressBar.fillAmount = m_TempAmount;
        ProgressBar.value = m_CurrentAmount;
        PercentageText.text = String.Format("{0:0.0}", m_Percentage) + "%";
    }
}
