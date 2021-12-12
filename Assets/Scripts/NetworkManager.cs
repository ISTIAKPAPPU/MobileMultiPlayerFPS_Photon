using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status")] public Text connectionStatusText;
    [Header("Login UI Panel")] public InputField playerNameInput;
    public GameObject loginUiPanel;

    [Header("Game Options UI Panel")] public GameObject gameOptionsUiPanel;
    [Header("Create Room UI Panel")] public GameObject createRoomUiPanel;
    [Header("Inside Room UI Panel")] public GameObject insideRoomUiPanel;
    [Header("Room List UI Panel")] public GameObject roomListUiPanel;
    [Header("Join Room UI Panel")] public GameObject joinRoomUiPanel;
    #region UnityMethods

    private void Start()
    {
        ActivatePanel(loginUiPanel.name);
    }

    private void Update()
    {
        connectionStatusText.text = $"Connection status: {PhotonNetwork.NetworkClientState}";
    }

    #endregion

    #region UI Callbacks

    public void OnLoginButtonClicked()
    {
        var playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Player name is invalid!");
        }
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} is Connected to photon");
        ActivatePanel(gameOptionsUiPanel.name);
    }

    #endregion

    #region Public Method

    public void ActivatePanel(string panelToBeActivated)
    {
        loginUiPanel.SetActive(panelToBeActivated.Equals(loginUiPanel.name));
        gameOptionsUiPanel.SetActive(panelToBeActivated.Equals(gameOptionsUiPanel.name));
        createRoomUiPanel.SetActive(panelToBeActivated.Equals(createRoomUiPanel.name));
        insideRoomUiPanel.SetActive(panelToBeActivated.Equals(insideRoomUiPanel.name));
        roomListUiPanel.SetActive(panelToBeActivated.Equals(roomListUiPanel.name));
        joinRoomUiPanel.SetActive(panelToBeActivated.Equals(joinRoomUiPanel.name));
    }

    #endregion
}