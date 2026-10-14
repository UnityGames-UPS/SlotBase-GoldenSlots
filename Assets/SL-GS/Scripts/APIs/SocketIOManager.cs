using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;

public class SocketIOManager : MonoBehaviour
{
  [SerializeField]
  private SlotBehaviour slotManager;


  [SerializeField]
  private UIManager uiManager;
  [SerializeField] BonusController bonusController;

  internal GameData InitialData = null;
  internal UiData UIData = null;
  internal Root ResultData = null;
  internal Player PlayerData = null;
  internal Root bonusData = new();
  //[SerializeField]
  //internal List<string> bonusdata = null;
  //WebSocket currentSocket = null;
  internal bool isResultdone = false;

  // protected string nameSpace="game"; //BackendChanges
  protected string nameSpace = "playground"; //BackendChanges

  private Socket gameSocket; //BackendChanges

  [SerializeField] internal JSFunctCalls JSManager;

  private SocketManager manager;



  protected string SocketURI = null;
  // COMPLETED: slot to be changed
  // protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
  [SerializeField] protected string TestSocketURI = "";
  //protected string TestSocketURI = "https://7p68wzhv-5000.inc1.devtunnels.ms/";

  [SerializeField]
  private string testToken;

  // COMPLETED: slot to be changed
  //protected string gameID = "";
  protected string gameID = "SL-GS";

  internal bool isLoaded = false;

  internal bool SetInit = false;

  private const int maxReconnectionAttempts = 6;
  private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

  private bool isConnected = false; //Back2 Start
  private bool hasEverConnected = false;
  private const int MaxReconnectAttempts = 5;
  private const float ReconnectDelaySeconds = 2f;

  private float lastPongTime = 0f;
  private float pingInterval = 2f;
  private float pongTimeout = 3f;
  private bool waitingForPong = false;
  private int missedPongs = 0;
  private const int MaxMissedPongs = 5;
  private Coroutine PingRoutine; //Back2 end

  private bool hasFocus = true;
  private float focusLostTime = 0f;
  private Coroutine focusCheckRoutine;
  private float maxBackgroundTime = 60f;
  private bool isExiting = false;
  private bool isBeingDestroyed = false;

  private void Awake()
  {
    //Debug.unityLogger.logEnabled = false;
    isLoaded = false;
    SetInit = false;

  }

  private void OnDestroy()
  {
    isBeingDestroyed = true;
  }

  private void Start()
  {
    //OpenWebsocket();
    OpenSocket();
  }

  void ReceiveAuthToken(string jsonData)
  {
    Debug.Log("Received data: " + jsonData);

    // Parse the JSON data
    var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
    SocketURI = data.socketURL;
    myAuth = data.cookie;

    nameSpace = data.nameSpace; //BackendChanges
                                // Proceed with connecting to the server using myAuth and socketURL
  }

  string myAuth = null;

  private void OpenSocket()
  {
    SocketOptions options = new SocketOptions(); //Back2 Start
    options.AutoConnect = false;
    options.Reconnection = false;
    options.Timeout = TimeSpan.FromSeconds(3); //Back2 end
    options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket;

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("authToken");
        StartCoroutine(WaitForAuthToken(options));
#else
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = testToken,

      };
    };
    options.Auth = authFunction;
    // Proceed with connecting to the server
    SetupSocketManager(options);
#endif

  }


  private IEnumerator WaitForAuthToken(SocketOptions options)
  {
    // Wait until myAuth is not null
    while (myAuth == null)
    {
      Debug.Log("My Auth is null");
      yield return null;
    }
    while (SocketURI == null)
    {
      Debug.Log("My Socket is null");
      yield return null;
    }
    Debug.Log("My Auth is not null");
    // Once myAuth is set, configure the authFunction
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = myAuth,

      };
    };
    options.Auth = authFunction;

    Debug.Log("Auth function configured with token: " + myAuth);

    // Proceed with connecting to the server
    SetupSocketManager(options);
  }

  private void SetupSocketManager(SocketOptions options)
  {
    // Create and setup SocketManager
#if UNITY_EDITOR
    this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif
    if (string.IsNullOrEmpty(nameSpace))
    {  //BackendChanges Start
      gameSocket = this.manager.Socket;
    }
    else
    {
      print("nameSpace: " + nameSpace);
      gameSocket = this.manager.GetSocket("/" + nameSpace);
    }
    // Set subscriptions
    gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
    gameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected); //Back2 Start
    gameSocket.On<Error>(SocketIOEventTypes.Error, OnError);

    gameSocket.On<string>("game:init", OnListenEvent);
    gameSocket.On<string>("result", OnResult);
    gameSocket.On<string>("bonus:result", OnBonusResult);
    gameSocket.On<bool>("socketState", OnSocketState);
    gameSocket.On<string>("internalError", OnSocketError);
    gameSocket.On<string>("alert", OnSocketAlert);
    gameSocket.On<string>("pong", OnPongReceived); //Back2 Start
    gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice); //BackendChanges Finish
    gameSocket.On<string>("balance:sync", OnBalanceSync);

    manager.Open();
  }

  // Connected event handler implementation
  void OnConnected(ConnectResponse resp)
  {
    Debug.Log("✅ Connected to server.");

    if (hasEverConnected)
    {
      uiManager.CheckAndClosePopups();
    }

    isConnected = true;
    hasEverConnected = true;
    waitingForPong = false;
    missedPongs = 0;
    lastPongTime = Time.time;
    SendPing();
  } //Back2 end

  void OnBonusResult(string data)
  {
    // Handle the game result here
    Debug.Log("Bonus Result: " + data);

    ParseResponse(data);

  }
  private void OnDisconnected() //Back2 Start
  {
    Debug.LogWarning("⚠️ Disconnected from server.");
    uiManager.DisconnectionPopup();
    isConnected = false;
    ResetPingRoutine();
  } //Back2 end
  private void OnPongReceived(string data) //Back2 Start
  {
    waitingForPong = false;
    missedPongs = 0;
    lastPongTime = Time.time;
  } //Back2 end
  void CloseGame()
  {
    Debug.Log("Unity: Closing Game");
    StartCoroutine(CloseSocket());
  }
  private void OnError(string response)
  {
    Debug.LogError("Error: " + response);
  }
  void OnResult(string data)
  {
    ParseResponse(data);
  }
  private void OnListenEvent(string data)
  {
    Debug.Log("Received some_event with data: " + data);
    ParseResponse(data);
  }

  private void OnSocketState(bool state)
  {
    if (state)
    {
      Debug.Log("my state is " + state);
    }

  }
  private void OnSocketError(string data)
  {
    Debug.Log("Received error with data: " + data);
  }
  private void OnSocketAlert(string data)
  {
    Debug.Log("Received alert with data: " + data);
  }

  private void OnSocketOtherDevice(string data)
  {
    Debug.Log("Received Device Error with data: " + data);
    uiManager.ADfunction();
  }
  private void OnBalanceSync(string data)
  {
    BalanceSyncPayload syncPayload = JsonConvert.DeserializeObject<BalanceSyncPayload>(data);
    if (syncPayload == null) return;

    if (PlayerData == null) PlayerData = new Player();
    PlayerData.balance = syncPayload.balance;

    slotManager.UpdateBalanceDisplay(syncPayload.balance);
  }
  private void OnError(Error err)
  {
    Debug.LogError("[ERROR] Socket error: " + err);
    if (!string.IsNullOrEmpty(err.message) && err.message.Contains("Session expired"))
    {
      Debug.LogWarning("Session expired detected");
      OnDisconnected();
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("session_expired");
#endif
    }
    else
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("error");
#endif
    }
  }

  // Called ONLY from UIManager.OnFocusChanged (JS/WebGL bridge) — never from a native
  // OnApplicationFocus path. OnApplicationFocus isn't reliable enough inside a WebView to
  // gate a network-affecting timer.
  internal void HandleFocusChange(bool focus)
  {
    hasFocus = focus;

    if (!focus)
    {
      focusLostTime = Time.time;
      if (focusCheckRoutine == null && !isExiting && !isBeingDestroyed)
        focusCheckRoutine = StartCoroutine(FocusTimeoutCheck());
    }
    else
    {
      if (focusCheckRoutine != null)
      {
        StopCoroutine(focusCheckRoutine);
        focusCheckRoutine = null;
      }
    }
  }

  private IEnumerator FocusTimeoutCheck()
  {
    while (!hasFocus && !isExiting && !isBeingDestroyed)
    {
      if (Time.time - focusLostTime >= maxBackgroundTime)
      {
        Debug.LogWarning("[SOCKET] Background timeout — closing connection");
        isConnected = false;
        ResetPingRoutine();

        if (manager != null)
        {
          try { manager.Close(); }
          catch (Exception e) { Debug.LogWarning($"[SOCKET] Focus close error: {e.Message}"); }
        }

        uiManager.DisconnectionPopup();
        focusCheckRoutine = null;
        yield break;
      }

      yield return new WaitForSecondsRealtime(1f);
    }

    focusCheckRoutine = null;
  }

  private void SendPing() //Back2 Start
  {
    ResetPingRoutine();
    PingRoutine = StartCoroutine(PingCheck());
  }
  void ResetPingRoutine()
  {
    if (PingRoutine != null)
    {
      StopCoroutine(PingRoutine);
    }
    PingRoutine = null;
  }
  private IEnumerator PingCheck()
  {
    while (true)
    {
      if (missedPongs == 0)
      {
        uiManager.CheckAndClosePopups();
      }

      // If waiting for pong, and timeout passed
      if (waitingForPong)
      {
        if (missedPongs == 2)
        {
          uiManager.ReconnectionPopup();
        }
        missedPongs++;
        Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

        if (missedPongs >= MaxMissedPongs)
        {
          Debug.LogError("❌ Unable to connect to server — 5 consecutive pongs missed.");
          isConnected = false;
          uiManager.DisconnectionPopup();
          yield break;
        }
      }

      // Send next ping
      waitingForPong = true;
      lastPongTime = Time.time;
      SendDataWithNamespace("ping");
      yield return new WaitForSeconds(pingInterval);
    }
  } //Back2 end

  private void AliveRequest()
  {
    SendDataWithNamespace("YES I AM ALIVE");
  }

  private void SendDataWithNamespace(string eventName, string json = null)
  {
    // Send the message
    if (gameSocket != null && gameSocket.IsOpen) //BackendChanges
    {
      if (json != null)
      {
        gameSocket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);
      }
      else
      {
        gameSocket.Emit(eventName);
      }
    }
    else
    {
      Debug.LogWarning("Socket is not connected.");
    }
  }



  internal IEnumerator CloseSocket() //Back2 Start
  {
    isExiting = true;
    uiManager.RaycastBlocker.SetActive(true);
    ResetPingRoutine();

    Debug.Log("Closing Socket");

    manager?.Close();
    manager = null;

    Debug.Log("Waiting for socket to close");

    yield return new WaitForSeconds(0.5f);

    Debug.Log("Socket Closed");

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit"); //Telling the react platform user wants to quit and go back to homepage
#endif
  } //Back2 end

  internal void ReactNativeCallOnFailedToConnect() //BackendChanges
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("onExit");
#endif
  }

  private void ParseResponse(string jsonObject)
  {
    Debug.Log(jsonObject);
    Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

    string id = myData.id;

    switch (id)
    {
      case "initData":
        {
          InitialData = myData.gameData;
          UIData = myData.uiData;
          PlayerData = myData.player;
          //bonusdata = GetBonusData(myData.gameData.spinBonus);

          if (!SetInit)
          {
            //   Debug.Log(jsonObject);
            List<string> LinesString = ConvertListListIntToListString(InitialData.lines);
            //List<string> InitialReels = ConvertListOfListsToStrings(InitialData.Reel);
            //InitialReels = RemoveQuotes(InitialReels);
            PopulateSlotSocket(LinesString);
            SetInit = true;
          }
          else
          {
            RefreshUI();
          }
          break;
        }
      case "ResultData":
        {
          //  Debug.Log(jsonObject);
          // myData.message.GameData.FinalResultReel = ConvertListOfListsToStrings(myData.message.GameData.ResultReel);
          // myData.message.GameData.FinalsymbolsToEmit = TransformAndRemoveRecurring(myData.message.GameData.symbolsToEmit);
          ResultData = myData;
          PlayerData = myData.player;
          isResultdone = true;
          break;
        }
      case "bonusResult":
        {
          // Debug.Log(jsonObject);
          //UpdateUiOnResult(myData);
          //isResultdone = true;
          slotManager.currentBalance = myData.player.balance;
          PlayerData = myData.player;
          bonusData = myData;
          isResultdone = true;
          break;
        }
      case "ExitUser":
        {
          if (gameSocket != null)
          {
            Debug.Log("Dispose my Socket");
            this.manager.Close();
          }
          // Application.ExternalCall("window.parent.postMessage", "onExit", "*");
#if UNITY_WEBGL && !UNITY_EDITOR
                        JSManager.SendCustomMessage("onExit");
#endif
          break;
        }
    }
  }
  //List<string> GetBonusData(List<int> bonusData)
  //{
  //    List<string> bonusDataString = new List<string>();
  //    foreach (int data in bonusData)
  //    {
  //        bonusDataString.Add(data.ToString());
  //    }
  //    return bonusDataString;
  //}
  //void UpdateUiOnResult(Root myData)
  //{
  //    PlayerData = myData.player;
  //    ResultData.payload.winAmount = myData.payload.winAmount;

  //    slotManager.updateBalance();
  //}
  private void RefreshUI()
  {
    uiManager.InitialiseUIData(UIData.paylines);
  }

  private void PopulateSlotSocket(List<string> LineIds)
  {
    uiManager.RaycastBlocker.SetActive(false);
    slotManager.shuffleInitialMatrix();
    for (int i = 0; i < LineIds.Count; i++)
    {
      slotManager.FetchLines(LineIds[i], i);
    }

    slotManager.SetInitialUI();

    isLoaded = true;
#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnEnter");
#endif
  }

  internal void AccumulateResult(double currBet)
  {
    isResultdone = false;
    MessageData message = new MessageData();
    message.payload = new SentDeta();
    message.type = "SPIN";
    Debug.Log(slotManager.BetCounter);
    message.payload.betIndex = slotManager.BetCounter;
    // Serialize message data to JSON
    string json = JsonUtility.ToJson(message);
    SendDataWithNamespace("request", json);
  }
  internal void OnBonusCollect(int index)
  {
    isResultdone = false;
    MessageData message = new MessageData();
    message.payload = new SentDeta();
    message.type = "BONUS";

    message.payload.betIndex = slotManager.BetCounter;
    message.payload.index = index;
    message.payload.Event = "tap";
    // Serialize message data to JSON
    string json = JsonUtility.ToJson(message);
    SendDataWithNamespace("request", json);
  }
  private List<string> RemoveQuotes(List<string> stringList)
  {
    for (int i = 0; i < stringList.Count; i++)
    {
      stringList[i] = stringList[i].Replace("\"", ""); // Remove inverted commas
    }
    return stringList;
  }

  private List<string> ConvertListListIntToListString(List<List<int>> listOfLists)
  {
    List<string> resultList = new List<string>();

    foreach (List<int> innerList in listOfLists)
    {
      // Convert each integer in the inner list to string
      List<string> stringList = new List<string>();
      foreach (int number in innerList)
      {
        stringList.Add(number.ToString());
      }

      // Join the string representation of integers with ","
      string joinedString = string.Join(",", stringList.ToArray()).Trim();
      resultList.Add(joinedString);
    }

    return resultList;
  }

  private List<string> ConvertListOfListsToStrings(List<List<string>> inputList)
  {
    List<string> outputList = new List<string>();

    foreach (List<string> row in inputList)
    {
      string concatenatedString = string.Join(",", row);
      outputList.Add(concatenatedString);
    }

    return outputList;
  }

  private List<string> TransformAndRemoveRecurring(List<List<string>> originalList)
  {
    // Flattened list
    List<string> flattenedList = new List<string>();
    foreach (List<string> sublist in originalList)
    {
      flattenedList.AddRange(sublist);
    }

    // Remove recurring elements
    HashSet<string> uniqueElements = new HashSet<string>(flattenedList);

    // Transformed list
    List<string> transformedList = new List<string>();
    foreach (string element in uniqueElements)
    {
      transformedList.Add(element.Replace(",", ""));
    }

    return transformedList;
  }
}


[Serializable]
public class MessageData
{
  public string type;

  public SentDeta payload;
}
[Serializable]
public class SentDeta
{
  public int betIndex;
  public string Event;
  public double lastWinning;
  public int index;
}
[Serializable]
public class GameData
{
  public List<List<int>> lines { get; set; }
  public List<double> bets { get; set; }
  public List<int> spinBonus { get; set; }
}

[Serializable]
public class Root
{
  //Result Data
  public bool success { get; set; }
  public List<List<string>> matrix { get; set; }
  public string name { get; set; }
  public Payload payload { get; set; }
  public Bonus bonus { get; set; }
  public Jackpot jackpot { get; set; }
  public Scatter scatter { get; set; }
  public FreeSpins freeSpin { get; set; }
  //Initial Data
  public string id { get; set; }
  public GameData gameData { get; set; }
  public UiData uiData { get; set; }
  public Player player { get; set; }
}
[Serializable]
public class Scatter
{
  public double amount { get; set; }
}
[Serializable]
public class Jackpot
{
  public bool isTriggered { get; set; }
  public double amount { get; set; }
}
[Serializable]
public class Payload
{
  public double winAmount { get; set; }
  public List<Win> wins { get; set; }

  //bonus
  public double payout { get; set; }
}
[Serializable]
public class Win
{
  public int line { get; set; }
  public List<int> positions { get; set; }
  public double amount { get; set; }
}


[Serializable]
public class FreeSpins
{
  public int count { get; set; }
  public bool isFreeSpin { get; set; }
}

[Serializable]
public class Bonus
{
  public bool isTriggered { get; set; }
  public List<int> result { get; set; }
  public int amount { get; set; }
}


[Serializable]
public class UiData
{
  public Paylines paylines { get; set; }
}

[Serializable]
public class Paylines
{
  public List<Symbol> symbols { get; set; }
}

[Serializable]
public class Player
{
  public double balance { get; set; }
}

[Serializable]
public class BalanceSyncPayload
{
  public double balance;
}


[Serializable]
public class Symbol
{
  public int id { get; set; }
  public string name { get; set; }
  public List<int> multiplier { get; set; }
  public string description { get; set; }
}

[Serializable]
public class PlayerData
{
  public double Balance { get; set; }
  public double haveWon { get; set; }
  public double currentWining { get; set; }
}
[Serializable]
public class AuthTokenData
{
  public string cookie;
  public string socketURL;
  public string nameSpace; //BackendChanges
}

[Serializable]
public class BonusData
{
  public string type;
  public string Event;
  public int index;
}
