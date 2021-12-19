using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status")] public Text connectionStatusText;
    [Header("Login UI Panel")] public InputField playerNameInput;
    public GameObject loginUiPanel;

    [Header("Game Options UI Panel")] public GameObject gameOptionsUiPanel;
    [Header("Create Room UI Panel")] public GameObject createRoomUiPanel;
    public InputField roomNameInputField;
    public InputField maxPlayerInputField;
    [Header("Inside Room UI Panel")] public GameObject insideRoomUiPanel;
    public Text roomInfoText;
    public GameObject playerListPrefab;
    public GameObject playerListContent;
    public GameObject startGameButton;
    [Header("Room List UI Panel")] public GameObject roomListUiPanel;
    public GameObject roomListEntryPrefab;
    public GameObject roomListParentGameObject;
    [Header("Join Room UI Panel")] public GameObject joinRoomUiPanel;

    private Dictionary<string, RoomInfo> _cachedRoomList;
    private Dictionary<string, GameObject> _roomListGameObjects;
    private Dictionary<int, GameObject> _playerListGameObjects;

    #region UnityMethods

    private void Start()
    {
        ActivatePanel(loginUiPanel.name);
        _cachedRoomList = new Dictionary<string, RoomInfo>();
        _roomListGameObjects = new Dictionary<string, GameObject>();
        PhotonNetwork.AutomaticallySyncScene = true;
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

    public void OnRoomCreateButtonClicked()
    {
        var roomName = roomNameInputField.text;
        var mxPlayer = string.IsNullOrEmpty(maxPlayerInputField.text)
            ? Random.Range(2, 20)
            : int.Parse(maxPlayerInputField.text);
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(100, 10000);
        }

        var roomOptions = new RoomOptions
        {
            MaxPlayers = (byte) mxPlayer
        };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }


    public void OnCancelButtonClicked()
    {
        ActivatePanel(gameOptionsUiPanel.name);
    }

    public void OnShowListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivatePanel(roomListUiPanel.name);
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        ActivatePanel(gameOptionsUiPanel.name);
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        ActivatePanel(joinRoomUiPanel.name);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnstartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
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

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(insideRoomUiPanel.name);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }

        roomInfoText.text =
            $"Room Name: {PhotonNetwork.CurrentRoom.Name} Players/MaxPlayers {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";

        if (_playerListGameObjects == null)
        {
            _playerListGameObjects = new Dictionary<int, GameObject>();
        }

        foreach (var player in PhotonNetwork.PlayerList)
        {
            var playerListGameObject = Instantiate(playerListPrefab, playerListContent.transform);
            playerListGameObject.transform.localScale = Vector3.one;

            playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
            }
            else
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
            }

            _playerListGameObjects.Add(player.ActorNumber, playerListGameObject);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomInfoText.text =
            $"Room Name: {PhotonNetwork.CurrentRoom.Name} Players/MaxPlayers {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";

        var playerListGameObject = Instantiate(playerListPrefab, playerListContent.transform);
        playerListGameObject.transform.localScale = Vector3.one;

        playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;

        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
        }
        else
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
        }

        _playerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomInfoText.text =
            $"Room Name: {PhotonNetwork.CurrentRoom.Name} Players/MaxPlayers {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";

        Destroy(_playerListGameObjects[otherPlayer.ActorNumber].gameObject);
        _playerListGameObjects.Remove(otherPlayer.ActorNumber);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
    }

    public override void OnLeftRoom()
    {
        ActivatePanel(gameOptionsUiPanel.name);
        foreach (var playerListGameObject in _playerListGameObjects.Values)
        {
            Destroy(playerListGameObject);
        }

        _playerListGameObjects.Clear();
        _playerListGameObjects = null;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();
        foreach (var room in roomList)
        {
            Debug.Log(room.Name);
            if (!room.IsOpen || !room.IsVisible || room.RemovedFromList)
            {
                if (_cachedRoomList.Remove(room.Name)) ;
            }

            if (_cachedRoomList.ContainsKey(room.Name))
            {
                _cachedRoomList[room.Name] = room;
            }
            else
            {
                _cachedRoomList.Add(room.Name, room);
            }
        }

        foreach (var roomInfo in _cachedRoomList.Values)
        {
            var roomListEmptyGameObject = Instantiate(roomListEntryPrefab, roomListParentGameObject.transform);
            roomListEmptyGameObject.transform.localScale = Vector3.one;

            roomListEmptyGameObject.transform.Find("RoomNameText").GetComponent<Text>().text = roomInfo.Name;
            roomListEmptyGameObject.transform.Find("RoomPlayersText").GetComponent<Text>().text =
                roomInfo.PlayerCount + " / " + roomInfo.MaxPlayers;
            roomListEmptyGameObject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick
                .AddListener(() => OnJointButtonClicked(roomInfo.Name));
            _roomListGameObjects.Add(roomInfo.Name, roomListEmptyGameObject);
        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomListView();
        _cachedRoomList.Clear();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        var roomName = "Room " + Random.Range(1000, 10000);
        var roomOptions = new RoomOptions
        {
            MaxPlayers = 20
        };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    #endregion

    #region Private Methods

    private void OnJointButtonClicked(string roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    private void ClearRoomListView()
    {
        foreach (var roomListGameObject in _roomListGameObjects.Values)
        {
            Destroy(roomListGameObject);
        }

        _roomListGameObjects.Clear();
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