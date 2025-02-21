using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalScript : MonoBehaviour
{
    [SerializeField] private GameObject messagePage, shopPage, sellPage, craftingPage, statusPage, recipePage;


    void Start()
    {
        messagePage.SetActive(true);
        shopPage.SetActive(false);
        sellPage.SetActive(false);
        craftingPage.SetActive(false);
        statusPage.SetActive(false);
        recipePage.SetActive(false);
    }

    public void EnableShop()
    {
        messagePage.SetActive(false);
        shopPage.SetActive(true);
        sellPage.SetActive(false);
        craftingPage.SetActive(false);
        statusPage.SetActive(false);
    }

    public void EnableSell()
    {
        messagePage.SetActive(false);
        shopPage.SetActive(false);
        sellPage.SetActive(true);
        craftingPage.SetActive(false);
        statusPage.SetActive(false);
    }

    public void EnableCrafting()
    {
        messagePage.SetActive(false);
        shopPage.SetActive(false);
        sellPage.SetActive(false);
        craftingPage.SetActive(true);
        statusPage.SetActive(false);
    }

    public void EnableShuttleStatus()
    {
        messagePage.SetActive(false);
        shopPage.SetActive(false);
        sellPage.SetActive(false);
        craftingPage.SetActive(false);
        statusPage.SetActive(true);
    }

    public void ToggleRecipePage(bool set)
    {
        recipePage.SetActive(set);
    }
}
