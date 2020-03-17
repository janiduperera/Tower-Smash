using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingView : BaseView<LoadingView>
{
    public GameObject LoadingObj;
    protected override void OnGamePhaseChanged(GameState _GamePhase)
    {
        base.OnGamePhaseChanged(_GamePhase);

        switch (_GamePhase)
        {
            case GameState.Loading:
                if(SingletonGeneric<DataClass>.Instance.IsReplayingTheGame)
                {
                    LoadingObj.transform.localScale = Vector3.zero;
                }
                Transition(true);
                break;
        }
    }
}

