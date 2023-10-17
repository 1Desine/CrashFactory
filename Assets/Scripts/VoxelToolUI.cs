using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoxelToolUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI toolUsingText;




    private void Start() {
        VoxelTool.Instance.OnSetToolActive += VoxelTool_OnSetToolActive;
        VoxelTool.Instance.OnSetBrush += VoxelTool_OnSetBrush;

        Hide();
    }


    private void VoxelTool_OnSetBrush(Voxel.Type currentBrush) {
        toolUsingText.text = currentBrush.ToString();
    }
    private void VoxelTool_OnSetToolActive(bool toolIsActive) {
        if (toolIsActive) Show();
        else Hide();
    }


    public void Hide() {
        gameObject.SetActive(false);
    }
    public void Show() {
        gameObject.SetActive(true);
    }



}
