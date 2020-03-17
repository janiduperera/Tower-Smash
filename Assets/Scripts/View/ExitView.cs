using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ExitView : BaseView<ExitView>
{
    public Animator LevelEndAnimator;

    public void OnPlayAgainButtonClick()
    {
        SingletonGeneric<DataClass>.Instance.IsReplayingTheGame = true;
        SceneManager.LoadSceneAsync("Game");
    }

    protected override void OnGamePhaseChanged(GameState _GamePhase)
    {
        base.OnGamePhaseChanged(_GamePhase);

        switch (_GamePhase)
        {
            case GameState.MainMenu:
            case GameState.Fail:
                Transition(false);
                break;
            case GameState.Exit:
                GameController.Instance.DestroyBall();
                Transition(true);
                StartCoroutine(OnLevelEndAnimationFinish());
                break;
        }
    }


    IEnumerator OnLevelEndAnimationFinish()
    {
        yield return new WaitForSeconds(1);
        LevelEndAnimator.SetBool("doPlay", true);
    }
}
