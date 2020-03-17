using UnityEngine.UI;

public class MainMenuView : BaseView<MainMenuView>
{
    public Text LevelText;

    public void OnPlayButtonClick()
    {
        if (GameController.Instance.CurrentGameState == GameState.MainMenu)
            GameController.Instance.ChangePhase(GameState.GameReady);
    }

    protected override void OnGamePhaseChanged(GameState _GamePhase)
    {
        base.OnGamePhaseChanged(_GamePhase);

        switch (_GamePhase)
        {
            case GameState.MainMenu:
                LevelText.text = "LEVEL "+SingletonGeneric<DataClass>.Instance.GameLevel;
                Transition(true);
                break;
            case GameState.GameReady:
                Transition(false);
                break;
        }
    }
}
