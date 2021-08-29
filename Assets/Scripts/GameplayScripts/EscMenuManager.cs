using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscMenuManager : MonoBehaviour
{
    [Header("Escape Menu UI")]
    [SerializeField] private GameObject EscMenuPanel;
    [SerializeField] private GameObject EscMenuPanelButtonHolder;
    [SerializeField] private GameObject SettingsMenuPanel;

    public bool isEscMenuOpen;

    // Start is called before the first frame update
    void Start()
    {
        EscMenuPanel.SetActive(false);
        EscMenuPanelButtonHolder.SetActive(false);
        SettingsMenuPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isEscMenuOpen)
            {
                EscMenuPanel.SetActive(true);
                EscMenuPanelButtonHolder.SetActive(true);
                isEscMenuOpen = true;
            }
            else
            {
                EscMenuPanel.SetActive(false);
                EscMenuPanelButtonHolder.SetActive(false);
                SettingsMenuPanel.SetActive(false);
                isEscMenuOpen = false;
            }
        }
    }
    public void BackToEscMenu()
    {
        EscMenuPanel.SetActive(true);
        EscMenuPanelButtonHolder.SetActive(true);
        SettingsMenuPanel.SetActive(false);
    }
    public void OpenSettingsPanel()
    {
        EscMenuPanel.SetActive(true);
        EscMenuPanelButtonHolder.SetActive(false);
        SettingsMenuPanel.SetActive(true);
    }
    public void BacktoTheGame()
    {
        EscMenuPanel.SetActive(false);
        EscMenuPanelButtonHolder.SetActive(false);
        SettingsMenuPanel.SetActive(false);
        isEscMenuOpen = false;
    }
}
