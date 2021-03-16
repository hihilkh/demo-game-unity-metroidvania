using System;
using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyCharDetection : MonoBehaviour {
    public event Action CharDetected;
    public event Action CharLost;

    private bool isDebugging = false;

    private void Awake () {
        if (SceneManager.GetActiveScene ().name == GameVariable.MapEditorSceneName ||
            SceneManager.GetActiveScene ().name == GameVariable.SandboxSceneName) {
            isDebugging = true;
        }
    }

    public void SetActive (bool isActive) {
        gameObject.SetActive (isActive);
    }
    
    public void OnTriggerEnter2D (Collider2D collision) {
        if (!GameSceneManager.IsGameStarted && !isDebugging) {
            return;
        }

        if (collision.tag == GameVariable.PlayerTag) {
            CharDetected?.Invoke ();
        }
    }

    public void OnTriggerExit2D (Collider2D collision) {
        if (!GameSceneManager.IsGameStarted && !isDebugging) {
            return;
        }

        if (collision.tag == GameVariable.PlayerTag) {
            CharLost?.Invoke ();
        }
    }
}