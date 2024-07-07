using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Client.Photon;
using UnityEngine;

#region Enums

public enum ClientState
{
	Uninitialized,
	PeerCreated,
	Queued,
	Authenticated,
	JoinedLobby,
	DisconnectingFromMasterserver,
	ConnectingToGameserver,
	ConnectedToGameserver,
	Joining,
	Joined,
	Leaving,
	DisconnectingFromGameserver,
	ConnectingToMasterserver,
	QueuedComingFromGameserver,
	Disconnecting,
	Disconnected,
	ConnectedToMaster,
	ConnectingToNameServer,
	ConnectedToNameServer,
	DisconnectingFromNameServer,
	Authenticating
}

internal enum JoinType
{
	CreateRoom,
	JoinRoom,
	JoinRandomRoom,
	JoinOrCreateRoom
}

public enum DisconnectCause
{
	DisconnectByServerUserLimit = 1042,
	ExceptionOnConnect = 1023,
	DisconnectByServerTimeout = 1041,
	DisconnectByServerLogic = 1043,
	Exception = 1026,
	InvalidAuthentication = 32767,
	MaxCcuReached = 32757,
	InvalidRegion = 32756,
	SecurityExceptionOnConnect = 1022,
	DisconnectByClientTimeout = 1040,
	InternalReceiveException = 1039,
	AuthenticationTicketExpired = 32753
}

public enum ServerConnection
{
	MasterServer,
	GameServer,
	NameServer
}

#endregion

internal class NetworkingPeer : LoadBalancingPeer, IPhotonPeerListener
{
	public const string NameServerHost = "ns.exitgames.com";

	public const string NameServerHttp = "http://ns.exitgamescloud.com:80/photon/n";

	protected internal const string CurrentSceneProperty = "s";

	public const int SyncViewId = 0;

	public const int SyncCompressed = 1;

	public const int SyncNullValues = 2;

	public const int SyncFirstValue = 3;

	protected internal string AppId;

	private string tokenCache;

	public AuthModeOption AuthMode;

	public EncryptionMode EncryptionMode;

	private static readonly Dictionary<ConnectionProtocol, int> ProtocolToNameServerPort = new Dictionary<ConnectionProtocol, int>
	{
		{
			ConnectionProtocol.Udp,
			5058
		},
		{
			ConnectionProtocol.Tcp,
			4533
		},
		{
			ConnectionProtocol.WebSocket,
			9093
		},
		{
			ConnectionProtocol.WebSocketSecure,
			19093
		}
	};

	public bool IsInitialConnect;

	public bool insideLobby;

	protected internal List<TypedLobbyInfo> LobbyStatistics = new List<TypedLobbyInfo>();

	public Dictionary<string, RoomInfo> mGameList = new Dictionary<string, RoomInfo>();

	public RoomInfo[] mGameListCopy = new RoomInfo[0];

	private string playername = string.Empty;

	private bool mPlayernameHasToBeUpdated;

	private Room currentRoom;

	private JoinType lastJoinType;

	protected internal EnterRoomParams enterRoomParamsCache;

	private bool didAuthenticate;

	private string[] friendListRequested;

	private int friendListTimestamp;

	private bool isFetchingFriendList;

	public Dictionary<int, PhotonPlayer> mActors = new Dictionary<int, PhotonPlayer>();

	public PhotonPlayer[] mOtherPlayerListCopy = new PhotonPlayer[0];

	public PhotonPlayer[] mPlayerListCopy = new PhotonPlayer[0];

	public bool hasSwitchedMC;

	private HashSet<byte> allowedReceivingGroups = new HashSet<byte>();

	private HashSet<byte> blockSendingGroups = new HashSet<byte>();

	protected internal Dictionary<int, PhotonView> photonViewList = new Dictionary<int, PhotonView>();

	private readonly PhotonStream readStream = new PhotonStream(false, null);

	private readonly PhotonStream pStream = new PhotonStream(true, null);

	private readonly Dictionary<int, ExitGames.Client.Photon.Hashtable> dataPerGroupReliable = new Dictionary<int, ExitGames.Client.Photon.Hashtable>();

	private readonly Dictionary<int, ExitGames.Client.Photon.Hashtable> dataPerGroupUnreliable = new Dictionary<int, ExitGames.Client.Photon.Hashtable>();

	protected internal short currentLevelPrefix;

	protected internal bool loadingLevelAndPausedNetwork;

	public static bool UsePrefabCache = true;

	internal IPunPrefabPool ObjectPool;

	public static Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();

	private Dictionary<Type, List<MethodInfo>> monoRPCMethodsCache = new Dictionary<Type, List<MethodInfo>>();

	private readonly Dictionary<string, int> rpcShortcuts;

	private static readonly string OnPhotonInstantiateString = PhotonNetworkingMessage.OnPhotonInstantiate.ToString();

	private string cachedServerAddress;

	private string cachedApplicationName;

	private ServerConnection cachedProtocolType;

	private bool _isReconnecting;

	private byte nFalseRPC;

	private int nEncryptKey;

	private int nNetViewID;

	private int nOtherSidePrefix;

	private string nInMethodName = string.Empty;

	private int nRpcIndex;

	private int nSendTime;

	private Dictionary<int, object[]> tempInstantiationData = new Dictionary<int, object[]>();

	public static int ObjectsInOneUpdate = 10;

	private RaiseEventOptions options = new RaiseEventOptions();

	protected internal string AppVersion
	{
		get
		{
			return string.Format("{0}_{1}_{2}", PhotonNetwork.gameVersion, "1.87", "rexetstudio");
		}
	}

	public AuthenticationValues AuthValues { get; set; }

	private string TokenForInit
	{
		get
		{
			if (AuthMode == AuthModeOption.Auth)
			{
				return null;
			}
			return (AuthValues == null) ? null : AuthValues.Token;
		}
	}

	public bool IsUsingNameServer { get; protected internal set; }

	public string NameServerAddress
	{
		get
		{
			return GetNameServerAddress();
		}
	}

	public string MasterServerAddress { get; protected internal set; }

	public string GameServerAddress { get; protected internal set; }

	protected internal ServerConnection Server { get; private set; }

	public ClientState State { get; internal set; }

	public TypedLobby lobby { get; set; }

	private bool requestLobbyStatistics
	{
		get
		{
			return PhotonNetwork.EnableLobbyStatistics && Server == ServerConnection.MasterServer;
		}
	}

	public string PlayerName
	{
		get
		{
			return playername;
		}
		set
		{
			if (!string.IsNullOrEmpty(value) && !value.Equals(playername))
			{
				if (LocalPlayer != null)
				{
					LocalPlayer.NickName = value;
				}
				playername = value;
				if (CurrentRoom != null)
				{
					SendPlayerName();
				}
			}
		}
	}

	public Room CurrentRoom
	{
		get
		{
			if (currentRoom != null && currentRoom.IsLocalClientInside)
			{
				return currentRoom;
			}
			return null;
		}
		private set
		{
			currentRoom = value;
		}
	}

	public PhotonPlayer LocalPlayer { get; internal set; }

	public int PlayersOnMasterCount { get; internal set; }

	public int PlayersInRoomsCount { get; internal set; }

	public int RoomsCount { get; internal set; }

	protected internal int FriendListAge
	{
		get
		{
			return (!isFetchingFriendList && friendListTimestamp != 0) ? (Environment.TickCount - friendListTimestamp) : 0;
		}
	}

	public bool IsAuthorizeSecretAvailable
	{
		get
		{
			return AuthValues != null && !string.IsNullOrEmpty(AuthValues.Token);
		}
	}

	public List<Region> AvailableRegions { get; protected internal set; }

	public CloudRegionCode CloudRegion { get; protected internal set; }

	public int mMasterClientId
	{
		get
		{
			if (PhotonNetwork.offlineMode)
			{
				return LocalPlayer.ID;
			}
			return (CurrentRoom != null) ? CurrentRoom.MasterClientId : 0;
		}
		private set
		{
			if (CurrentRoom != null)
			{
				CurrentRoom.MasterClientId = value;
			}
		}
	}

	public NetworkingPeer(string playername, ConnectionProtocol connectionProtocol)
		: base(connectionProtocol)
	{
		Listener = this;
		LimitOfUnreliableCommands = 40;
		lobby = TypedLobby.Default;
		PlayerName = playername;
		LocalPlayer = new PhotonPlayer(true, -1, this.playername);
		AddNewPlayer(LocalPlayer.ID, LocalPlayer);
		rpcShortcuts = new Dictionary<string, int>(PhotonNetwork.PhotonServerSettings.RpcList.Count);
		for (int i = 0; i < PhotonNetwork.PhotonServerSettings.RpcList.Count; i++)
		{
			string key = PhotonNetwork.PhotonServerSettings.RpcList[i];
			rpcShortcuts[key] = i;
		}
		State = ClientState.PeerCreated;
	}

	private string GetNameServerAddress()
	{
		ConnectionProtocol transportProtocol = TransportProtocol;
		int value = 0;
		ProtocolToNameServerPort.TryGetValue(transportProtocol, out value);
		string arg = string.Empty;
		switch (transportProtocol)
		{
		case ConnectionProtocol.WebSocket:
			arg = "ws://";
			break;
		case ConnectionProtocol.WebSocketSecure:
			arg = "wss://";
			break;
		}
		return string.Format("{0}{1}:{2}", arg, "ns.exitgames.com", value);
	}

	public override bool Connect(string serverAddress, string applicationName)
	{
		Debug.LogError("Avoid using this directly. Thanks.");
		return false;
	}

	public bool ReconnectToMaster()
	{
		if (AuthValues == null)
		{
			Debug.LogWarning("ReconnectToMaster() with AuthValues == null is not correct!");
			AuthValues = new AuthenticationValues();
		}
		AuthValues.Token = tokenCache;
		return Connect(MasterServerAddress, ServerConnection.MasterServer);
	}

	public bool ReconnectAndRejoin()
	{
		if (AuthValues == null)
		{
			Debug.LogWarning("ReconnectAndRejoin() with AuthValues == null is not correct!");
			AuthValues = new AuthenticationValues();
		}
		AuthValues.Token = tokenCache;
		if (!string.IsNullOrEmpty(GameServerAddress) && enterRoomParamsCache != null)
		{
			lastJoinType = JoinType.JoinRoom;
			enterRoomParamsCache.RejoinOnly = true;
			return Connect(GameServerAddress, ServerConnection.GameServer);
		}
		return false;
	}

	public bool Connect(string serverAddress, ServerConnection type)
	{
		if (PhotonHandler.AppQuits)
		{
			Debug.LogWarning("Ignoring Connect() because app gets closed. If this is an error, check PhotonHandler.AppQuits.");
			return false;
		}
		if (State == ClientState.Disconnecting)
		{
			Debug.LogError("Connect() failed. Can't connect while disconnecting (still). Current state: " + PhotonNetwork.connectionStateDetailed);
			return false;
		}
		cachedProtocolType = type;
		cachedServerAddress = serverAddress;
		cachedApplicationName = string.Empty;
		SetupProtocol(type);
		bool flag = base.Connect(serverAddress, string.Empty, TokenForInit);
		if (flag)
		{
			switch (type)
			{
			case ServerConnection.NameServer:
				State = ClientState.ConnectingToNameServer;
				break;
			case ServerConnection.MasterServer:
				State = ClientState.ConnectingToMasterserver;
				break;
			case ServerConnection.GameServer:
				State = ClientState.ConnectingToGameserver;
				break;
			}
		}
		return flag;
	}

	private bool Reconnect()
	{
		_isReconnecting = true;
		PhotonNetwork.SwitchToProtocol(PhotonNetwork.PhotonServerSettings.Protocol);
		SetupProtocol(cachedProtocolType);
		bool flag = base.Connect(cachedServerAddress, cachedApplicationName, TokenForInit);
		if (flag)
		{
			switch (cachedProtocolType)
			{
			case ServerConnection.NameServer:
				State = ClientState.ConnectingToNameServer;
				break;
			case ServerConnection.MasterServer:
				State = ClientState.ConnectingToMasterserver;
				break;
			case ServerConnection.GameServer:
				State = ClientState.ConnectingToGameserver;
				break;
			}
		}
		return flag;
	}

	public bool ConnectToNameServer()
	{
		if (PhotonHandler.AppQuits)
		{
			Debug.LogWarning("Ignoring Connect() because app gets closed. If this is an error, check PhotonHandler.AppQuits.");
			return false;
		}
		IsUsingNameServer = true;
		CloudRegion = CloudRegionCode.none;
		if (State == ClientState.ConnectedToNameServer)
		{
			return true;
		}
		SetupProtocol(ServerConnection.NameServer);
		cachedProtocolType = ServerConnection.NameServer;
		cachedServerAddress = NameServerAddress;
		cachedApplicationName = "ns";
		if (!base.Connect(NameServerAddress, "ns", TokenForInit))
		{
			return false;
		}
		State = ClientState.ConnectingToNameServer;
		return true;
	}

	public bool ConnectToRegionMaster(CloudRegionCode region)
	{
		if (PhotonHandler.AppQuits)
		{
			Debug.LogWarning("Ignoring Connect() because app gets closed. If this is an error, check PhotonHandler.AppQuits.");
			return false;
		}
		IsUsingNameServer = true;
		CloudRegion = region;
		if (State == ClientState.ConnectedToNameServer)
		{
			return CallAuthenticate();
		}
		cachedProtocolType = ServerConnection.NameServer;
		cachedServerAddress = NameServerAddress;
		cachedApplicationName = "ns";
		SetupProtocol(ServerConnection.NameServer);
		if (!base.Connect(NameServerAddress, "ns", TokenForInit))
		{
			return false;
		}
		State = ClientState.ConnectingToNameServer;
		return true;
	}

    protected internal void SetupProtocol(ServerConnection serverType)
    {
        ConnectionProtocol protocolOverride = this.TransportProtocol;

        #if UNITY_XBOXONE
        this.AuthMode = AuthModeOption.Auth;
        if (this.AuthValues == null)
        {
            this.AuthValues = new AuthenticationValues();
            UnityEngine.Debug.LogError("UNITY_XBOXONE builds must set AuthValues. Set this before calling any Connect method. Refer to the online docs for guidance.");
            throw new Exception("UNITY_XBOXONE builds must set AuthValues.");
        }
        if (this.AuthValues.AuthPostData == null)
        {
            UnityEngine.Debug.LogError("UNITY_XBOXONE builds must use Photon's XBox Authentication and set the XSTS token by calling: PhotonNetwork.AuthValues.SetAuthPostData(xstsToken). Refer to the online docs for guidance.");
            Debug.Break();
			throw new Exception("UNITY_XBOXONE builds must use Photon's XBox Authentication.");
        }
        if (this.AuthValues.AuthType != CustomAuthenticationType.Xbox)
        {
            UnityEngine.Debug.LogWarning("UNITY_XBOXONE builds must use AuthValues.AuthType \"CustomAuthenticationType.Xbox\". PUN sets this value now. Refer to the online docs to avoid this warning.");
            this.AuthValues.AuthType = CustomAuthenticationType.Xbox;
        }
        if (this.TransportProtocol != ConnectionProtocol.WebSocketSecure)
        {
            UnityEngine.Debug.LogWarning("UNITY_XBOXONE builds must use WSS (Secure WebSockets) as Transport Protocol. Changing to WSS from your selection: " + this.TransportProtocol);
            this.TransportProtocol = ConnectionProtocol.WebSocketSecure;
        }
        #endif

        if (this.AuthMode == AuthModeOption.AuthOnceWss)
        {
            if (serverType != ServerConnection.NameServer)
            {
                if (PhotonNetwork.logLevel >= PhotonLogLevel.ErrorsOnly)
                {
                    Debug.LogWarning("Using PhotonServerSettings.Protocol when leaving the NameServer (AuthMode is AuthOnceWss): " + PhotonNetwork.PhotonServerSettings.Protocol);
                }
                protocolOverride = PhotonNetwork.PhotonServerSettings.Protocol;
            }
            else
            {
                if (PhotonNetwork.logLevel >= PhotonLogLevel.ErrorsOnly)
                {
                    Debug.LogWarning("Using WebSocket to connect NameServer (AuthMode is AuthOnceWss).");
                }
                protocolOverride = ConnectionProtocol.WebSocketSecure;
            }
        }

        Type socketTcp = null;
        #if UNITY_XBOXONE
        socketTcp = Type.GetType("ExitGames.Client.Photon.SocketWebTcpNativeDynamic, Assembly-CSharp", false);
        if (socketTcp == null)
        {
            socketTcp = Type.GetType("ExitGames.Client.Photon.SocketWebTcpNativeDynamic, Assembly-CSharp-firstpass", false);
        }
        #else
        // to support WebGL export in Unity, we find and assign the SocketWebTcp class (if it's in the project).
        // alternatively class SocketWebTcp might be in the Photon3Unity3D.dll
        socketTcp = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, Assembly-CSharp", false);
        if (socketTcp == null)
        {
            socketTcp = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, Assembly-CSharp-firstpass", false);
        }
        #endif
        if (socketTcp != null)
        {
            this.SocketImplementationConfig[ConnectionProtocol.WebSocket] = socketTcp;
            this.SocketImplementationConfig[ConnectionProtocol.WebSocketSecure] = socketTcp;
        }


        #if UNITY_WEBGL
        if (this.TransportProtocol != ConnectionProtocol.WebSocket && this.TransportProtocol != ConnectionProtocol.WebSocketSecure)
        {
            Debug.Log("WebGL only supports WebSocket protocol. Overriding PhotonServerSettings.");
            protocolOverride = ConnectionProtocol.WebSocketSecure;
        }
        PhotonHandler.PingImplementation = typeof(PingHttp);
        #endif


        #if !UNITY_EDITOR && (UNITY_WINRT)
        // this automatically uses a separate assembly-file with Win8-style Socket usage (not possible in Editor)
        Debug.LogWarning("Using PingWindowsStore");
        PhotonHandler.PingImplementation = typeof(PingWindowsStore);    // but for ping, we have to set the implementation explicitly to Win 8 Store/Phone
        #endif


        #pragma warning disable 0162    // the library variant defines if we should use PUN's SocketUdp variant (at all)
        if (PhotonPeer.NoSocket)
        {
            if (this.AuthMode != AuthModeOption.AuthOnceWss || serverType != ServerConnection.NameServer)
            {
                if (this.TransportProtocol != ConnectionProtocol.Udp)
                {
                    Debug.Log("This Photon3Unity3d.dll only allows UDP. TransportProtocol was: " + this.TransportProtocol + ". SocketImplementation: " + this.SocketImplementation);
                }
                protocolOverride = ConnectionProtocol.Udp;
            }

            #if !UNITY_EDITOR && (UNITY_PS3 || UNITY_ANDROID)
            this.SocketImplementationConfig[ConnectionProtocol.Udp] = typeof(SocketUdpNativeDynamic);
            PhotonHandler.PingImplementation = typeof(PingNativeDynamic);
            #elif !UNITY_EDITOR && (UNITY_IPHONE || UNITY_SWITCH)
            this.SocketImplementationConfig[ConnectionProtocol.Udp] = typeof(SocketUdpNativeStatic);
            PhotonHandler.PingImplementation = typeof(PingNativeStatic);
            #elif !UNITY_EDITOR && UNITY_WINRT
            // this automatically uses a separate assembly-file with Win8-style Socket usage (not possible in Editor)
            #else
            this.SocketImplementationConfig[ConnectionProtocol.Udp] = typeof(SocketUdp);
            PhotonHandler.PingImplementation = typeof(PingMonoEditor);
            #endif

            if (this.SocketImplementationConfig[ConnectionProtocol.Udp] == null)
            {
                Debug.Log("No socket implementation set for 'NoSocket' assembly. Please check your settings.");
            }
        }
        #pragma warning restore 0162

        if (PhotonHandler.PingImplementation == null)
        {
            PhotonHandler.PingImplementation = typeof(PingMono);
        }


        if (this.TransportProtocol == protocolOverride)
        {
            return;
        }


        if (PhotonNetwork.logLevel >= PhotonLogLevel.ErrorsOnly)
        {
            Debug.LogWarning("Protocol switch from: " + this.TransportProtocol + " to: " + protocolOverride + ".");
        }

        this.TransportProtocol = protocolOverride;
    }

	public override void Disconnect()
	{
		if (PeerState == PeerStateValue.Disconnected)
		{
			if (!PhotonHandler.AppQuits)
			{
				Debug.LogWarning(string.Format("Can't execute Disconnect() while not connected. Nothing changed. State: {0}", State));
			}
		}
		else
		{
			State = ClientState.Disconnecting;
			base.Disconnect();
		}
	}

	private bool CallAuthenticate()
	{
		AuthenticationValues authenticationValues = AuthValues;
		if (authenticationValues == null)
		{
			AuthenticationValues authenticationValues2 = new AuthenticationValues();
			authenticationValues2.UserId = PlayerName;
			authenticationValues = authenticationValues2;
		}
		AuthenticationValues authValues = authenticationValues;
		if (AuthMode == AuthModeOption.Auth)
		{
			return OpAuthenticate(AppId, AppVersion, authValues, CloudRegion.ToString(), requestLobbyStatistics);
		}
		return OpAuthenticateOnce(AppId, AppVersion, authValues, CloudRegion.ToString(), EncryptionMode, PhotonNetwork.PhotonServerSettings.Protocol);
	}

	private void DisconnectToReconnect()
	{
		switch (Server)
		{
		case ServerConnection.NameServer:
			State = ClientState.DisconnectingFromNameServer;
			base.Disconnect();
			break;
		case ServerConnection.MasterServer:
			State = ClientState.DisconnectingFromMasterserver;
			base.Disconnect();
			break;
		case ServerConnection.GameServer:
			State = ClientState.DisconnectingFromGameserver;
			base.Disconnect();
			break;
		}
	}

	public bool GetRegions()
	{
		if (Server != ServerConnection.NameServer)
		{
			return false;
		}
		bool flag = OpGetRegions(AppId);
		if (flag)
		{
			AvailableRegions = null;
		}
		return flag;
	}

	public override bool OpFindFriends(string[] friendsToFind)
	{
		if (isFetchingFriendList)
		{
			return false;
		}
		friendListRequested = friendsToFind;
		isFetchingFriendList = true;
		return base.OpFindFriends(friendsToFind);
	}

	public bool OpCreateGame(EnterRoomParams enterRoomParams)
	{
		bool flag = (enterRoomParams.OnGameServer = Server == ServerConnection.GameServer);
		enterRoomParams.PlayerProperties = GetLocalActorProperties();
		if (!flag)
		{
			enterRoomParamsCache = enterRoomParams;
		}
		lastJoinType = JoinType.CreateRoom;
		return base.OpCreateRoom(enterRoomParams);
	}

	public override bool OpJoinRoom(EnterRoomParams opParams)
	{
		if (!(opParams.OnGameServer = Server == ServerConnection.GameServer))
		{
			enterRoomParamsCache = opParams;
		}
		lastJoinType = ((!opParams.CreateIfNotExists) ? JoinType.JoinRoom : JoinType.JoinOrCreateRoom);
		return base.OpJoinRoom(opParams);
	}

	public override bool OpJoinRandomRoom(OpJoinRandomRoomParams opJoinRandomRoomParams)
	{
		enterRoomParamsCache = new EnterRoomParams();
		enterRoomParamsCache.Lobby = opJoinRandomRoomParams.TypedLobby;
		enterRoomParamsCache.ExpectedUsers = opJoinRandomRoomParams.ExpectedUsers;
		lastJoinType = JoinType.JoinRandomRoom;
		return base.OpJoinRandomRoom(opJoinRandomRoomParams);
	}

	public virtual bool OpLeave()
	{
		if (State != ClientState.Joined)
		{
			Debug.LogWarning("Not sending leave operation. State is not 'Joined': " + State);
			return false;
		}
		return OpCustom(254, null, true, 0);
	}

	public override bool OpRaiseEvent(byte eventCode, object customEventContent, bool sendReliable, RaiseEventOptions raiseEventOptions)
	{
		if (PhotonNetwork.offlineMode)
		{
			return false;
		}
		return base.OpRaiseEvent(eventCode, customEventContent, sendReliable, raiseEventOptions);
	}

	private void ReadoutProperties(ExitGames.Client.Photon.Hashtable gameProperties, ExitGames.Client.Photon.Hashtable pActorProperties, int targetActorNr)
	{
		if (pActorProperties != null && pActorProperties.Count > 0)
		{
			if (targetActorNr > 0)
			{
				PhotonPlayer playerWithId = GetPlayerWithId(targetActorNr);
				if (playerWithId != null)
				{
					ExitGames.Client.Photon.Hashtable hashtable = ReadoutPropertiesForActorNr(pActorProperties, targetActorNr);
					playerWithId.InternalCacheProperties(hashtable);
					SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerPropertiesChanged, playerWithId, hashtable);
				}
			}
			else
			{
				foreach (object key in pActorProperties.Keys)
				{
					int num = (int)key;
					ExitGames.Client.Photon.Hashtable hashtable2 = (ExitGames.Client.Photon.Hashtable)pActorProperties[key];
					string name = (string)hashtable2[byte.MaxValue];
					PhotonPlayer photonPlayer = GetPlayerWithId(num);
					if (photonPlayer == null)
					{
						photonPlayer = new PhotonPlayer(false, num, name);
						AddNewPlayer(num, photonPlayer);
					}
					photonPlayer.InternalCacheProperties(hashtable2);
					SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerPropertiesChanged, photonPlayer, hashtable2);
				}
			}
		}
		if (CurrentRoom != null && gameProperties != null)
		{
			CurrentRoom.InternalCacheProperties(gameProperties);
			SendMonoMessage(PhotonNetworkingMessage.OnPhotonCustomRoomPropertiesChanged, gameProperties);
			if (PhotonNetwork.automaticallySyncScene)
			{
				LoadLevelIfSynced();
			}
		}
	}

	private ExitGames.Client.Photon.Hashtable ReadoutPropertiesForActorNr(ExitGames.Client.Photon.Hashtable actorProperties, int actorNr)
	{
		if (actorProperties.ContainsKey(actorNr))
		{
			return (ExitGames.Client.Photon.Hashtable)actorProperties[actorNr];
		}
		return actorProperties;
	}

	public void ChangeLocalID(int newID)
	{
		if (LocalPlayer == null)
		{
			Debug.LogWarning(string.Format("LocalPlayer is null or not in mActors! LocalPlayer: {0} mActors==null: {1} newID: {2}", LocalPlayer, mActors == null, newID));
		}
		if (mActors.ContainsKey(LocalPlayer.ID))
		{
			mActors.Remove(LocalPlayer.ID);
		}
		LocalPlayer.InternalChangeLocalID(newID);
		mActors[LocalPlayer.ID] = LocalPlayer;
		RebuildPlayerListCopies();
	}

	private void LeftLobbyCleanup()
	{
		mGameList = new Dictionary<string, RoomInfo>();
		mGameListCopy = new RoomInfo[0];
		if (insideLobby)
		{
			insideLobby = false;
			SendMonoMessage(PhotonNetworkingMessage.OnLeftLobby);
		}
	}

	private void LeftRoomCleanup()
	{
		bool flag = CurrentRoom != null;
		bool flag2 = ((CurrentRoom == null) ? PhotonNetwork.autoCleanUpPlayerObjects : CurrentRoom.AutoCleanUp);
		hasSwitchedMC = false;
		CurrentRoom = null;
		mActors = new Dictionary<int, PhotonPlayer>();
		mPlayerListCopy = new PhotonPlayer[0];
		mOtherPlayerListCopy = new PhotonPlayer[0];
		allowedReceivingGroups = new HashSet<byte>();
		blockSendingGroups = new HashSet<byte>();
		mGameList = new Dictionary<string, RoomInfo>();
		mGameListCopy = new RoomInfo[0];
		isFetchingFriendList = false;
		ChangeLocalID(-1);
		if (flag2)
		{
			LocalCleanupAnythingInstantiated(true);
			PhotonNetwork.manuallyAllocatedViewIds = new List<int>();
		}
		if (flag)
		{
			SendMonoMessage(PhotonNetworkingMessage.OnLeftRoom);
		}
	}

	protected internal void LocalCleanupAnythingInstantiated(bool destroyInstantiatedGameObjects)
	{
		if (tempInstantiationData.Count > 0)
		{
			Debug.LogWarning("It seems some instantiation is not completed, as instantiation data is used. You should make sure instantiations are paused when calling this method. Cleaning now, despite this.");
		}
		if (destroyInstantiatedGameObjects)
		{
			HashSet<GameObject> hashSet = new HashSet<GameObject>();
			foreach (PhotonView value in photonViewList.Values)
			{
				if (value.isRuntimeInstantiated)
				{
					hashSet.Add(value.gameObject);
				}
			}
			foreach (GameObject item in hashSet)
			{
				RemoveInstantiatedGO(item, true);
			}
		}
		tempInstantiationData.Clear();
		PhotonNetwork.lastUsedViewSubId = 0;
		PhotonNetwork.lastUsedViewSubIdStatic = 0;
	}

	private void GameEnteredOnGameServer(OperationResponse operationResponse)
	{
		if (operationResponse.ReturnCode != 0)
		{
			switch (operationResponse.OperationCode)
			{
			case 227:
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
				{
					Debug.Log("Create failed on GameServer. Changing back to MasterServer. Msg: " + operationResponse.DebugMessage);
				}
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonCreateRoomFailed, operationResponse.ReturnCode, operationResponse.DebugMessage);
				break;
			case 226:
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
				{
					Debug.Log("Join failed on GameServer. Changing back to MasterServer. Msg: " + operationResponse.DebugMessage);
					if (operationResponse.ReturnCode == 32758)
					{
						Debug.Log("Most likely the game became empty during the switch to GameServer.");
					}
				}
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonJoinRoomFailed, operationResponse.ReturnCode, operationResponse.DebugMessage);
				break;
			case 225:
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
				{
					Debug.Log("Join failed on GameServer. Changing back to MasterServer. Msg: " + operationResponse.DebugMessage);
					if (operationResponse.ReturnCode == 32758)
					{
						Debug.Log("Most likely the game became empty during the switch to GameServer.");
					}
				}
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonRandomJoinFailed, operationResponse.ReturnCode, operationResponse.DebugMessage);
				break;
			}
			DisconnectToReconnect();
		}
		else
		{
			Room room = new Room(enterRoomParamsCache.RoomName, null);
			room.IsLocalClientInside = true;
			CurrentRoom = room;
			State = ClientState.Joined;
			if (operationResponse.Parameters.ContainsKey(252))
			{
				int[] actorsInRoom = (int[])operationResponse.Parameters[252];
				UpdatedActorList(actorsInRoom);
			}
			int newID = (int)operationResponse[254];
			ChangeLocalID(newID);
			ExitGames.Client.Photon.Hashtable pActorProperties = (ExitGames.Client.Photon.Hashtable)operationResponse[249];
			ExitGames.Client.Photon.Hashtable gameProperties = (ExitGames.Client.Photon.Hashtable)operationResponse[248];
			ReadoutProperties(gameProperties, pActorProperties, 0);
			if (!CurrentRoom.serverSideMasterClient)
			{
				CheckMasterClient(-1);
			}
			if (mPlayernameHasToBeUpdated)
			{
				SendPlayerName();
			}
			switch (operationResponse.OperationCode)
			{
			case 227:
				SendMonoMessage(PhotonNetworkingMessage.OnCreatedRoom);
				break;
			case 225:
			case 226:
				break;
			}
		}
	}

	private void AddNewPlayer(int ID, PhotonPlayer player)
	{
		if (!mActors.ContainsKey(ID))
		{
			mActors[ID] = player;
			RebuildPlayerListCopies();
		}
		else
		{
			Debug.LogError("Adding player twice: " + ID);
		}
	}

	private void RemovePlayer(int ID, PhotonPlayer player)
	{
		mActors.Remove(ID);
		if (!player.IsLocal)
		{
			RebuildPlayerListCopies();
		}
	}

	private void RebuildPlayerListCopies()
	{
		mPlayerListCopy = new PhotonPlayer[mActors.Count];
		mActors.Values.CopyTo(mPlayerListCopy, 0);
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		for (int i = 0; i < mPlayerListCopy.Length; i++)
		{
			PhotonPlayer photonPlayer = mPlayerListCopy[i];
			if (!photonPlayer.IsLocal)
			{
				list.Add(photonPlayer);
			}
		}
		mOtherPlayerListCopy = list.ToArray();
	}

	private void ResetPhotonViewsOnSerialize()
	{
		foreach (PhotonView value in photonViewList.Values)
		{
			value.lastOnSerializeDataSent = null;
		}
	}

	private void HandleEventLeave(int actorID, EventData evLeave)
	{
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log("HandleEventLeave for player ID: " + actorID + " evLeave: " + evLeave.ToStringFull());
		}
		PhotonPlayer playerWithId = GetPlayerWithId(actorID);
		if (playerWithId == null)
		{
			Debug.LogError(string.Format("Received event Leave for unknown player ID: {0}", actorID));
			return;
		}
		bool isInactive = playerWithId.IsInactive;
		if (evLeave.Parameters.ContainsKey(233))
		{
			playerWithId.IsInactive = (bool)evLeave.Parameters[233];
			if (playerWithId.IsInactive != isInactive)
			{
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerActivityChanged, playerWithId);
			}
			if (playerWithId.IsInactive && isInactive)
			{
				Debug.LogWarning("HandleEventLeave for player ID: " + actorID + " isInactive: " + playerWithId.IsInactive + ". Stopping handling if inactive.");
				return;
			}
		}
		if (evLeave.Parameters.ContainsKey(203))
		{
			if ((int)evLeave[203] != 0)
			{
				mMasterClientId = (int)evLeave[203];
				UpdateMasterClient();
			}
		}
		else if (!CurrentRoom.serverSideMasterClient)
		{
			CheckMasterClient(actorID);
		}
		if (!playerWithId.IsInactive || isInactive)
		{
			if (CurrentRoom != null && CurrentRoom.AutoCleanUp)
			{
				DestroyPlayerObjects(actorID, true);
			}
			RemovePlayer(actorID, playerWithId);
			SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerDisconnected, playerWithId);
		}
	}

	private void CheckMasterClient(int leavingPlayerId)
	{
		bool flag = mMasterClientId == leavingPlayerId;
		bool flag2 = leavingPlayerId > 0;
		if (flag2 && !flag)
		{
			return;
		}
		int num;
		if (mActors.Count <= 1)
		{
			num = LocalPlayer.ID;
		}
		else
		{
			num = int.MaxValue;
			foreach (int key in mActors.Keys)
			{
				if (key < num && key != leavingPlayerId)
				{
					num = key;
				}
			}
		}
		mMasterClientId = num;
		if (flag2)
		{
			SendMonoMessage(PhotonNetworkingMessage.OnMasterClientSwitched, GetPlayerWithId(num));
		}
	}

	protected internal void UpdateMasterClient()
	{
		SendMonoMessage(PhotonNetworkingMessage.OnMasterClientSwitched, PhotonNetwork.masterClient);
	}

	private static int ReturnLowestPlayerId(PhotonPlayer[] players, int playerIdToIgnore)
	{
		if (players == null || players.Length == 0)
		{
			return -1;
		}
		int num = int.MaxValue;
		foreach (PhotonPlayer photonPlayer in players)
		{
			if (photonPlayer.ID != playerIdToIgnore && photonPlayer.ID < num)
			{
				num = photonPlayer.ID;
			}
		}
		return num;
	}

	protected internal bool SetMasterClient(int playerId, bool sync)
	{
		if (mMasterClientId == playerId || !mActors.ContainsKey(playerId))
		{
			return false;
		}
		if (sync && !OpRaiseEvent(208, new ExitGames.Client.Photon.Hashtable { 
		{
			(byte)1,
			playerId
		} }, true, null))
		{
			return false;
		}
		hasSwitchedMC = true;
		CurrentRoom.MasterClientId = playerId;
		SendMonoMessage(PhotonNetworkingMessage.OnMasterClientSwitched, GetPlayerWithId(playerId));
		return true;
	}

	public bool SetMasterClient(int nextMasterId)
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable.Add((byte)248, nextMasterId);
		ExitGames.Client.Photon.Hashtable gameProperties = hashtable;
		hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable.Add((byte)248, mMasterClientId);
		ExitGames.Client.Photon.Hashtable expectedProperties = hashtable;
		return OpSetPropertiesOfRoom(gameProperties, expectedProperties);
	}

	protected internal PhotonPlayer GetPlayerWithId(int number)
	{
		if (mActors == null)
		{
			return null;
		}
		PhotonPlayer value = null;
		mActors.TryGetValue(number, out value);
		return value;
	}

	private void SendPlayerName()
	{
		if (State == ClientState.Joining)
		{
			mPlayernameHasToBeUpdated = true;
		}
		else if (LocalPlayer != null)
		{
			LocalPlayer.NickName = PlayerName;
			ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
			hashtable[byte.MaxValue] = PlayerName;
			if (LocalPlayer.ID > 0)
			{
				OpSetPropertiesOfActor(LocalPlayer.ID, hashtable);
				mPlayernameHasToBeUpdated = false;
			}
		}
	}

	private ExitGames.Client.Photon.Hashtable GetLocalActorProperties()
	{
		if (PhotonNetwork.player != null)
		{
			return PhotonNetwork.player.AllProperties;
		}
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable[byte.MaxValue] = PlayerName;
		return hashtable;
	}

	public void DebugReturn(DebugLevel level, string message)
	{
		switch (level)
		{
		case DebugLevel.ERROR:
			Debug.LogError(message);
			return;
		case DebugLevel.WARNING:
			Debug.LogWarning(message);
			return;
		case DebugLevel.INFO:
			if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
			{
				Debug.Log(message);
				return;
			}
			break;
		}
		if (level == DebugLevel.ALL && PhotonNetwork.logLevel == PhotonLogLevel.Full)
		{
			Debug.Log(message);
		}
	}

	public void OnOperationResponse(OperationResponse operationResponse)
	{
		if (PhotonNetwork.networkingPeer.State == ClientState.Disconnecting)
		{
			if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
			{
				Debug.Log("OperationResponse ignored while disconnecting. Code: " + operationResponse.OperationCode);
			}
			return;
		}
		if (operationResponse.ReturnCode == 0)
		{
			if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
			{
				Debug.Log(operationResponse.ToString());
			}
		}
		else if (operationResponse.ReturnCode == -3)
		{
			Debug.LogError("Operation " + operationResponse.OperationCode + " could not be executed (yet). Wait for state JoinedLobby or ConnectedToMaster and their callbacks before calling operations. WebRPCs need a server-side configuration. Enum OperationCode helps identify the operation.");
		}
		else if (operationResponse.ReturnCode == 32752)
		{
			Debug.LogError("Operation " + operationResponse.OperationCode + " failed in a server-side plugin. Check the configuration in the Dashboard. Message from server-plugin: " + operationResponse.DebugMessage);
		}
		else if (operationResponse.ReturnCode == 32760)
		{
			Debug.LogWarning("Operation failed: " + operationResponse.ToStringFull());
		}
		if (operationResponse.Parameters.ContainsKey(221))
		{
			if (AuthValues == null)
			{
				AuthValues = new AuthenticationValues();
			}
			AuthValues.Token = operationResponse[221] as string;
			tokenCache = AuthValues.Token;
		}
		switch (operationResponse.OperationCode)
		{
		case 230:
		case 231:
			if (operationResponse.ReturnCode != 0)
			{
				if (operationResponse.ReturnCode == -2)
				{
					Debug.LogError(string.Format("If you host Photon yourself, make sure to start the 'Instance LoadBalancing' " + ServerAddress));
				}
				else if (operationResponse.ReturnCode == short.MaxValue)
				{
					Debug.LogError(string.Format("The appId this client sent is unknown on the server (Cloud). Check settings. If using the Cloud, check account."));
					SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, DisconnectCause.InvalidAuthentication);
				}
				else if (operationResponse.ReturnCode == 32755)
				{
					Debug.LogError(string.Format("Custom Authentication failed (either due to user-input or configuration or AuthParameter string format). Calling: OnCustomAuthenticationFailed()"));
					SendMonoMessage(PhotonNetworkingMessage.OnCustomAuthenticationFailed, operationResponse.DebugMessage);
				}
				else
				{
					Debug.LogError(string.Format("Authentication failed: '{0}' Code: {1}", operationResponse.DebugMessage, operationResponse.ReturnCode));
				}
				State = ClientState.Disconnecting;
				Disconnect();
				if (operationResponse.ReturnCode == 32757)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.LogWarning(string.Format("Currently, the limit of users is reached for this title. Try again later. Disconnecting"));
					}
					SendMonoMessage(PhotonNetworkingMessage.OnPhotonMaxCccuReached);
					SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, DisconnectCause.MaxCcuReached);
				}
				else if (operationResponse.ReturnCode == 32756)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.LogError(string.Format("The used master server address is not available with the subscription currently used. Got to Photon Cloud Dashboard or change URL. Disconnecting."));
					}
					SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, DisconnectCause.InvalidRegion);
				}
				else if (operationResponse.ReturnCode == 32753)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.LogError(string.Format("The authentication ticket expired. You need to connect (and authenticate) again. Disconnecting."));
					}
					SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, DisconnectCause.AuthenticationTicketExpired);
				}
				break;
			}
			if (Server == ServerConnection.NameServer || Server == ServerConnection.MasterServer)
			{
				if (operationResponse.Parameters.ContainsKey(225))
				{
					string text4 = (string)operationResponse.Parameters[225];
					if (!string.IsNullOrEmpty(text4))
					{
						if (AuthValues == null)
						{
							AuthValues = new AuthenticationValues();
						}
						AuthValues.UserId = text4;
						PhotonNetwork.player.UserId = text4;
						if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
						{
							DebugReturn(DebugLevel.INFO, string.Format("Received your UserID from server. Updating local value to: {0}", text4));
						}
					}
				}
				if (operationResponse.Parameters.ContainsKey(202))
				{
					PlayerName = (string)operationResponse.Parameters[202];
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						DebugReturn(DebugLevel.INFO, string.Format("Received your NickName from server. Updating local value to: {0}", playername));
					}
				}
				if (operationResponse.Parameters.ContainsKey(192))
				{
					SetupEncryption((Dictionary<byte, object>)operationResponse.Parameters[192]);
				}
			}
			if (Server == ServerConnection.NameServer)
			{
				MasterServerAddress = operationResponse[230] as string;
				DisconnectToReconnect();
			}
			else if (Server == ServerConnection.MasterServer)
			{
				if (AuthMode != 0)
				{
					OpSettings(requestLobbyStatistics);
				}
				if (PhotonNetwork.autoJoinLobby)
				{
					State = ClientState.Authenticated;
					OpJoinLobby(lobby);
				}
				else
				{
					State = ClientState.ConnectedToMaster;
					SendMonoMessage(PhotonNetworkingMessage.OnConnectedToMaster);
				}
			}
			else if (Server == ServerConnection.GameServer)
			{
				State = ClientState.Joining;
				enterRoomParamsCache.PlayerProperties = GetLocalActorProperties();
				enterRoomParamsCache.OnGameServer = true;
				if (lastJoinType == JoinType.JoinRoom || lastJoinType == JoinType.JoinRandomRoom || lastJoinType == JoinType.JoinOrCreateRoom)
				{
					OpJoinRoom(enterRoomParamsCache);
				}
				else if (lastJoinType == JoinType.CreateRoom)
				{
					OpCreateGame(enterRoomParamsCache);
				}
			}
			if (operationResponse.Parameters.ContainsKey(245))
			{
				Dictionary<string, object> dictionary = (Dictionary<string, object>)operationResponse.Parameters[245];
				if (dictionary != null)
				{
					SendMonoMessage(PhotonNetworkingMessage.OnCustomAuthenticationResponse, dictionary);
				}
			}
			break;
		case 220:
		{
			if (operationResponse.ReturnCode == short.MaxValue)
			{
				Debug.LogError(string.Format("The appId this client sent is unknown on the server (Cloud). Check settings. If using the Cloud, check account."));
				SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, DisconnectCause.InvalidAuthentication);
				State = ClientState.Disconnecting;
				Disconnect();
				break;
			}
			if (operationResponse.ReturnCode != 0)
			{
				Debug.LogError("GetRegions failed. Can't provide regions list. Error: " + operationResponse.ReturnCode + ": " + operationResponse.DebugMessage);
				break;
			}
			string[] array3 = operationResponse[210] as string[];
			string[] array4 = operationResponse[230] as string[];
			if (array3 == null || array4 == null || array3.Length != array4.Length)
			{
				Debug.LogError("The region arrays from Name Server are not ok. Must be non-null and same length. " + (array3 == null) + " " + (array4 == null) + "\n" + operationResponse.ToStringFull());
				break;
			}
			AvailableRegions = new List<Region>(array3.Length);
			for (int j = 0; j < array3.Length; j++)
			{
				string text3 = array3[j];
				if (string.IsNullOrEmpty(text3))
				{
					continue;
				}
				text3 = text3.ToLower();
				CloudRegionCode cloudRegionCode = Region.Parse(text3);
				bool flag = true;
				if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.BestRegion && PhotonNetwork.PhotonServerSettings.EnabledRegions != 0)
				{
					CloudRegionFlag cloudRegionFlag = Region.ParseFlag(cloudRegionCode);
					flag = (PhotonNetwork.PhotonServerSettings.EnabledRegions & cloudRegionFlag) != 0;
					if (!flag && PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.Log("Skipping region because it's not in PhotonServerSettings.EnabledRegions: " + cloudRegionCode);
					}
				}
				if (flag)
				{
					AvailableRegions.Add(new Region(cloudRegionCode, text3, array4[j]));
				}
			}
			if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.BestRegion)
			{
				PhotonHandler.PingAvailableRegionsAndConnectToBest();
			}
			break;
		}
		case 227:
			if (Server == ServerConnection.GameServer)
			{
				GameEnteredOnGameServer(operationResponse);
			}
			else if (operationResponse.ReturnCode != 0)
			{
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
				{
					Debug.LogWarning(string.Format("CreateRoom failed, client stays on masterserver: {0}.", operationResponse.ToStringFull()));
				}
				State = ((!insideLobby) ? ClientState.ConnectedToMaster : ClientState.JoinedLobby);
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonCreateRoomFailed, operationResponse.ReturnCode, operationResponse.DebugMessage);
			}
			else
			{
				string text2 = (string)operationResponse[byte.MaxValue];
				if (!string.IsNullOrEmpty(text2))
				{
					enterRoomParamsCache.RoomName = text2;
				}
				GameServerAddress = (string)operationResponse[230];
				DisconnectToReconnect();
			}
			break;
		case 226:
			if (Server != ServerConnection.GameServer)
			{
				if (operationResponse.ReturnCode != 0)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.Log(string.Format("JoinRoom failed (room maybe closed by now). Client stays on masterserver: {0}. State: {1}", operationResponse.ToStringFull(), State));
					}
					SendMonoMessage(PhotonNetworkingMessage.OnPhotonJoinRoomFailed, operationResponse.ReturnCode, operationResponse.DebugMessage);
				}
				else
				{
					GameServerAddress = (string)operationResponse[230];
					DisconnectToReconnect();
				}
			}
			else
			{
				GameEnteredOnGameServer(operationResponse);
			}
			break;
		case 225:
			if (operationResponse.ReturnCode != 0)
			{
				if (operationResponse.ReturnCode == 32760)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
					{
						Debug.Log("JoinRandom failed: No open game. Calling: OnPhotonRandomJoinFailed() and staying on master server.");
					}
				}
				else if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
				{
					Debug.LogWarning(string.Format("JoinRandom failed: {0}.", operationResponse.ToStringFull()));
				}
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonRandomJoinFailed, operationResponse.ReturnCode, operationResponse.DebugMessage);
			}
			else
			{
				string roomName = (string)operationResponse[byte.MaxValue];
				enterRoomParamsCache.RoomName = roomName;
				GameServerAddress = (string)operationResponse[230];
				DisconnectToReconnect();
			}
			break;
		case 217:
		{
			if (operationResponse.ReturnCode != 0)
			{
				DebugReturn(DebugLevel.ERROR, "GetGameList failed: " + operationResponse.ToStringFull());
				break;
			}
			mGameList = new Dictionary<string, RoomInfo>();
			ExitGames.Client.Photon.Hashtable hashtable = (ExitGames.Client.Photon.Hashtable)operationResponse[222];
			foreach (object key in hashtable.Keys)
			{
				string text = (string)key;
				mGameList[text] = new RoomInfo(text, (ExitGames.Client.Photon.Hashtable)hashtable[key]);
			}
			mGameListCopy = new RoomInfo[mGameList.Count];
			mGameList.Values.CopyTo(mGameListCopy, 0);
			SendMonoMessage(PhotonNetworkingMessage.OnReceivedRoomListUpdate);
			break;
		}
		case 229:
			State = ClientState.JoinedLobby;
			insideLobby = true;
			SendMonoMessage(PhotonNetworkingMessage.OnJoinedLobby);
			break;
		case 228:
			State = ClientState.Authenticated;
			LeftLobbyCleanup();
			break;
		case 254:
			DisconnectToReconnect();
			break;
		case 251:
		{
			ExitGames.Client.Photon.Hashtable pActorProperties = (ExitGames.Client.Photon.Hashtable)operationResponse[249];
			ExitGames.Client.Photon.Hashtable gameProperties = (ExitGames.Client.Photon.Hashtable)operationResponse[248];
			ReadoutProperties(gameProperties, pActorProperties, 0);
			break;
		}
		case 252:
		case 253:
			break;
		case 222:
		{
			bool[] array = operationResponse[1] as bool[];
			string[] array2 = operationResponse[2] as string[];
			if (array != null && array2 != null && friendListRequested != null && array.Length == friendListRequested.Length)
			{
				List<FriendInfo> list = new List<FriendInfo>(friendListRequested.Length);
				for (int i = 0; i < friendListRequested.Length; i++)
				{
					FriendInfo friendInfo = new FriendInfo();
					friendInfo.Name = friendListRequested[i];
					friendInfo.Room = array2[i];
					friendInfo.IsOnline = array[i];
					list.Insert(i, friendInfo);
				}
				PhotonNetwork.Friends = list;
			}
			else
			{
				Debug.LogError("FindFriends failed to apply the result, as a required value wasn't provided or the friend list length differed from result.");
			}
			friendListRequested = null;
			isFetchingFriendList = false;
			friendListTimestamp = Environment.TickCount;
			if (friendListTimestamp == 0)
			{
				friendListTimestamp = 1;
			}
			SendMonoMessage(PhotonNetworkingMessage.OnUpdatedFriendList);
			break;
		}
		case 219:
			SendMonoMessage(PhotonNetworkingMessage.OnWebRpcResponse, operationResponse);
			break;
		default:
			Debug.LogWarning(string.Format("OperationResponse unhandled: {0}", operationResponse.ToString()));
			break;
		}
	}

	public void OnStatusChanged(StatusCode statusCode)
	{
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log(string.Format("OnStatusChanged: {0} current State: {1}", statusCode.ToString(), State));
		}
		switch (statusCode)
		{
		case StatusCode.Connect:
			if (State == ClientState.ConnectingToNameServer)
			{
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
				{
					Debug.Log("Connected to NameServer.");
				}
				Server = ServerConnection.NameServer;
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
			}
			if (State == ClientState.ConnectingToGameserver)
			{
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
				{
					Debug.Log("Connected to gameserver.");
				}
				Server = ServerConnection.GameServer;
				State = ClientState.ConnectedToGameserver;
			}
			if (State == ClientState.ConnectingToMasterserver)
			{
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
				{
					Debug.Log("Connected to masterserver.");
				}
				Server = ServerConnection.MasterServer;
				State = ClientState.Authenticating;
				if (IsInitialConnect)
				{
					IsInitialConnect = false;
					SendMonoMessage(PhotonNetworkingMessage.OnConnectedToPhoton);
				}
			}
			if (TransportProtocol != ConnectionProtocol.WebSocketSecure)
			{
				if (Server == ServerConnection.NameServer || AuthMode == AuthModeOption.Auth)
				{
					EstablishEncryption();
				}
				break;
			}
			if (DebugOut == DebugLevel.INFO)
			{
				Debug.Log("Skipping EstablishEncryption. Protocol is secure.");
			}
			goto case StatusCode.EncryptionEstablished;
		case StatusCode.EncryptionEstablished:
			_isReconnecting = false;
			if (Server == ServerConnection.NameServer)
			{
				State = ClientState.ConnectedToNameServer;
				if (!didAuthenticate && CloudRegion == CloudRegionCode.none)
				{
					OpGetRegions(AppId);
				}
			}
			if (Server != ServerConnection.NameServer && (AuthMode == AuthModeOption.AuthOnce || AuthMode == AuthModeOption.AuthOnceWss))
			{
				Debug.Log("didAuthenticate " + didAuthenticate + " AuthMode " + AuthMode);
			}
			else if (!didAuthenticate && (!IsUsingNameServer || CloudRegion != CloudRegionCode.none))
			{
				didAuthenticate = CallAuthenticate();
				if (didAuthenticate)
				{
					State = ClientState.Authenticating;
				}
			}
			break;
		case StatusCode.EncryptionFailedToEstablish:
		{
			Debug.LogError(string.Concat("Encryption wasn't established: ", statusCode, ". Going to authenticate anyways."));
			AuthenticationValues authenticationValues = AuthValues;
			if (authenticationValues == null)
			{
				AuthenticationValues authenticationValues2 = new AuthenticationValues();
				authenticationValues2.UserId = PlayerName;
				authenticationValues = authenticationValues2;
			}
			AuthenticationValues authValues = authenticationValues;
			OpAuthenticate(AppId, AppVersion, authValues, CloudRegion.ToString(), requestLobbyStatistics);
			break;
		}
		case StatusCode.Disconnect:
			didAuthenticate = false;
			isFetchingFriendList = false;
			if (Server == ServerConnection.GameServer)
			{
				LeftRoomCleanup();
			}
			if (Server == ServerConnection.MasterServer)
			{
				LeftLobbyCleanup();
			}
			if (State == ClientState.DisconnectingFromMasterserver)
			{
				if (Connect(GameServerAddress, ServerConnection.GameServer))
				{
					State = ClientState.ConnectingToGameserver;
				}
			}
			else if (State == ClientState.DisconnectingFromGameserver || State == ClientState.DisconnectingFromNameServer)
			{
				SetupProtocol(ServerConnection.MasterServer);
				if (Connect(MasterServerAddress, ServerConnection.MasterServer))
				{
					State = ClientState.ConnectingToMasterserver;
				}
			}
			else if (!_isReconnecting)
			{
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
				IsInitialConnect = false;
				State = ClientState.PeerCreated;
				SendMonoMessage(PhotonNetworkingMessage.OnDisconnectedFromPhoton);
			}
			break;
		case StatusCode.SecurityExceptionOnConnect:
		case StatusCode.ExceptionOnConnect:
		{
			IsInitialConnect = false;
			State = ClientState.PeerCreated;
			if (AuthValues != null)
			{
				AuthValues.Token = null;
			}
			DisconnectCause disconnectCause = (DisconnectCause)statusCode;
			SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, disconnectCause);
			break;
		}
		case StatusCode.Exception:
			if (IsInitialConnect)
			{
				Debug.LogError("Exception while connecting to: " + ServerAddress + ". Check if the server is available.");
				if (ServerAddress == null || ServerAddress.StartsWith("127.0.0.1"))
				{
					Debug.LogWarning("The server address is 127.0.0.1 (localhost): Make sure the server is running on this machine. Android and iOS emulators have their own localhost.");
					if (ServerAddress == GameServerAddress)
					{
						Debug.LogWarning("This might be a misconfiguration in the game server config. You need to edit it to a (public) address.");
					}
				}
				State = ClientState.PeerCreated;
				DisconnectCause disconnectCause = (DisconnectCause)statusCode;
				IsInitialConnect = false;
				SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, disconnectCause);
			}
			else
			{
				State = ClientState.PeerCreated;
				DisconnectCause disconnectCause = (DisconnectCause)statusCode;
				SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, disconnectCause);
			}
			Disconnect();
			break;
		case StatusCode.TimeoutDisconnect:
			if (IsInitialConnect)
			{
				if (!_isReconnecting)
				{
					Debug.LogWarning(string.Concat(statusCode, " while connecting to: ", ServerAddress, ". Check if the server is available."));
					IsInitialConnect = false;
					DisconnectCause disconnectCause = (DisconnectCause)statusCode;
					SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, disconnectCause);
				}
			}
			else
			{
				DisconnectCause disconnectCause = (DisconnectCause)statusCode;
				SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, disconnectCause);
			}
			if (AuthValues != null)
			{
				AuthValues.Token = null;
			}
			Disconnect();
			break;
		case StatusCode.ExceptionOnReceive:
		case StatusCode.DisconnectByServer:
		case StatusCode.DisconnectByServerUserLimit:
		case StatusCode.DisconnectByServerLogic:
			if (IsInitialConnect)
			{
				Debug.LogWarning(string.Concat(statusCode, " while connecting to: ", ServerAddress, ". Check if the server is available."));
				IsInitialConnect = false;
				DisconnectCause disconnectCause = (DisconnectCause)statusCode;
				SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, disconnectCause);
			}
			else
			{
				DisconnectCause disconnectCause = (DisconnectCause)statusCode;
				SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, disconnectCause);
			}
			if (AuthValues != null)
			{
				AuthValues.Token = null;
			}
			Disconnect();
			break;
		case StatusCode.SendError:
			break;
		default:
			Debug.LogError("Received unknown status code: " + statusCode);
			break;
		}
	}

	public void OnEvent(EventData photonEvent)
	{
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log(string.Format("OnEvent: {0}", photonEvent.ToString()));
		}
		int num = -1;
		PhotonPlayer photonPlayer = null;
		if (photonEvent.Parameters.ContainsKey(254))
		{
			num = (int)photonEvent[254];
			photonPlayer = GetPlayerWithId(num);
		}
		switch (photonEvent.Code)
		{
		case 209:
		{
			int[] array = (int[])photonEvent.Parameters[245];
			int num2 = array[0];
			int num3 = array[1];
			PhotonView photonView = PhotonView.Find(num2);
			if (photonView == null)
			{
				Debug.LogWarning("Can't find PhotonView of incoming OwnershipRequest. ViewId not found: " + num2);
				return;
			}
			if (PhotonNetwork.logLevel == PhotonLogLevel.Informational)
			{
				Debug.Log(string.Concat("Ev OwnershipRequest ", photonView.ownershipTransfer, ". ActorNr: ", num, " takes from: ", num3, ". local RequestedView.ownerId: ", photonView.ownerId, " isOwnerActive: ", photonView.isOwnerActive, ". MasterClient: ", mMasterClientId, ". This client's player: ", PhotonNetwork.player.ToStringFull()));
			}
			switch (photonView.ownershipTransfer)
			{
			case OwnershipOption.Fixed:
				Debug.LogWarning("Ownership mode == fixed. Ignoring request.");
				break;
			case OwnershipOption.Takeover:
				if (num3 == photonView.ownerId || (num3 == 0 && photonView.ownerId == mMasterClientId) || photonView.ownerId == 0)
				{
					photonView.OwnerShipWasTransfered = true;
					int ownerId = photonView.ownerId;
					PhotonPlayer playerWithId = GetPlayerWithId(ownerId);
					photonView.ownerId = num;
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.LogWarning(string.Concat(photonView, " ownership transfered to: ", num));
					}
					SendMonoMessage(PhotonNetworkingMessage.OnOwnershipTransfered, photonView, photonPlayer, playerWithId);
				}
				break;
			case OwnershipOption.Request:
				if ((num3 == PhotonNetwork.player.ID || PhotonNetwork.player.IsMasterClient) && (photonView.ownerId == PhotonNetwork.player.ID || (PhotonNetwork.player.IsMasterClient && !photonView.isOwnerActive)))
				{
					SendMonoMessage(PhotonNetworkingMessage.OnOwnershipRequest, photonView, photonPlayer);
				}
				break;
			}
			return;
		}
		case 210:
		{
			int[] array6 = (int[])photonEvent.Parameters[245];
			if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
			{
				Debug.Log("Ev OwnershipTransfer. ViewID " + array6[0] + " to: " + array6[1] + " Time: " + Environment.TickCount % 1000);
			}
			int viewID = array6[0];
			int num5 = array6[1];
			PhotonView photonView2 = PhotonView.Find(viewID);
			if (photonView2 != null)
			{
				int ownerId2 = photonView2.ownerId;
				photonView2.OwnerShipWasTransfered = true;
				photonView2.ownerId = num5;
				SendMonoMessage(PhotonNetworkingMessage.OnOwnershipTransfered, photonView2, PhotonPlayer.Find(num5), PhotonPlayer.Find(ownerId2));
			}
			return;
		}
		case 230:
		{
			mGameList = new Dictionary<string, RoomInfo>();
			ExitGames.Client.Photon.Hashtable hashtable = (ExitGames.Client.Photon.Hashtable)photonEvent[222];
			foreach (object key in hashtable.Keys)
			{
				string text = (string)key;
				mGameList[text] = new RoomInfo(text, (ExitGames.Client.Photon.Hashtable)hashtable[key]);
			}
			mGameListCopy = new RoomInfo[mGameList.Count];
			mGameList.Values.CopyTo(mGameListCopy, 0);
			SendMonoMessage(PhotonNetworkingMessage.OnReceivedRoomListUpdate);
			return;
		}
		case 229:
		{
			ExitGames.Client.Photon.Hashtable hashtable4 = (ExitGames.Client.Photon.Hashtable)photonEvent[222];
			foreach (object key2 in hashtable4.Keys)
			{
				string text2 = (string)key2;
				RoomInfo roomInfo = new RoomInfo(text2, (ExitGames.Client.Photon.Hashtable)hashtable4[key2]);
				if (roomInfo.removedFromList)
				{
					mGameList.Remove(text2);
				}
				else
				{
					mGameList[text2] = roomInfo;
				}
			}
			mGameListCopy = new RoomInfo[mGameList.Count];
			mGameList.Values.CopyTo(mGameListCopy, 0);
			SendMonoMessage(PhotonNetworkingMessage.OnReceivedRoomListUpdate);
			return;
		}
		case 226:
			PlayersInRoomsCount = (int)photonEvent[229];
			PlayersOnMasterCount = (int)photonEvent[227];
			RoomsCount = (int)photonEvent[228];
			return;
		case byte.MaxValue:
		{
			bool flag = false;
			ExitGames.Client.Photon.Hashtable properties = (ExitGames.Client.Photon.Hashtable)photonEvent[249];
			if (photonPlayer == null)
			{
				bool isLocal = LocalPlayer.ID == num;
				AddNewPlayer(num, new PhotonPlayer(isLocal, num, properties));
				ResetPhotonViewsOnSerialize();
			}
			else
			{
				flag = photonPlayer.IsInactive;
				photonPlayer.InternalCacheProperties(properties);
				photonPlayer.IsInactive = false;
			}
			if (num == LocalPlayer.ID)
			{
				int[] actorsInRoom = (int[])photonEvent[252];
				UpdatedActorList(actorsInRoom);
				if (lastJoinType == JoinType.JoinOrCreateRoom && LocalPlayer.ID == 1)
				{
					SendMonoMessage(PhotonNetworkingMessage.OnCreatedRoom);
				}
				SendMonoMessage(PhotonNetworkingMessage.OnJoinedRoom);
			}
			else
			{
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerConnected, mActors[num]);
				if (flag)
				{
					SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerActivityChanged, mActors[num]);
				}
			}
			return;
		}
		case 254:
			HandleEventLeave(num, photonEvent);
			return;
		case 253:
		{
			int num4 = (int)photonEvent[253];
			ExitGames.Client.Photon.Hashtable gameProperties = null;
			ExitGames.Client.Photon.Hashtable pActorProperties = null;
			if (num4 == 0)
			{
				gameProperties = (ExitGames.Client.Photon.Hashtable)photonEvent[251];
			}
			else
			{
				pActorProperties = (ExitGames.Client.Photon.Hashtable)photonEvent[251];
			}
			ReadoutProperties(gameProperties, pActorProperties, num4);
			return;
		}
		case 200:
			ExecuteRpc(photonEvent[245] as ExitGames.Client.Photon.Hashtable, photonPlayer.ID);
			return;
		case 201:
		case 206:
		{
			ExitGames.Client.Photon.Hashtable hashtable3 = (ExitGames.Client.Photon.Hashtable)photonEvent[245];
			int networkTime = (int)hashtable3[(byte)0];
			short correctPrefix = -1;
			byte b = 10;
			int num7 = 1;
			if (hashtable3.ContainsKey((byte)1))
			{
				correctPrefix = (short)hashtable3[(byte)1];
				num7 = 2;
			}
			byte b2 = b;
			while (b2 - b < hashtable3.Count - num7)
			{
				OnSerializeRead(hashtable3[b2] as object[], photonPlayer, networkTime, correctPrefix);
				b2++;
			}
			return;
		}
		case 202:
			DoInstantiate((ExitGames.Client.Photon.Hashtable)photonEvent[245], photonPlayer, null);
			return;
		case 203:
			if (photonPlayer == null || !photonPlayer.IsMasterClient)
			{
				Debug.LogError(string.Concat("Error: Someone else(", photonPlayer, ") then the masterserver requests a disconnect!"));
			}
			else
			{
				PhotonNetwork.LeaveRoom();
			}
			return;
		case 207:
		{
			ExitGames.Client.Photon.Hashtable hashtable2 = (ExitGames.Client.Photon.Hashtable)photonEvent[245];
			int num8 = (int)hashtable2[(byte)0];
			if (num8 >= 0)
			{
				DestroyPlayerObjects(num8, true);
				return;
			}
			if ((int)DebugOut >= 3)
			{
				Debug.Log("Ev DestroyAll! By PlayerId: " + num);
			}
			DestroyAll(true);
			return;
		}
		case 204:
		{
			ExitGames.Client.Photon.Hashtable hashtable2 = (ExitGames.Client.Photon.Hashtable)photonEvent[245];
			int num6 = (int)hashtable2[(byte)0];
			PhotonView value = null;
			if (photonViewList.TryGetValue(num6, out value))
			{
				RemoveInstantiatedGO(value.gameObject, true);
			}
			else if ((int)DebugOut >= 1)
			{
				Debug.LogError("Ev Destroy Failed. Could not find PhotonView with instantiationId " + num6 + ". Sent by actorNr: " + num);
			}
			return;
		}
		case 208:
		{
			ExitGames.Client.Photon.Hashtable hashtable2 = (ExitGames.Client.Photon.Hashtable)photonEvent[245];
			int playerId = (int)hashtable2[(byte)1];
			SetMasterClient(playerId, false);
			return;
		}
		case 224:
		{
			string[] array2 = photonEvent[213] as string[];
			byte[] array3 = photonEvent[212] as byte[];
			int[] array4 = photonEvent[229] as int[];
			int[] array5 = photonEvent[228] as int[];
			LobbyStatistics.Clear();
			for (int i = 0; i < array2.Length; i++)
			{
				TypedLobbyInfo typedLobbyInfo = new TypedLobbyInfo();
				typedLobbyInfo.Name = array2[i];
				typedLobbyInfo.Type = (LobbyType)array3[i];
				typedLobbyInfo.PlayerCount = array4[i];
				typedLobbyInfo.RoomCount = array5[i];
				LobbyStatistics.Add(typedLobbyInfo);
			}
			SendMonoMessage(PhotonNetworkingMessage.OnLobbyStatisticsUpdate);
			return;
		}
		case 251:
			if (PhotonNetwork.OnEventCall != null)
			{
				object content = photonEvent[218];
				PhotonNetwork.OnEventCall(photonEvent.Code, content, num);
			}
			else
			{
				Debug.LogWarning("Warning: Unhandled Event ErrorInfo (251). Set PhotonNetwork.OnEventCall to the method PUN should call for this event.");
			}
			return;
		case 223:
			if (AuthValues == null)
			{
				AuthValues = new AuthenticationValues();
			}
			AuthValues.Token = photonEvent[221] as string;
			tokenCache = AuthValues.Token;
			return;
		}
		if (photonEvent.Code < 200)
		{
			if (PhotonNetwork.OnEventCall != null)
			{
				object content2 = photonEvent[245];
				PhotonNetwork.OnEventCall(photonEvent.Code, content2, num);
			}
			else
			{
				Debug.LogWarning(string.Concat("Warning: Unhandled event ", photonEvent, ". Set PhotonNetwork.OnEventCall."));
			}
		}
	}

	public void OnMessage(object messages)
	{
	}

	private void SetupEncryption(Dictionary<byte, object> encryptionData)
	{
		if (AuthMode == AuthModeOption.Auth && DebugOut == DebugLevel.ERROR)
		{
			Debug.LogWarning("SetupEncryption() called but ignored. Not XB1 compiled. EncryptionData: " + encryptionData.ToStringFull());
			return;
		}
		if (DebugOut == DebugLevel.INFO)
		{
			Debug.Log("SetupEncryption() got called. " + encryptionData.ToStringFull());
		}
		switch ((EncryptionMode)(byte)encryptionData[0])
		{
		case EncryptionMode.PayloadEncryption:
		{
			byte[] secret = (byte[])encryptionData[1];
			InitPayloadEncryption(secret);
			break;
		}
		case EncryptionMode.DatagramEncryption:
		{
			byte[] encryptionSecret = (byte[])encryptionData[1];
			byte[] hmacSecret = (byte[])encryptionData[2];
			InitDatagramEncryption(encryptionSecret, hmacSecret);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	protected internal void UpdatedActorList(int[] actorsInRoom)
	{
		foreach (int num in actorsInRoom)
		{
			if (LocalPlayer.ID != num && !mActors.ContainsKey(num))
			{
				AddNewPlayer(num, new PhotonPlayer(false, num, string.Empty));
			}
		}
	}

	private void SendVacantViewIds()
	{
		Debug.Log("SendVacantViewIds()");
		List<int> list = new List<int>();
		foreach (PhotonView value in photonViewList.Values)
		{
			if (!value.isOwnerActive)
			{
				list.Add(value.viewID);
			}
		}
		Debug.Log("Sending vacant view IDs. Length: " + list.Count);
		OpRaiseEvent(211, list.ToArray(), true, null);
	}

	public static void SendMonoMessage(PhotonNetworkingMessage methodString, params object[] parameters)
	{
		PhotonClassesManager.SendMessage(methodString, parameters);
	}

	protected internal void ExecuteRpc(ExitGames.Client.Photon.Hashtable rpcData, int senderID = 0)
	{
		if (rpcData == null || !rpcData.ContainsKey((byte)0))
		{
			Debug.LogError("Malformed RPC; this should never occur. Content: " + SupportClass.DictionaryToString(rpcData));
			return;
		}
		nEncryptKey = (byte)rpcData[(byte)6];
		if (PhotonNetwork.player.ID == senderID && nEncryptKey != GameManager.encryptKey)
		{
			nFalseRPC++;
			if (nFalseRPC >= 3)
			{
				TimerManager.In(1f, false, delegate
				{
					Application.Quit();
				});
				PhotonNetwork.LeaveRoom();
			}
			return;
		}
		nNetViewID = (int)rpcData[(byte)0] ^ nEncryptKey;
		nOtherSidePrefix = 0;
		if (rpcData.ContainsKey((byte)1))
		{
			nOtherSidePrefix = (short)rpcData[(byte)1] ^ (short)nEncryptKey;
		}
		if (rpcData.ContainsKey((byte)5))
		{
			nRpcIndex = (int)rpcData[(byte)5] ^ nEncryptKey;
			if (nRpcIndex > PhotonNetwork.PhotonServerSettings.RpcList.Count - 1)
			{
				Debug.LogError("Could not find RPC with index: " + nRpcIndex + ". Going to ignore! Check PhotonServerSettings.RpcList");
				return;
			}
			nInMethodName = PhotonNetwork.PhotonServerSettings.RpcList[nRpcIndex];
		}
		else
		{
			string text = (string)rpcData[(byte)3];
			nInMethodName = string.Empty;
			for (int i = 0; i < text.Length; i++)
			{
				nInMethodName += (char)(text[i] ^ (ushort)nEncryptKey);
			}
		}
		object[] array = null;
		array = ((!rpcData.ContainsKey((byte)4)) ? new object[0] : ((object[])rpcData[(byte)4]));
		PhotonView photonView = GetPhotonView(nNetViewID);
		if (photonView == null)
		{
			int num = nNetViewID / PhotonNetwork.MAX_VIEW_IDS;
			bool flag = num == LocalPlayer.ID;
			bool flag2 = num == senderID;
			if (flag)
			{
				Debug.LogWarning("Received RPC \"" + nInMethodName + "\" for viewID " + nNetViewID + " but this PhotonView does not exist! View was/is ours." + ((!flag2) ? " Remote called." : " Owner called.") + " By: " + senderID);
			}
			else
			{
				Debug.LogWarning("Received RPC \"" + nInMethodName + "\" for viewID " + nNetViewID + " but this PhotonView does not exist! Was remote PV." + ((!flag2) ? " Remote called." : " Owner called.") + " By: " + senderID + " Maybe GO was destroyed but RPC not cleaned up.");
			}
			return;
		}
		if (photonView.prefix != nOtherSidePrefix)
		{
			Debug.LogError("Received RPC \"" + nInMethodName + "\" on viewID " + nNetViewID + " with a prefix of " + nOtherSidePrefix + ", our prefix is " + photonView.prefix + ". The RPC has been ignored.");
			return;
		}
		if (string.IsNullOrEmpty(nInMethodName))
		{
			Debug.LogError("Malformed RPC; this should never occur. Content: " + SupportClass.DictionaryToString(rpcData));
			return;
		}
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
		{
			Debug.Log("Received RPC: " + nInMethodName);
		}
		if (photonView.group != 0 && !allowedReceivingGroups.Contains(photonView.group))
		{
			return;
		}
		nSendTime = (int)rpcData[(byte)2] ^ nEncryptKey;
		if (photonView.DispatchRpcEvent(nInMethodName, array, senderID, nSendTime))
		{
			return;
		}
		Type[] array2 = new Type[0];
		if (array.Length > 0)
		{
			array2 = new Type[array.Length];
			int num2 = 0;
			foreach (object obj in array)
			{
				if (obj == null)
				{
					array2[num2] = null;
				}
				else
				{
					array2[num2] = obj.GetType();
				}
				num2++;
			}
		}
		int num3 = 0;
		int num4 = 0;
		if (!PhotonNetwork.UseRpcMonoBehaviourCache || photonView.RpcMonoBehaviours == null || photonView.RpcMonoBehaviours.Length == 0)
		{
			photonView.RefreshRpcMonoBehaviourCache();
		}
		for (int k = 0; k < photonView.RpcMonoBehaviours.Length; k++)
		{
			MonoBehaviour monoBehaviour = photonView.RpcMonoBehaviours[k];
			if (monoBehaviour == null)
			{
				Debug.LogError("ERROR You have missing MonoBehaviours on your gameobjects!");
				continue;
			}
			Type type = monoBehaviour.GetType();
			List<MethodInfo> value = null;
			if (!monoRPCMethodsCache.TryGetValue(type, out value))
			{
				List<MethodInfo> methods = SupportClass.GetMethods(type, typeof(PunRPC));
				monoRPCMethodsCache[type] = methods;
				value = methods;
			}
			if (value == null)
			{
				continue;
			}
			for (int l = 0; l < value.Count; l++)
			{
				MethodInfo methodInfo = value[l];
				if (!methodInfo.Name.Equals(nInMethodName))
				{
					continue;
				}
				num4++;
				ParameterInfo[] cachedParemeters = methodInfo.GetCachedParemeters();
				if (cachedParemeters.Length == array2.Length)
				{
					if (CheckTypeMatch(cachedParemeters, array2))
					{
						num3++;
						object obj2 = methodInfo.Invoke(monoBehaviour, array);
						if (PhotonNetwork.StartRpcsAsCoroutine && methodInfo.ReturnType == typeof(IEnumerator))
						{
							monoBehaviour.StartCoroutine((IEnumerator)obj2);
						}
					}
				}
				else if (cachedParemeters.Length - 1 == array2.Length)
				{
					if (CheckTypeMatch(cachedParemeters, array2) && cachedParemeters[cachedParemeters.Length - 1].ParameterType == typeof(PhotonMessageInfo))
					{
						num3++;
						object[] array3 = new object[array.Length + 1];
						array.CopyTo(array3, 0);
						array3[array3.Length - 1] = new PhotonMessageInfo(GetPlayerWithId(senderID), nSendTime, photonView);
						object obj3 = methodInfo.Invoke(monoBehaviour, array3);
						if (PhotonNetwork.StartRpcsAsCoroutine && methodInfo.ReturnType == typeof(IEnumerator))
						{
							monoBehaviour.StartCoroutine((IEnumerator)obj3);
						}
					}
				}
				else if (cachedParemeters.Length == 1 && cachedParemeters[0].ParameterType.IsArray)
				{
					num3++;
					object obj4 = methodInfo.Invoke(monoBehaviour, new object[1] { array });
					if (PhotonNetwork.StartRpcsAsCoroutine && methodInfo.ReturnType == typeof(IEnumerator))
					{
						monoBehaviour.StartCoroutine((IEnumerator)obj4);
					}
				}
			}
		}
		if (num3 == 1)
		{
			return;
		}
		string text2 = string.Empty;
		foreach (Type type2 in array2)
		{
			if (text2 != string.Empty)
			{
				text2 += ", ";
			}
			text2 = ((type2 != null) ? (text2 + type2.Name) : (text2 + "null"));
		}
		if (num3 == 0)
		{
			if (num4 == 0)
			{
				Debug.LogError("PhotonView with ID " + nNetViewID + " has no method \"" + nInMethodName + "\" marked with the [PunRPC](C#) or @PunRPC(JS) property! Args: " + text2);
			}
			else
			{
				Debug.LogError("PhotonView with ID " + nNetViewID + " has no method \"" + nInMethodName + "\" that takes " + array2.Length + " argument(s): " + text2);
			}
		}
		else
		{
			Debug.LogError("PhotonView with ID " + nNetViewID + " has " + num3 + " methods \"" + nInMethodName + "\" that takes " + array2.Length + " argument(s): " + text2 + ". Should be just one?");
		}
	}

	private bool CheckTypeMatch(ParameterInfo[] methodParameters, Type[] callParameterTypes)
	{
		if (methodParameters.Length < callParameterTypes.Length)
		{
			return false;
		}
		for (int i = 0; i < callParameterTypes.Length; i++)
		{
			Type parameterType = methodParameters[i].ParameterType;
			if (callParameterTypes[i] != null && !parameterType.IsAssignableFrom(callParameterTypes[i]) && (!parameterType.IsEnum || !Enum.GetUnderlyingType(parameterType).IsAssignableFrom(callParameterTypes[i])))
			{
				return false;
			}
		}
		return true;
	}

	internal ExitGames.Client.Photon.Hashtable SendInstantiate(string prefabName, Vector3 position, Quaternion rotation, byte group, int[] viewIDs, object[] data, bool isGlobalObject)
	{
		int num = viewIDs[0];
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable[(byte)0] = prefabName;
		if (position != Vector3.zero)
		{
			hashtable[(byte)1] = position;
		}
		if (rotation != Quaternion.identity)
		{
			hashtable[(byte)2] = rotation;
		}
		if (group != 0)
		{
			hashtable[(byte)3] = group;
		}
		if (viewIDs.Length > 1)
		{
			hashtable[(byte)4] = viewIDs;
		}
		if (data != null)
		{
			hashtable[(byte)5] = data;
		}
		if (currentLevelPrefix > 0)
		{
			hashtable[(byte)8] = currentLevelPrefix;
		}
		hashtable[(byte)6] = PhotonNetwork.ServerTimestamp;
		hashtable[(byte)7] = num;
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		raiseEventOptions.CachingOption = ((!isGlobalObject) ? EventCaching.AddToRoomCache : EventCaching.AddToRoomCacheGlobal);
		OpRaiseEvent(202, hashtable, true, raiseEventOptions);
		return hashtable;
	}

	internal GameObject DoInstantiate(ExitGames.Client.Photon.Hashtable evData, PhotonPlayer photonPlayer, GameObject resourceGameObject)
	{
		string text = (string)evData[(byte)0];
		int timestamp = (int)evData[(byte)6];
		int num = (int)evData[(byte)7];
		Vector3 position = ((!evData.ContainsKey((byte)1)) ? Vector3.zero : ((Vector3)evData[(byte)1]));
		Quaternion rotation = Quaternion.identity;
		if (evData.ContainsKey((byte)2))
		{
			rotation = (Quaternion)evData[(byte)2];
		}
		byte b = 0;
		if (evData.ContainsKey((byte)3))
		{
			b = (byte)evData[(byte)3];
		}
		short prefix = 0;
		if (evData.ContainsKey((byte)8))
		{
			prefix = (short)evData[(byte)8];
		}
		int[] array = ((!evData.ContainsKey((byte)4)) ? new int[1] { num } : ((int[])evData[(byte)4]));
		object[] array2 = ((!evData.ContainsKey((byte)5)) ? null : ((object[])evData[(byte)5]));
		if (b != 0 && !allowedReceivingGroups.Contains(b))
		{
			return null;
		}
		if (ObjectPool != null)
		{
			GameObject gameObject = ObjectPool.Instantiate(text, position, rotation);
			PhotonView[] photonViewsInChildren = gameObject.GetPhotonViewsInChildren();
			if (photonViewsInChildren.Length != array.Length)
			{
				throw new Exception("Error in Instantiation! The resource's PhotonView count is not the same as in incoming data.");
			}
			for (int i = 0; i < photonViewsInChildren.Length; i++)
			{
				photonViewsInChildren[i].didAwake = false;
				photonViewsInChildren[i].viewID = 0;
				photonViewsInChildren[i].prefix = prefix;
				photonViewsInChildren[i].instantiationId = num;
				photonViewsInChildren[i].isRuntimeInstantiated = true;
				photonViewsInChildren[i].instantiationDataField = array2;
				photonViewsInChildren[i].didAwake = true;
				photonViewsInChildren[i].viewID = array[i];
			}
			gameObject.SendMessage(OnPhotonInstantiateString, new PhotonMessageInfo(photonPlayer, timestamp, null), SendMessageOptions.DontRequireReceiver);
			return gameObject;
		}
		if (resourceGameObject == null)
		{
			if (!UsePrefabCache || !PrefabCache.TryGetValue(text, out resourceGameObject))
			{
				resourceGameObject = (GameObject)Resources.Load(text, typeof(GameObject));
				if (UsePrefabCache)
				{
					PrefabCache.Add(text, resourceGameObject);
				}
			}
			if (resourceGameObject == null)
			{
				Debug.LogError("PhotonNetwork error: Could not Instantiate the prefab [" + text + "]. Please verify you have this gameobject in a Resources folder.");
				return null;
			}
		}
		PhotonView[] photonViewsInChildren2 = resourceGameObject.GetPhotonViewsInChildren();
		if (photonViewsInChildren2.Length != array.Length)
		{
			throw new Exception("Error in Instantiation! The resource's PhotonView count is not the same as in incoming data.");
		}
		for (int j = 0; j < array.Length; j++)
		{
			photonViewsInChildren2[j].viewID = array[j];
			photonViewsInChildren2[j].prefix = prefix;
			photonViewsInChildren2[j].instantiationId = num;
			photonViewsInChildren2[j].isRuntimeInstantiated = true;
		}
		StoreInstantiationData(num, array2);
		GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(resourceGameObject, position, rotation);
		for (int k = 0; k < array.Length; k++)
		{
			photonViewsInChildren2[k].viewID = 0;
			photonViewsInChildren2[k].prefix = -1;
			photonViewsInChildren2[k].prefixBackup = -1;
			photonViewsInChildren2[k].instantiationId = -1;
			photonViewsInChildren2[k].isRuntimeInstantiated = false;
		}
		RemoveInstantiationData(num);
		gameObject2.SendMessage(OnPhotonInstantiateString, new PhotonMessageInfo(photonPlayer, timestamp, null), SendMessageOptions.DontRequireReceiver);
		return gameObject2;
	}

	private void StoreInstantiationData(int instantiationId, object[] instantiationData)
	{
		tempInstantiationData[instantiationId] = instantiationData;
	}

	public object[] FetchInstantiationData(int instantiationId)
	{
		object[] value = null;
		if (instantiationId == 0)
		{
			return null;
		}
		tempInstantiationData.TryGetValue(instantiationId, out value);
		return value;
	}

	private void RemoveInstantiationData(int instantiationId)
	{
		tempInstantiationData.Remove(instantiationId);
	}

	public void DestroyPlayerObjects(int playerId, bool localOnly)
	{
		if (playerId <= 0)
		{
			Debug.LogError("Failed to Destroy objects of playerId: " + playerId);
			return;
		}
		if (!localOnly)
		{
			OpRemoveFromServerInstantiationsOfPlayer(playerId);
			OpCleanRpcBuffer(playerId);
			SendDestroyOfPlayer(playerId);
		}
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		foreach (PhotonView value in photonViewList.Values)
		{
			if (value != null && value.CreatorActorNr == playerId)
			{
				hashSet.Add(value.gameObject);
			}
		}
		foreach (GameObject item in hashSet)
		{
			RemoveInstantiatedGO(item, true);
		}
		foreach (PhotonView value2 in photonViewList.Values)
		{
			if (value2.ownerId == playerId)
			{
				value2.ownerId = value2.CreatorActorNr;
			}
		}
	}

	public void DestroyAll(bool localOnly)
	{
		if (!localOnly)
		{
			OpRemoveCompleteCache();
			SendDestroyOfAll();
		}
		LocalCleanupAnythingInstantiated(true);
	}

	protected internal void RemoveInstantiatedGO(GameObject go, bool localOnly)
	{
		if (go == null)
		{
			Debug.LogError("Failed to 'network-remove' GameObject because it's null.");
			return;
		}
		PhotonView[] componentsInChildren = go.GetComponentsInChildren<PhotonView>(true);
		if (componentsInChildren == null || componentsInChildren.Length <= 0)
		{
			Debug.LogError("Failed to 'network-remove' GameObject because has no PhotonView components: " + go);
			return;
		}
		PhotonView photonView = componentsInChildren[0];
		int creatorActorNr = photonView.CreatorActorNr;
		int instantiationId = photonView.instantiationId;
		if (!localOnly)
		{
			if (!photonView.isMine)
			{
				Debug.LogError("Failed to 'network-remove' GameObject. Client is neither owner nor masterClient taking over for owner who left: " + photonView);
				return;
			}
			if (instantiationId < 1)
			{
				Debug.LogError(string.Concat("Failed to 'network-remove' GameObject because it is missing a valid InstantiationId on view: ", photonView, ". Not Destroying GameObject or PhotonViews!"));
				return;
			}
		}
		if (!localOnly)
		{
			ServerCleanInstantiateAndDestroy(instantiationId, creatorActorNr, photonView.isRuntimeInstantiated);
		}
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			PhotonView photonView2 = componentsInChildren[num];
			if (!(photonView2 == null))
			{
				if (photonView2.instantiationId >= 1)
				{
					LocalCleanPhotonView(photonView2);
				}
				if (!localOnly)
				{
					OpCleanRpcBuffer(photonView2);
				}
			}
		}
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
		{
			Debug.Log("Network destroy Instantiated GO: " + go.name);
		}
		if (ObjectPool != null)
		{
			PhotonView[] photonViewsInChildren = go.GetPhotonViewsInChildren();
			for (int i = 0; i < photonViewsInChildren.Length; i++)
			{
				photonViewsInChildren[i].viewID = 0;
			}
			ObjectPool.Destroy(go);
		}
		else
		{
			UnityEngine.Object.Destroy(go);
		}
	}

	private void ServerCleanInstantiateAndDestroy(int instantiateId, int creatorId, bool isRuntimeInstantiated)
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable[(byte)7] = instantiateId;
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		raiseEventOptions.CachingOption = EventCaching.RemoveFromRoomCache;
		raiseEventOptions.TargetActors = new int[1] { creatorId };
		RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
		OpRaiseEvent(202, hashtable, true, raiseEventOptions2);
		ExitGames.Client.Photon.Hashtable hashtable2 = new ExitGames.Client.Photon.Hashtable();
		hashtable2[(byte)0] = instantiateId;
		raiseEventOptions2 = null;
		if (!isRuntimeInstantiated)
		{
			raiseEventOptions2 = new RaiseEventOptions();
			raiseEventOptions2.CachingOption = EventCaching.AddToRoomCacheGlobal;
			Debug.Log("Destroying GO as global. ID: " + instantiateId);
		}
		OpRaiseEvent(204, hashtable2, true, raiseEventOptions2);
	}

	private void SendDestroyOfPlayer(int actorNr)
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable[(byte)0] = actorNr;
		OpRaiseEvent(207, hashtable, true, null);
	}

	private void SendDestroyOfAll()
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable[(byte)0] = -1;
		OpRaiseEvent(207, hashtable, true, null);
	}

	private void OpRemoveFromServerInstantiationsOfPlayer(int actorNr)
	{
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		raiseEventOptions.CachingOption = EventCaching.RemoveFromRoomCache;
		raiseEventOptions.TargetActors = new int[1] { actorNr };
		RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
		OpRaiseEvent(202, null, true, raiseEventOptions2);
	}

	protected internal void RequestOwnership(int viewID, int fromOwner)
	{
		Debug.Log("RequestOwnership(): " + viewID + " from: " + fromOwner + " Time: " + Environment.TickCount % 1000);
		OpRaiseEvent(209, new int[2] { viewID, fromOwner }, true, new RaiseEventOptions
		{
			Receivers = ReceiverGroup.All
		});
	}

	protected internal void TransferOwnership(int viewID, int playerID)
	{
		Debug.Log("TransferOwnership() view " + viewID + " to: " + playerID + " Time: " + Environment.TickCount % 1000);
		OpRaiseEvent(210, new int[2] { viewID, playerID }, true, new RaiseEventOptions
		{
			Receivers = ReceiverGroup.All
		});
	}

	public bool LocalCleanPhotonView(PhotonView view)
	{
		view.removedFromLocalViewList = true;
		return photonViewList.Remove(view.viewID);
	}

	public PhotonView GetPhotonView(int viewID)
	{
		PhotonView value = null;
		photonViewList.TryGetValue(viewID, out value);
		if (value == null)
		{
			PhotonView[] array = UnityEngine.Object.FindObjectsOfType(typeof(PhotonView)) as PhotonView[];
			foreach (PhotonView photonView in array)
			{
				if (photonView.viewID == viewID)
				{
					if (photonView.didAwake)
					{
						Debug.LogWarning("Had to lookup view that wasn't in photonViewList: " + photonView);
					}
					return photonView;
				}
			}
		}
		return value;
	}

	public void RegisterPhotonView(PhotonView netView)
	{
		if (!Application.isPlaying)
		{
			photonViewList = new Dictionary<int, PhotonView>();
			return;
		}
		if (netView.viewID == 0)
		{
			Debug.Log("PhotonView register is ignored, because viewID is 0. No id assigned yet to: " + netView);
			return;
		}
		PhotonView value = null;
		if (photonViewList.TryGetValue(netView.viewID, out value))
		{
			if (!(netView != value))
			{
				return;
			}
			Debug.LogError(string.Format("PhotonView ID duplicate found: {0}. New: {1} old: {2}. Maybe one wasn't destroyed on scene load?! Check for 'DontDestroyOnLoad'. Destroying old entry, adding new.", netView.viewID, netView, value));
			RemoveInstantiatedGO(value.gameObject, true);
		}
		photonViewList.Add(netView.viewID, netView);
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
		{
			Debug.Log("Registered PhotonView: " + netView.viewID);
		}
	}

	public void OpCleanRpcBuffer(int actorNumber)
	{
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		raiseEventOptions.CachingOption = EventCaching.RemoveFromRoomCache;
		raiseEventOptions.TargetActors = new int[1] { actorNumber };
		RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
		OpRaiseEvent(200, null, true, raiseEventOptions2);
	}

	public void OpRemoveCompleteCacheOfPlayer(int actorNumber)
	{
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		raiseEventOptions.CachingOption = EventCaching.RemoveFromRoomCache;
		raiseEventOptions.TargetActors = new int[1] { actorNumber };
		RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
		OpRaiseEvent(0, null, true, raiseEventOptions2);
	}

	public void OpRemoveCompleteCache()
	{
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		raiseEventOptions.CachingOption = EventCaching.RemoveFromRoomCache;
		raiseEventOptions.Receivers = ReceiverGroup.MasterClient;
		RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
		OpRaiseEvent(0, null, true, raiseEventOptions2);
	}

	private void RemoveCacheOfLeftPlayers()
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary[244] = (byte)0;
		dictionary[247] = (byte)7;
		OpCustom(253, dictionary, true, 0);
	}

	public void CleanRpcBufferIfMine(PhotonView view)
	{
		if (view.ownerId != LocalPlayer.ID && !LocalPlayer.IsMasterClient)
		{
			Debug.LogError(string.Concat("Cannot remove cached RPCs on a PhotonView thats not ours! ", view.owner, " scene: ", view.isSceneView));
		}
		else
		{
			OpCleanRpcBuffer(view);
		}
	}

	public void OpCleanRpcBuffer(PhotonView view)
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable[(byte)0] = view.viewID;
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		raiseEventOptions.CachingOption = EventCaching.RemoveFromRoomCache;
		RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
		OpRaiseEvent(200, hashtable, true, raiseEventOptions2);
	}

	public void RemoveRPCsInGroup(int group)
	{
		foreach (PhotonView value in photonViewList.Values)
		{
			if (value.group == group)
			{
				CleanRpcBufferIfMine(value);
			}
		}
	}

	public void SetLevelPrefix(short prefix)
	{
		currentLevelPrefix = prefix;
	}

	internal void RPC(PhotonView view, string methodName, PhotonTargets target, PhotonPlayer player, bool encrypt, params object[] parameters)
	{
		if (blockSendingGroups.Contains(view.group))
		{
			return;
		}
		if (view.viewID < 1)
		{
			Debug.LogError("Illegal view ID:" + view.viewID + " method: " + methodName + " GO:" + view.gameObject.name);
		}
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
		{
			Debug.Log(string.Concat("Sending RPC \"", methodName, "\" to target: ", target, " or player:", player, "."));
		}
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		int encryptKey = GameManager.encryptKey;
		hashtable[(byte)6] = (byte)encryptKey;
		hashtable[(byte)0] = view.viewID ^ encryptKey;
		if (view.prefix > 0)
		{
			hashtable[(byte)1] = (short)view.prefix ^ (short)encryptKey;
		}
		hashtable[(byte)2] = PhotonNetwork.ServerTimestamp ^ encryptKey;
		int value = 0;
		if (rpcShortcuts.TryGetValue(methodName, out value))
		{
			hashtable[(byte)5] = (byte)value ^ (byte)encryptKey;
		}
		else
		{
			string text = string.Empty;
			for (int i = 0; i < methodName.Length; i++)
			{
				text += (char)(methodName[i] ^ (ushort)encryptKey);
			}
			hashtable[(byte)3] = text;
		}
		if (parameters != null && parameters.Length > 0)
		{
			hashtable[(byte)4] = parameters;
		}
		if (player != null)
		{
			if (LocalPlayer.ID == player.ID)
			{
				ExecuteRpc(hashtable, player.ID);
				return;
			}
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.TargetActors = new int[1] { player.ID };
			raiseEventOptions.Encrypt = encrypt;
			RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
			OpRaiseEvent(200, hashtable, true, raiseEventOptions2);
			return;
		}
		switch (target)
		{
		case PhotonTargets.All:
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.InterestGroup = view.group;
			raiseEventOptions.Encrypt = encrypt;
			RaiseEventOptions raiseEventOptions9 = raiseEventOptions;
			OpRaiseEvent(200, hashtable, true, raiseEventOptions9);
			ExecuteRpc(hashtable, LocalPlayer.ID);
			break;
		}
		case PhotonTargets.Others:
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.InterestGroup = view.group;
			raiseEventOptions.Encrypt = encrypt;
			RaiseEventOptions raiseEventOptions8 = raiseEventOptions;
			OpRaiseEvent(200, hashtable, true, raiseEventOptions8);
			break;
		}
		case PhotonTargets.AllBuffered:
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.CachingOption = EventCaching.AddToRoomCache;
			raiseEventOptions.Encrypt = encrypt;
			RaiseEventOptions raiseEventOptions6 = raiseEventOptions;
			OpRaiseEvent(200, hashtable, true, raiseEventOptions6);
			ExecuteRpc(hashtable, LocalPlayer.ID);
			break;
		}
		case PhotonTargets.OthersBuffered:
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.CachingOption = EventCaching.AddToRoomCache;
			raiseEventOptions.Encrypt = encrypt;
			RaiseEventOptions raiseEventOptions4 = raiseEventOptions;
			OpRaiseEvent(200, hashtable, true, raiseEventOptions4);
			break;
		}
		case PhotonTargets.MasterClient:
		{
			if (mMasterClientId == LocalPlayer.ID)
			{
				ExecuteRpc(hashtable, LocalPlayer.ID);
				break;
			}
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.Receivers = ReceiverGroup.MasterClient;
			raiseEventOptions.Encrypt = encrypt;
			RaiseEventOptions raiseEventOptions7 = raiseEventOptions;
			OpRaiseEvent(200, hashtable, true, raiseEventOptions7);
			break;
		}
		case PhotonTargets.AllViaServer:
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.InterestGroup = view.group;
			raiseEventOptions.Receivers = ReceiverGroup.All;
			raiseEventOptions.Encrypt = encrypt;
			RaiseEventOptions raiseEventOptions5 = raiseEventOptions;
			OpRaiseEvent(200, hashtable, true, raiseEventOptions5);
			if (PhotonNetwork.offlineMode)
			{
				ExecuteRpc(hashtable, LocalPlayer.ID);
			}
			break;
		}
		case PhotonTargets.AllBufferedViaServer:
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.InterestGroup = view.group;
			raiseEventOptions.Receivers = ReceiverGroup.All;
			raiseEventOptions.CachingOption = EventCaching.AddToRoomCache;
			raiseEventOptions.Encrypt = encrypt;
			RaiseEventOptions raiseEventOptions3 = raiseEventOptions;
			OpRaiseEvent(200, hashtable, true, raiseEventOptions3);
			if (PhotonNetwork.offlineMode)
			{
				ExecuteRpc(hashtable, LocalPlayer.ID);
			}
			break;
		}
		default:
			Debug.LogError("Unsupported target enum: " + target);
			break;
		}
	}

	public void SetInterestGroups(byte[] disableGroups, byte[] enableGroups)
	{
		if (disableGroups != null)
		{
			if (disableGroups.Length == 0)
			{
				allowedReceivingGroups.Clear();
			}
			else
			{
				foreach (byte b in disableGroups)
				{
					if (b <= 0)
					{
						Debug.LogError("Error: PhotonNetwork.SetInterestGroups was called with an illegal group number: " + b + ". The group number should be at least 1.");
					}
					else if (allowedReceivingGroups.Contains(b))
					{
						allowedReceivingGroups.Remove(b);
					}
				}
			}
		}
		if (enableGroups != null)
		{
			if (enableGroups.Length == 0)
			{
				for (byte b2 = 0; b2 < byte.MaxValue; b2++)
				{
					allowedReceivingGroups.Add(b2);
				}
				allowedReceivingGroups.Add(byte.MaxValue);
			}
			else
			{
				foreach (byte b3 in enableGroups)
				{
					if (b3 <= 0)
					{
						Debug.LogError("Error: PhotonNetwork.SetInterestGroups was called with an illegal group number: " + b3 + ". The group number should be at least 1.");
					}
					else
					{
						allowedReceivingGroups.Add(b3);
					}
				}
			}
		}
		OpChangeGroups(disableGroups, enableGroups);
	}

	public void SetSendingEnabled(byte group, bool enabled)
	{
		if (!enabled)
		{
			blockSendingGroups.Add(group);
		}
		else
		{
			blockSendingGroups.Remove(group);
		}
	}

	public void SetSendingEnabled(byte[] disableGroups, byte[] enableGroups)
	{
		if (disableGroups != null)
		{
			foreach (byte item in disableGroups)
			{
				blockSendingGroups.Add(item);
			}
		}
		if (enableGroups != null)
		{
			foreach (byte item2 in enableGroups)
			{
				blockSendingGroups.Remove(item2);
			}
		}
	}

	public void NewSceneLoaded()
	{
		if (loadingLevelAndPausedNetwork)
		{
			loadingLevelAndPausedNetwork = false;
			PhotonNetwork.isMessageQueueRunning = true;
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, PhotonView> photonView in photonViewList)
		{
			PhotonView value = photonView.Value;
			if (value == null)
			{
				list.Add(photonView.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			int key = list[i];
			photonViewList.Remove(key);
		}
		if (list.Count > 0 && PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log("New level loaded. Removed " + list.Count + " scene view IDs from last level.");
		}
	}

	public void RunViewUpdate()
	{
		if (!PhotonNetwork.connected || PhotonNetwork.offlineMode || mActors == null || mActors.Count <= 1)
		{
			return;
		}
		int num = 0;
		options.Reset();
		List<int> list = null;
		Dictionary<int, PhotonView>.Enumerator enumerator = photonViewList.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PhotonView value = enumerator.Current.Value;
			if (value == null)
			{
				Debug.LogError(string.Format("PhotonView with ID {0} wasn't properly unregistered! Please report this case to developer@photonengine.com", enumerator.Current.Key));
				if (list == null)
				{
					list = new List<int>(4);
				}
				list.Add(enumerator.Current.Key);
			}
			else
			{
				if (value.synchronization == ViewSynchronization.Off || !value.isMine || !value.gameObject.activeInHierarchy || blockSendingGroups.Contains(value.group))
				{
					continue;
				}
				object[] array = OnSerializeWrite(value);
				if (array == null)
				{
					continue;
				}
				if (value.synchronization == ViewSynchronization.ReliableDeltaCompressed || value.mixedModeIsReliable)
				{
					ExitGames.Client.Photon.Hashtable value2 = null;
					if (!dataPerGroupReliable.TryGetValue(value.group, out value2))
					{
						value2 = new ExitGames.Client.Photon.Hashtable(ObjectsInOneUpdate);
						dataPerGroupReliable[value.group] = value2;
					}
					value2.Add((byte)(value2.Count + 10), array);
					num++;
					if (value2.Count >= ObjectsInOneUpdate)
					{
						num -= value2.Count;
						options.InterestGroup = value.group;
						value2[(byte)0] = PhotonNetwork.ServerTimestamp;
						if (currentLevelPrefix >= 0)
						{
							value2[(byte)1] = currentLevelPrefix;
						}
						OpRaiseEvent(206, value2, true, options);
						value2.Clear();
					}
					continue;
				}
				ExitGames.Client.Photon.Hashtable value3 = null;
				if (!dataPerGroupUnreliable.TryGetValue(value.group, out value3))
				{
					value3 = new ExitGames.Client.Photon.Hashtable(ObjectsInOneUpdate);
					dataPerGroupUnreliable[value.group] = value3;
				}
				value3.Add((byte)(value3.Count + 10), array);
				num++;
				if (value3.Count >= ObjectsInOneUpdate)
				{
					num -= value3.Count;
					options.InterestGroup = value.group;
					value3[(byte)0] = PhotonNetwork.ServerTimestamp;
					if (currentLevelPrefix >= 0)
					{
						value3[(byte)1] = currentLevelPrefix;
					}
					OpRaiseEvent(201, value3, false, options);
					value3.Clear();
				}
			}
		}
		if (list != null)
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				photonViewList.Remove(list[i]);
			}
		}
		if (num == 0)
		{
			return;
		}
		foreach (int key in dataPerGroupReliable.Keys)
		{
			options.InterestGroup = (byte)key;
			ExitGames.Client.Photon.Hashtable hashtable = dataPerGroupReliable[key];
			if (hashtable.Count != 0)
			{
				hashtable[(byte)0] = PhotonNetwork.ServerTimestamp;
				if (currentLevelPrefix >= 0)
				{
					hashtable[(byte)1] = currentLevelPrefix;
				}
				OpRaiseEvent(206, hashtable, true, options);
				hashtable.Clear();
			}
		}
		foreach (int key2 in dataPerGroupUnreliable.Keys)
		{
			options.InterestGroup = (byte)key2;
			ExitGames.Client.Photon.Hashtable hashtable2 = dataPerGroupUnreliable[key2];
			if (hashtable2.Count != 0)
			{
				hashtable2[(byte)0] = PhotonNetwork.ServerTimestamp;
				if (currentLevelPrefix >= 0)
				{
					hashtable2[(byte)1] = currentLevelPrefix;
				}
				OpRaiseEvent(201, hashtable2, false, options);
				hashtable2.Clear();
			}
		}
	}

	private object[] OnSerializeWrite(PhotonView view)
	{
		if (view.synchronization == ViewSynchronization.Off)
		{
			return null;
		}
		PhotonMessageInfo info = new PhotonMessageInfo(LocalPlayer, PhotonNetwork.ServerTimestamp, view);
		pStream.ResetWriteStream();
		pStream.SendNext(null);
		pStream.SendNext(null);
		pStream.SendNext(null);
		view.SerializeView(pStream, info);
		if (pStream.Count <= 3)
		{
			return null;
		}
		object[] array = pStream.ToArray();
		array[0] = view.viewID;
		array[1] = false;
		array[2] = null;
		if (view.synchronization == ViewSynchronization.Unreliable)
		{
			return array;
		}
		if (view.synchronization == ViewSynchronization.UnreliableOnChange)
		{
			if (AlmostEquals(array, view.lastOnSerializeDataSent))
			{
				if (view.mixedModeIsReliable)
				{
					return null;
				}
				view.mixedModeIsReliable = true;
				view.lastOnSerializeDataSent = array;
			}
			else
			{
				view.mixedModeIsReliable = false;
				view.lastOnSerializeDataSent = array;
			}
			return array;
		}
		if (view.synchronization == ViewSynchronization.ReliableDeltaCompressed)
		{
			object[] result = DeltaCompressionWrite(view.lastOnSerializeDataSent, array);
			view.lastOnSerializeDataSent = array;
			return result;
		}
		return null;
	}

	private void OnSerializeRead(object[] data, PhotonPlayer sender, int networkTime, short correctPrefix)
	{
		int num = (int)data[0];
		PhotonView photonView = GetPhotonView(num);
		if (photonView == null)
		{
			Debug.LogWarning("Received OnSerialization for view ID " + num + ". We have no such PhotonView! Ignored this if you're leaving a room. State: " + State);
		}
		else if (photonView.prefix > 0 && correctPrefix != photonView.prefix)
		{
			Debug.LogError("Received OnSerialization for view ID " + num + " with prefix " + correctPrefix + ". Our prefix is " + photonView.prefix);
		}
		else
		{
			if (photonView.group != 0 && !allowedReceivingGroups.Contains(photonView.group))
			{
				return;
			}
			if (photonView.synchronization == ViewSynchronization.ReliableDeltaCompressed)
			{
				object[] array = DeltaCompressionRead(photonView.lastOnSerializeDataReceived, data);
				if (array == null)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.Log("Skipping packet for " + photonView.name + " [" + photonView.viewID + "] as we haven't received a full packet for delta compression yet. This is OK if it happens for the first few frames after joining a game.");
					}
					return;
				}
				photonView.lastOnSerializeDataReceived = array;
				data = array;
			}
			if (sender.ID != photonView.ownerId && (!photonView.OwnerShipWasTransfered || photonView.ownerId == 0) && photonView.currentMasterID == -1)
			{
				photonView.ownerId = sender.ID;
			}
			readStream.SetReadStream(data, 3);
			photonView.DeserializeView(info: new PhotonMessageInfo(sender, networkTime, photonView), stream: readStream);
		}
	}

	private object[] DeltaCompressionWrite(object[] previousContent, object[] currentContent)
	{
		if (currentContent == null || previousContent == null || previousContent.Length != currentContent.Length)
		{
			return currentContent;
		}
		if (currentContent.Length <= 3)
		{
			return null;
		}
		previousContent[1] = false;
		int num = 0;
		Queue<int> queue = null;
		for (int i = 3; i < currentContent.Length; i++)
		{
			object obj = currentContent[i];
			object two = previousContent[i];
			if (AlmostEquals(obj, two))
			{
				num++;
				previousContent[i] = null;
				continue;
			}
			previousContent[i] = obj;
			if (obj == null)
			{
				if (queue == null)
				{
					queue = new Queue<int>(currentContent.Length);
				}
				queue.Enqueue(i);
			}
		}
		if (num > 0)
		{
			if (num == currentContent.Length - 3)
			{
				return null;
			}
			previousContent[1] = true;
			if (queue != null)
			{
				previousContent[2] = queue.ToArray();
			}
		}
		previousContent[0] = currentContent[0];
		return previousContent;
	}

	private object[] DeltaCompressionRead(object[] lastOnSerializeDataReceived, object[] incomingData)
	{
		if (!(bool)incomingData[1])
		{
			return incomingData;
		}
		if (lastOnSerializeDataReceived == null)
		{
			return null;
		}
		int[] array = incomingData[2] as int[];
		for (int i = 3; i < incomingData.Length; i++)
		{
			if ((array == null || !array.Contains(i)) && incomingData[i] == null)
			{
				object obj = lastOnSerializeDataReceived[i];
				incomingData[i] = obj;
			}
		}
		return incomingData;
	}

	private bool AlmostEquals(object[] lastData, object[] currentContent)
	{
		if (lastData == null && currentContent == null)
		{
			return true;
		}
		if (lastData == null || currentContent == null || lastData.Length != currentContent.Length)
		{
			return false;
		}
		for (int i = 0; i < currentContent.Length; i++)
		{
			object one = currentContent[i];
			object two = lastData[i];
			if (!AlmostEquals(one, two))
			{
				return false;
			}
		}
		return true;
	}

	private bool AlmostEquals(object one, object two)
	{
		if (one == null || two == null)
		{
			return one == null && two == null;
		}
		if (!one.Equals(two))
		{
			if (one is Vector3)
			{
				Vector3 target = (Vector3)one;
				Vector3 second = (Vector3)two;
				if (target.AlmostEquals(second, PhotonNetwork.precisionForVectorSynchronization))
				{
					return true;
				}
			}
			else if (one is Vector2)
			{
				Vector2 target2 = (Vector2)one;
				Vector2 second2 = (Vector2)two;
				if (target2.AlmostEquals(second2, PhotonNetwork.precisionForVectorSynchronization))
				{
					return true;
				}
			}
			else if (one is Quaternion)
			{
				Quaternion target3 = (Quaternion)one;
				Quaternion second3 = (Quaternion)two;
				if (target3.AlmostEquals(second3, PhotonNetwork.precisionForQuaternionSynchronization))
				{
					return true;
				}
			}
			else if (one is float)
			{
				float target4 = (float)one;
				float second4 = (float)two;
				if (target4.AlmostEquals(second4, PhotonNetwork.precisionForFloatSynchronization))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	protected internal static bool GetMethod(MonoBehaviour monob, string methodType, out MethodInfo mi)
	{
		mi = null;
		if (monob == null || string.IsNullOrEmpty(methodType))
		{
			return false;
		}
		List<MethodInfo> methods = SupportClass.GetMethods(monob.GetType(), null);
		for (int i = 0; i < methods.Count; i++)
		{
			MethodInfo methodInfo = methods[i];
			if (methodInfo.Name.Equals(methodType))
			{
				mi = methodInfo;
				return true;
			}
		}
		return false;
	}

	protected internal void LoadLevelIfSynced()
	{
		if (!PhotonNetwork.automaticallySyncScene || PhotonNetwork.isMasterClient || PhotonNetwork.room == null || !PhotonNetwork.room.CustomProperties.ContainsKey("s"))
		{
			return;
		}
		object obj = PhotonNetwork.room.CustomProperties["s"];
		if (obj is int)
		{
			if (SceneManagerHelper.ActiveSceneBuildIndex != (int)obj)
			{
				PhotonNetwork.LoadLevel((int)obj);
			}
		}
		else if (obj is string && SceneManagerHelper.ActiveSceneName != (string)obj)
		{
			PhotonNetwork.LoadLevel((string)obj);
		}
	}

	protected internal void SetLevelInPropsIfSynced(object levelId)
	{
		if (!PhotonNetwork.automaticallySyncScene || !PhotonNetwork.isMasterClient || PhotonNetwork.room == null)
		{
			return;
		}
		if (levelId == null)
		{
			Debug.LogError("Parameter levelId can't be null!");
			return;
		}
		if (PhotonNetwork.room.CustomProperties.ContainsKey("s"))
		{
			object obj = PhotonNetwork.room.CustomProperties["s"];
			if ((obj is int && SceneManagerHelper.ActiveSceneBuildIndex == (int)obj) || (obj is string && SceneManagerHelper.ActiveSceneName != null && SceneManagerHelper.ActiveSceneName.Equals((string)obj)))
			{
				return;
			}
		}
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		if (levelId is int)
		{
			hashtable["s"] = (int)levelId;
		}
		else if (levelId is string)
		{
			hashtable["s"] = (string)levelId;
		}
		else
		{
			Debug.LogError("Parameter levelId must be int or string!");
		}
		PhotonNetwork.room.SetCustomProperties(hashtable);
		SendOutgoingCommands();
	}

	public void SetApp(string appId, string gameVersion)
	{
		AppId = appId.Trim();
		if (!string.IsNullOrEmpty(gameVersion))
		{
			PhotonNetwork.gameVersion = gameVersion.Trim();
		}
	}

	public bool WebRpc(string uriPath, object parameters)
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary.Add(209, uriPath);
		dictionary.Add(208, parameters);
		return OpCustom(219, dictionary, true);
	}
}