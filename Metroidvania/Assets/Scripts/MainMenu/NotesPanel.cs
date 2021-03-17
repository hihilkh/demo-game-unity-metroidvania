using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using HihiFramework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NotesPanel : GeneralPanelBase {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;

    [SerializeField] private Transform selectNoteBtnBaseTransform;
    [SerializeField] private HIHIButton selectNoteBtnTemplate;
    [SerializeField] private ScrollRect selectNoteBtnScrollRect;
    [SerializeField] private ScrollRect noteContentScrollRect;

    private bool isInitialized = false;
    private readonly List<HIHIButton> selectNoteBtnList = new List<HIHIButton> ();
    private readonly List<LocalizedTextDetails> fixedLocalizedTextDetailsList = new List<LocalizedTextDetails> ();
    private readonly List<Collectable.Type> collectedNoteTypeList = new List<Collectable.Type> ();

    private void Init (List<Collectable.Type> collectedNoteTypeList) {
        if (isInitialized) {
            return;
        }

        isInitialized = true;

        this.collectedNoteTypeList.Clear ();
        if (collectedNoteTypeList != null && collectedNoteTypeList.Count > 0) {
            this.collectedNoteTypeList.AddRange (collectedNoteTypeList);
        }

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
                Log.PrintError ("Cannot get text component of select note button. Please check.", LogTypes.UI);
            } else {
                var key = GameVariable.UnknownTextKey;
                if (collectedNoteTypeList.Contains (note.CollectableType)) {
                    key = note.DisplayNameKey;
                }
                fixedLocalizedTextDetailsList.Add (new LocalizedTextDetails (text, key));
            }
        }

        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_SelectNote, SelectNoteBtnClickedHandler);
    }

    protected override void OnDestroy () {
        if (isInitialized) {
            UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_SelectNote, SelectNoteBtnClickedHandler);
        }

        base.OnDestroy ();
    }

    public void Show (List<Collectable.Type> collectedNoteTypeList) {
        Init (collectedNoteTypeList);

        LangManager.SetTexts (fixedLocalizedTextDetailsList);
        contentText.text = "";
        EventSystem.current.SetSelectedGameObject (null);

        base.Show ();

        StartCoroutine (WaitAndSetSelectNoteBtnScrollRect ());
    }

    private IEnumerator WaitAndSetSelectNoteBtnScrollRect () {
        yield return null;

        selectNoteBtnScrollRect.verticalNormalizedPosition = 1;
    }

    #region Events

    private void SelectNoteBtnClickedHandler (HIHIButton sender, object info) {
        if (!(info is NoteCollectable)) {
            Log.PrintError ("OnSelectNoteClick failed. Getting invalid info type : " + info.GetType (), LogTypes.UI | LogTypes.Input | LogTypes.GameFlow);
            return;
        }

        var note = (NoteCollectable)info;

        var key = GameVariable.UnknownTextKey;
        if (collectedNoteTypeList.Contains (note.CollectableType)) {
            key = note.NoteContentKey;
        }

        LangManager.SetText (new LocalizedTextDetails (contentText, key));

        noteContentScrollRect.verticalNormalizedPosition = 1;
    }

    #endregion
}