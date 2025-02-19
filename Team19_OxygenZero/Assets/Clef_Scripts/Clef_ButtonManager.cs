using UnityEngine;

public class Clef_ButtonManager : MonoBehaviour
{
    public GameObject settingButton;
    public GameObject hideTitle;
    public GameObject hideButtons;

    private void Start()
    {
        settingButton.SetActive(false);
        hideButtons.SetActive(true);
        hideTitle.SetActive(true);
    }
    public void OnSettingPanelClick()
    {
        settingButton.SetActive(true);
        hideButtons.SetActive(false);
        hideTitle.SetActive(false);
    }

    public void OnCloseSettingPanelClick()
    {
        settingButton.SetActive(false);
        hideButtons.SetActive(true);
        hideTitle.SetActive(true);
    }
}
