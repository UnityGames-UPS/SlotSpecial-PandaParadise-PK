using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using Best.HTTP;



public class SocketIOManager : MonoBehaviour
{
    [SerializeField]
    private SlotBehaviour slotManager;

    [SerializeField]
    private UIManager uiManager;

    internal GameData initialData = null;
    internal UiData initUIData = null;
    internal GameData resultData = null;
    internal Root FullResultData = null;
    internal Player playerdata = null;
    [SerializeField]
    internal List<string> bonusdata = null;
    //WebSocket currentSocket = null;
    internal bool isResultdone = false;

    private SocketManager manager;

    [SerializeField]
    internal JSHandler _jsManager;

    protected string SocketURI = null;
    // protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
    protected string TestSocketURI = "http://localhost:5000/";
    // protected string nameSpace="game"; //BackendChanges
    protected string nameSpace = "playground"; //BackendChanges
    private Socket gameSocket; //BackendChanges
    [SerializeField] internal JSFunctCalls JSManager;
    [SerializeField]
    private string testToken;

    protected string gameID = "SL-PP";
    // protected string gameID = "";
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
    [SerializeField] private GameObject RaycastBlocker;

    private void Awake()
    {
        //Debug.unityLogger.logEnabled = false;
        isLoaded = false;
        SetInit = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval(@"
          if(window.ReactNativeWebView){
            window.ReactNativeWebView.postMessage('This is the new version of the game 1.2');
          }
        ");
#endif
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
        // AWSALBTG=data.AWSALBTG;
        // AWSALBTGCORS=data.AWSALBTGCORS;
        SocketURI = data.socketURL;
        myAuth = data.cookie;
        nameSpace = data.nameSpace;
        // Proceed with connecting to the server using myAuth and socketURL
    }

    string myAuth = null;

    private void OpenSocket()
    {
        //Create and setup SocketOptions
        SocketOptions options = new SocketOptions(); //Back2 Start
        options.AutoConnect = false;
        options.Reconnection = false;
        options.Timeout = TimeSpan.FromSeconds(3); //Back2 end
        options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket; //BackendChanges

        //Application.ExternalCall("window.parent.postMessage", "authToken", "*");

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("authToken");
        StartCoroutine(WaitForAuthToken(options));
#else
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = testToken,
                gameId = gameID
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
                gameId = gameID
            };
        };
        options.Auth = authFunction;
        // options.HTTPRequestCustomizationCallback= (SocketManager req, HTTPRequest context)=>{
        //     context.SetHeader("Cookie", $"AWSALBTG={AWSALBTG};, AWSALBTGCORS={AWSALBTGCORS}");
        //     context.SetHeader("X-Custom-Header", "your_custom_value");

        // };
        Debug.Log("Auth function configured with token: " + myAuth);

        // Proceed with connecting to the server
        SetupSocketManager(options);

        yield return null;
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
        gameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected);
        gameSocket.On<Error>(SocketIOEventTypes.Error, OnError);
        gameSocket.On<string>("game:init", OnListenEvent);
        gameSocket.On<string>("result", OnListenEvent);
        gameSocket.On<bool>("socketState", OnSocketState);
        gameSocket.On<string>("internalError", OnSocketError);
        gameSocket.On<string>("alert", OnSocketAlert);
        gameSocket.On<string>("pong", OnPongReceived);
        gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice); //BackendChanges Finish
        manager.Open(); //Back2 Start
    }

    // Connected event handler implementation
    void OnConnected(ConnectResponse resp) //Back2 Start
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

    private void OnDisconnected() //Back2 Start
    {
        Debug.LogWarning("⚠️ Disconnected from server.");
        isConnected = false;
        ResetPingRoutine();
        uiManager.DisconnectionPopup();
    } //Back2 end
    private void OnPongReceived(string data) //Back2 Start
    {
        Debug.Log("✅ Received pong from server.");
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        Debug.Log($"⏱️ Updated last pong time: {lastPongTime}");
        Debug.Log($"📦 Pong payload: {data}");
    } //Back2 end
    private void OnError(Error err)
    {
        Debug.LogError("Socket Error Message: " + err);
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("error");
#endif
    }

    private void OnListenEvent(string data)
    {
        Debug.Log("Received some_event with data: " + data);
        ParseResponse(data);
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval(@"
          if(window.ReactNativeWebView){
            window.ReactNativeWebView.postMessage('Game Socket OnListenEvent');
          }
        ");
#endif
    }
    void CloseGame()
    {
        Debug.Log("Unity: Closing Game");
        StartCoroutine(CloseSocket());
    }

    private void OnSocketState(bool state)
    {
        if (state)
        {
            Debug.Log("my state is " + state);
        }
        else
        {

        }
    }
    private void OnSocketError(string data)
    {
        Debug.Log("Received error with data: " + data);
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval(@"
          if(window.ReactNativeWebView){
            window.ReactNativeWebView.postMessage('Game Socket OnSocketError');
          }
        ");
#endif
    }
    private void OnSocketAlert(string data)
    {
        Debug.Log("Received alert with data: " + data);
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval(@"
          if(window.ReactNativeWebView){
            window.ReactNativeWebView.postMessage('Game Socket Alert');
          }
        ");
#endif
    }

    private void OnSocketOtherDevice(string data)
    {
        Debug.Log("Received Device Error with data: " + data);
        uiManager.ADfunction();
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval(@"
          if(window.ReactNativeWebView){
            window.ReactNativeWebView.postMessage('Game Socket OnSocketOtherDevice');
          }
        ");
#endif
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
            Debug.Log($"🟡 PingCheck | waitingForPong: {waitingForPong}, missedPongs: {missedPongs}, timeSinceLastPong: {Time.time - lastPongTime}");

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
            Debug.Log("📤 Sending ping...");
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
        // Send the message
        if (gameSocket != null && gameSocket.IsOpen)
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
        RaycastBlocker.SetActive(true);
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

    private void ParseResponse(string jsonObject)
    {
        Debug.Log("JSON OBJECT " + jsonObject);
        Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

        string id = myData.id;

        switch (id)
        {
            case "initData":
                {
                    initialData = myData.gameData;
                    initUIData = myData.uiData;
                    playerdata = myData.player;
                    //   bonusdata = myData.message.BonusData;
                    if (!SetInit)
                    {
                        Debug.Log(jsonObject);
                        List<string> LinesString = ConvertListListIntToListString(initialData.lines);
                        // List<string> InitialReels = ConvertListOfListsToStrings(initialData.Reel);
                        // InitialReels = RemoveQuotes(InitialReels);
                        PopulateSlotSocket(LinesString); ///, LinesString //InitialReels
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
                    // Debug.Log(jsonObject);
                    FullResultData = myData;
                    playerdata = myData.player;
                    //  myData.message.GameData.FinalResultReel = ConvertListOfListsToInt(myData.matrix);
                    //myData.message.GameData.FinalsymbolsToEmit =  TransformAndRemoveRecurring(myData.message.GameData.cascade[0].winningLines[0].symbolsToEmit);
                    // resultData = myData.message.GameData;
                    // playerdata = myData.player;
                    isResultdone = true;
                    break;
                }
            case "ExitUser":
                {
                    if (gameSocket != null) //BackendChanges
                    {
                        Debug.Log("Dispose my Socket");
                        this.manager.Close();
                    }
                    //   Application.ExternalCall("window.parent.postMessage", "onExit", "*");
#if UNITY_WEBGL && !UNITY_EDITOR
                        JSManager.SendCustomMessage("onExit");
#endif
                    break;
                }
        }
    }




    internal void ReactNativeCallOnFailedToConnect() //BackendChanges
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("onExit");
#endif
    }

    private void RefreshUI()
    {
        uiManager.InitialiseUIData(initUIData.paylines);
    }

    private void PopulateSlotSocket(List<string> LineIds) //, List<string> LineIds
    {
        slotManager.shuffleInitialMatrix();
        Debug.Log($"LineIDS : count " + LineIds.Count);
        for (int i = 0; i < LineIds.Count; i++)
        {
            slotManager.FetchLines(LineIds[i], i);
        }

        slotManager.SetInitialUI();

        isLoaded = true;
        // Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnEnter");
#endif
        RaycastBlocker.SetActive(false);

    }

    // internal void AccumulateResult(double currBet)
    // {
    //     isResultdone = false;
    //     MessageData message = new MessageData();
    //     message.data = new BetData();
    //     message.data.currentBet = currBet;
    //     message.data.spins = 1;
    //     message.data.currentLines = 9;
    //     message.id = "SPIN";
    //     // Serialize message data to JSON
    //     string json = JsonUtility.ToJson(message);
    //     SendDataWithNamespace("request", json);
    // }
    internal void AccumulateResult(int currBet)
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.type = "SPIN";
        message.payload = new Data();
        message.payload.betIndex = currBet;
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

    private List<string> ConvertListOfListsToInt(List<List<string>> inputList)
    {
        List<string> outputList = new List<string>();

        foreach (List<string> row in inputList)
        {
            string concatenatedString = string.Join(",", row);
            outputList.Add(concatenatedString);
        }

        return outputList;
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

    private List<string> TransformAndRemoveRecurring(List<string> originalList)
    {
        // Flattened list
        List<string> flattenedList = new List<string>();
        // foreach (List<string> sublist in originalList)
        // {
        //     flattenedList.AddRange(sublist);
        // }

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
public class BetData
{
    public double currentBet;
    public double currentLines;
    public double spins;
}

[Serializable]
public class AuthData
{
    public string GameID;
    //public double TotalLines;
}

[Serializable]
public class MessageData
{
    // public BetData data;
    // public string id;

    public string type;
    public Data payload;
}
[Serializable]
public class Data
{
    public int betIndex;
    public string Event;
    public double lastWinning;
    public int index;

}

[Serializable]
public class ExitData
{
    public string id;
}

[Serializable]
public class InitData
{
    public AuthData Data;
    public string id;
}

[Serializable]
public class AbtLogo
{
    public string logoSprite { get; set; }
    public string link { get; set; }
}

[Serializable]
public class GameData
{
    public List<List<string>> Reel { get; set; }
    public List<List<int>> linesApiData { get; set; }
    // public List<double> bets { get; set; }
    public bool canSwitchLines { get; set; }
    public List<int> LinesCount { get; set; }
    public List<int> autoSpin { get; set; }
    public List<List<int>> firstReel { get; set; }
    public List<int> linesToEmit { get; set; }
    public List<List<string>> symbolsToEmit { get; set; }
    public double WinAmout { get; set; }
    //public FreeSpins freeSpins { get; set; }
    public bool isFreeSpin { get; set; }
    public int count { get; set; }

    public List<string> FinalsymbolsToEmit { get; set; }
    public List<string> FinalResultReel { get; set; }
    public double jackpot { get; set; }
    public bool isBonus { get; set; }
    public double BonusStopIndex { get; set; }
    public List<int> FeatureMults { get; set; }
    public List<int> goldWildCol { get; set; }
    public bool featureAll { get; set; }

    public int goldenFrameReel { get; set; }
    public ExpandingWild expandingWild { get; set; }
    public List<Cascade> cascade { get; set; }
    public FreeSpin freeSpin { get; set; }

    //

    public List<List<int>> lines { get; set; }
    public List<double> bets { get; set; }


}

[Serializable]
public class Features
{
    public FreeSpin freeSpin { get; set; }
    public bool isCascade { get; set; }
    public Wild wild { get; set; }
    public Multiplier multiplier { get; set; }
    public double totalCascadeWin { get; set; }

}
[Serializable]
public class Multiplier
{
    public int payout { get; set; }
    public List<object> positions { get; set; }
}

[Serializable]
public class Wild
{
    public bool isColumnWild { get; set; }
    public int column { get; set; }
    public List<List<int>> positions { get; set; }
}

[Serializable]
public class FreeSpin
{
    public int freeSpinCount { get; set; }
    public bool isTriggered { get; set; }
    public List<object> positions { get; set; }
}

[Serializable]
public class FreeSpins
{
    public int count { get; set; }
    public bool isNewAdded { get; set; }
}

[Serializable]
public class Message
{
    public GameData GameData { get; set; }
    public UiData UIData { get; set; }
    public Player PlayerData { get; set; }
    //public List<string> BonusData { get; set; }
}

[Serializable]
public class Root
{
    // public string id { get; set; }
    public Message message { get; set; }

    public string id { get; set; }
    public GameData gameData { get; set; }
    public Features features { get; set; }
    public UiData uiData { get; set; }
    public Player player { get; set; }
    public bool success { get; set; }
    public List<List<string>> matrix { get; set; }
    public Payload payload { get; set; }
}



[Serializable]
public class Payload
{
    public double winAmount { get; set; }
    public List<LineWin> lineWins { get; set; }
}
[Serializable]
public class LineWin
{
    public int lineIndex { get; set; }
    public List<int> positions { get; set; }
    public double win { get; set; }
}
[Serializable]
public class UiData
{
    public Paylines paylines { get; set; }
    // public List<string> spclSymbolTxt { get; set; }
    // public AbtLogo AbtLogo { get; set; }
    // public string ToULink { get; set; }
    // public string PopLink { get; set; }
}

[Serializable]
public class Paylines
{
    public List<Symbol> symbols { get; set; }
}

[Serializable]
public class Symbol
{
    // public int ID { get; set; }
    // public string Name { get; set; }
    // [JsonProperty("multiplier")]
    // public object MultiplierObject { get; set; }

    // // This property will hold the properly deserialized list of lists of integers
    // [JsonIgnore]
    // public List<List<int>> multiplier { get; private set; }

    // // Custom deserialization method to handle the conversion
    // [OnDeserialized]
    // internal void OnDeserializedMethod(StreamingContext context)
    // {
    //     // Handle the case where multiplier is an object (empty in JSON)
    //     if (MultiplierObject is JObject)
    //     {
    //         multiplier = new List<List<int>>();
    //     }
    //     else
    //     {
    //         // Deserialize normally assuming it's an array of arrays
    //         multiplier = JsonConvert.DeserializeObject<List<List<int>>>(MultiplierObject.ToString());
    //     }
    // }
    // public object defaultAmount { get; set; }
    // public object symbolsCount { get; set; }
    // public object increaseValue { get; set; }
    // public object description { get; set; }
    // public int freeSpin { get; set; }


    public int id { get; set; }
    public string name { get; set; }
    //  public ReelsInstance reelsInstance { get; set; }
    public List<int> multiplier { get; set; }
    public string description { get; set; }

}
[Serializable]
public class Player
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
    public string nameSpace;
}

[Serializable]
public class ExpandingWild
{
    public bool isTriggered;
    public int position;
}
[Serializable]
public class Cascade
{
    public int cascadeNumber { get; set; }
    public List<Winning> winnings { get; set; }
    public double totalWin { get; set; }
    public List<List<int>> symbolsToFill { get; set; }
}

[Serializable]
public class Winning
{
    public List<int> line { get; set; }
    public List<string> symbolsToEmit { get; set; }
    public double win { get; set; }
}

// [Serializable]
// public class FreeSpin
// {
//     public bool isTriggered { get; set; }
//     public int freeSpinCount { get; set; }
//     public List<List<int>> symToWild { get; set; }
// }


