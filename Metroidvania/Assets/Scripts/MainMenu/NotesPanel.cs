using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NotesPanel : MonoBehaviour {
    [SerializeField] private Animator animator;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;

    [SerializeField] private Transform selectNoteBtnBaseTransform;
    [SerializeField] private HIHIButton selectNoteBtnTemplate;
    [SerializeField] private ScrollRect selectNoteBtnScrollRect;

    private bool isInitialized = false;
    private List<HIHIButton> selectNoteBtnList = new List<HIHIButton> ();
    private List<LocalizedTextDetails> fixedLocalizedTextDetailsList = new List<LocalizedTextDetails> ();
    private List<Collectable.Type> collectedNoteTypeList = new List<Collectable.Type> ();

    private void Init (List<Collectable.Type> collectedNoteTypeList) {
        if (isInitialized) {
            return;
        }

        isInitialized = true;

        this.collectedNoteTypeList = collectedNoteTypeList;

        fixedLocalizedTextDetailsList.Clear ();
        fixedLocalizedTextDetailsList.Add (new LocalizedTextDetails (titleText, "NotePanelTitle"));

        selectNoteBtnList.Clear ();
        var notes = CollectableManager.GetAllNotes ();
        foreach (var note in notes) {
            var btn = Instantiate (selectNoteBtnTemplate, selectNoteBtnBaseTransform);
            selectNoteBtnList.Add (btn);

            btn.SetOnClickInfo (BtnOnClickType.MainMenu_SelectNote, note);
            var text = btn.GetComponentInChildren<TextMeshProUGUI> ();
            if (text == null) {
                Log.PrintError ("Cannot get text component of select note button. Please check.", LogType.UI);
            } else {
                var key = GameVariable.UnknownTextKey;
                if (collectedNoteTypeList.Contains (note.type)) {
                    key = note.displayNameKey;
                }
                fixedLocalizedTextDetailsList.Add (new LocalizedTextDetails (text, key));
            }
        }

        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_SelectNote, OnSelectNoteClick);
        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_CloseNotesPanel, OnCloseButtonClick);
    }

    private void OnDestroy () {
        if (isInitialized) {
            UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_SelectNote, OnSelectNoteClick);
            UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_CloseNotesPanel, OnCloseButtonClick);
        }
    }

    public void Show (List<Collectable.Type> collectedNoteTypeList) {
        Init (collectedNoteTypeList);

        LangManager.SetTexts (fixedLocalizedTextDetailsList);
        contentText.text = "";
        EventSystem.current.SetSelectedGameObject (null);

        gameObject.SetActive (true);

        StartCoroutine (WaitAndSetScrollRect ());
    }

    private IEnumerator WaitAndSetScrollRect () {
        yield return null;

        selectNoteBtnScrollRect.verticalNormalizedPosition = 1;
    }

    private void Hide() {
        Action onFinished = () => {
            gameObject.SetActive (false);
        };

        FrameworkUtils.Instance.StartSingleAnim (animator, GameVariable.HidePanelAnimStateName, onFinished);
    }

    private void OnCloseButtonClick (HIHIButton btn) {
        Hide ();
    }

    private void OnSelectNoteClick (HIHIButton btn, object info) {
        if (!(info is NoteCollectable)) {
            Log.PrintError ("OnSelectNoteClick failed. Getting invalid info type : " + info.GetType (), LogType.UI | LogType.Input | LogType.GameFlow);
            return;
        }

        var note = (NoteCollectable)info;

        var key = GameVariable.UnknownTextKey;
        if (collectedNoteTypeList.Contains (note.type)) {
            key = note.noteContentKey;
        }
        LangManager.SetText (new LocalizedTextDetails (contentText, key));
    }
}