using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FlappyBatGame.Managers
{
    public class UIManager : MonoBehaviour
    {
        public LogicManager logicManager;
        [Header("UI panels")]
        public GameObject startScreen;
        public GameObject gameplayScreen;
        public GameObject gameOverScreen;
        
        [Header("gameplayScreen's UI elements")]
        public Text scoreText;

        public void ShowStartScreen()
        {
            if (startScreen != null) startScreen.SetActive(true);
            if (gameplayScreen != null) gameplayScreen.SetActive(false);
            if (gameOverScreen != null) gameOverScreen.SetActive(false);
        }
        
        public void ShowGameplayScreen()
        {
            if (startScreen != null) startScreen.SetActive(false);
            if (gameplayScreen != null) gameplayScreen.SetActive(true);
            if (gameOverScreen != null) gameOverScreen.SetActive(false);
        }
        
        public void ShowGameOverScreen()
        {
            if (startScreen != null) startScreen.SetActive(false);
            if (gameplayScreen != null) gameplayScreen.SetActive(false);
            if (gameOverScreen != null) gameOverScreen.SetActive(true);
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = score.ToString();
        }

        public void OnStartButtonPressed()
        {
            if (logicManager != null)
            {
                logicManager.ResetGame();
                logicManager.StartGame();
            }
                
            else
                Debug.LogWarning("LogicManager is not assigned in UIManager!");
        }

        public void OnCloseButtonPressed()
        {
            Application.Quit();
        }
    }
}