using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class MainMenuSceneUIManager : MonoBehaviour {
    [SerializeField] private Transform selectMissionItemContainer;

    public void SetSelectMissionItems (List<SelectMissionItem> selectMissionItemList) {
        if (selectMissionItemList != null && selectMissionItemList.Count > 0) {
            FrameworkUtils.InsertChildrenToParent<SelectMissionItem> (selectMissionItemContainer, selectMissionItemList, true, -1, false);
        }
    }
}
