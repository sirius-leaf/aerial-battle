using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuControl2 : MonoBehaviour
{
    public RectTransform slide;

    private int slideIndex = 0;
    private float slidePosX = 0;

    void Start()
    {
        
    }

    void Update()
    {
        slidePosX = Mathf.Lerp(slidePosX, -1280f * slideIndex, 1f - Mathf.Exp(-5f * Time.deltaTime));

        slide.anchoredPosition = new(slidePosX, 0f);
    }

    public void IncreaseSlideIndex(int increase)
    {
        slideIndex = Mathf.Clamp(slideIndex + increase, 0, 3);
    }

    public void ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        AudioListener.pause = false;
        Time.timeScale = 1f;
    }
}
