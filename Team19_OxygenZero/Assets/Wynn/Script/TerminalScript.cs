using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalScript : MonoBehaviour
{
    [SerializeField] private GameObject messagePage, shopPage, craftingPage, statusPage, recipePage, shopDescPage;


    void Start()
    {
        messagePage.SetActive(true);
        shopPage.SetActive(false);
        craftingPage.SetActive(false);
        statusPage.SetActive(false);
        recipePage.SetActive(false);
        shopDescPage.SetActive(false);
    }

    public void EnableShop()
    {
        messagePage.SetActive(false);
        shopPage.SetActive(true);
        craftingPage.SetActive(false);
        statusPage.SetActive(false);
    }

    public void EnableCrafting()
    {
        messagePage.SetActive(false);
        shopPage.SetActive(false);
        craftingPage.SetActive(true);
        statusPage.SetActive(false);
    }

    public void EnableShuttleStatus()
    {
        messagePage.SetActive(false);
        shopPage.SetActive(false);
        craftingPage.SetActive(false);
        statusPage.SetActive(true);
    }

    public void ToggleRecipePage(bool set)
    {
        recipePage.SetActive(set);
    }

    public void ToggleShopDescPage(bool set)
    {
        shopDescPage.SetActive(set);
    }
}
