using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultUIController : MonoBehaviour
{

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button restartButton;

    // Start is called before the first frame update
    void Start()
    {
        restartButton.onClick.AddListener(OnRestartButtonClick);
    }


    public void Hide()
    {
        Display(false);
    }

    public void Show()
    {
        Display(true);
    }

   void Display(bool isShowing)
    {
        canvasGroup.alpha = isShowing ? 1 : 0;
        canvasGroup.interactable = isShowing;
        canvasGroup.blocksRaycasts = isShowing;
    }

    void OnRestartButtonClick()
    {
        GameManager.Instance.RestartGame();
    }
}
