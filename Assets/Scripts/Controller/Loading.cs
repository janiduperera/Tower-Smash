using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    public Image LoadingBar;
    private float m_FillSpeed = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FillLoadingBar());
    }

    IEnumerator FillLoadingBar()
    {
        while(LoadingBar.fillAmount < 1)
        {
            yield return new WaitForSeconds(0.01f);
            LoadingBar.fillAmount += m_FillSpeed;

            if(LoadingBar.fillAmount > 0.4f && LoadingBar.fillAmount < 0.7f)
            {
                m_FillSpeed = 0.05f;
            }
            else
            {
                m_FillSpeed += 0.001f;
            }
        }

        SceneManager.LoadSceneAsync("Game");
    }

}
