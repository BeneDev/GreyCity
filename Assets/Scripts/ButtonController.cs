using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour {

    [SerializeField] GameObject fadeInObject;

    private void OnEnable()
    {
        StartCoroutine(FadeIn(2f));
        
    }

    IEnumerator FadeIn(float duration)
    {
        Color fadeColor = fadeInObject.GetComponent<SpriteRenderer>().color;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            fadeColor.a = 255 * t / duration;
            fadeInObject.GetComponent<SpriteRenderer>().color = fadeColor;
            yield return new WaitForEndOfFrame();
        }
        fadeColor.a = 255;
        fadeInObject.GetComponent<SpriteRenderer>().color = fadeColor;
    }

    public void OnPlayAgain()
    {
        SceneManager.LoadScene(1);
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
