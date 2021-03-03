using System;
using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class EnemyCharDetection : MonoBehaviour {
    public event Action CharDetected;
    public event Action CharLost;

    public void SetActive (bool isActive) {
        gameObject.SetActive (isActive);
    }
    
    public void OnTriggerEnter2D (Collider2D collision) {
        if (collision.tag == GameVariable.PlayerTag) {
            CharDetected?.Invoke ();
        }
    }

    public void OnTriggerExit2D (Collider2D collision) {
        if (collision.tag == GameVariable.PlayerTag) {
            CharLost?.Invoke ();
        }
    }
}