using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Sockets;

namespace Server;

using ClassLibrary;
using system;

/// <summary>
///     Represents an async server.
/// </summary>
public partial class Server
{
    /// <summary>
    ///     Analyzes the client request and executes it.
    /// </summary>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> which has been sent by the client.</param>
    /// <returns>Instance of <see cref="Packet" /> containing the response from the <see cref="Server" />.</returns>
    public static Packet GetFromDatabase(IAsyncResult ar, Socket socket, Packet packetReceived)
    {
        // Initialize the packet to default with some received values.
        var packet = new Packet
        {
            IdMessage = packetReceived.IdMessage,
            IdPlayer = packetReceived.IdPlayer,
            IdRoom = packetReceived.IdRoom
        };
        
        var state = (StateObject?)ar.AsyncState;
        if (state is null) // Checking for errors.
        {
            // Setting the error value.
            // TODO : state is null
            packet.Error = Tools.Errors.Permission;
            return packet;
        }

        // Check IdMessage : different action

        switch (packetReceived.IdMessage)
        {
            case Tools.IdMessage.AccountSignup:
                AccountSignup(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.AccountLogin:
                AccountLogin(ar, packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.AccountLogout: // impossible
                AccountLogout(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.AccountStatistics:
                AccountStatistics(packetReceived, ref packet, socket);
                break;

            case Tools.IdMessage.RoomList:
                RoomList(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.RoomCreate:
                RoomCreate(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.RoomSettingsGet:
                RoomSettingsGet(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.RoomSettingsSet:
                RoomSettingsSet(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.AskPort:
                AskPort(packetReceived, ref packet, socket);
                break;

            case Tools.IdMessage.PlayerJoin:
                PlayerJoin(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.PlayerLeave:
                PlayerLeave(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.PlayerReady:
                PlayerReady(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.PlayerKick:
                PlayerKick(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.PlayerCheat:
                PlayerCheat(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.PlayerList:
                PlayerList(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.PlayerCurrent:
                PlayerCurrent(packetReceived, ref packet, socket);
                break;

            case Tools.IdMessage.StartGame:
                GameStart(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.EndGame:
                GameEnd(packetReceived, ref packet, socket);
                break;
            
            case Tools.IdMessage.EndTurn:
                EndTurn(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.TimerPlayer:
                TimerPlayer(ref packet, socket); 
                break;

            case Tools.IdMessage.TuileDraw:
                TuileDraw(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.TuilePlacement:
                packet.Error = TuilePlacement(packetReceived, socket);
                break;
            case Tools.IdMessage.PionPlacement:
                packet.Error = PionPlacement(packetReceived, socket);
                break;
            case Tools.IdMessage.CancelTuilePlacement:
                packet.Error = CancelTuilePlacement(packetReceived, ref packet, socket);
                break;
            case Tools.IdMessage.CancelPionPlacement:
                packet.Error = CancelPionPlacement(packetReceived, ref packet, socket);
                break;

            case Tools.IdMessage.Default:
            default:
                packet.Error = Tools.Errors.Unknown;
                break;
        }

        return packet;
    }

    /// <summary>
    ///     New user is creating an account.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void AccountSignup(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par le serveur main
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening != 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }

        var db = new Database();
        try
        {
            // Adding new user to the database.
            db.Adduser(packetReceived.Data[0], packetReceived.Data[1], packetReceived.Data[2], 0, 1, 0, 0, 0, packetReceived.Data[3]);
        }
        catch (Exception ex)
        {
            // Something went wrong.
            Console.WriteLine("ERROR: Signup : " + ex);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.Database;
        }
    }
    /// <summary>
    ///     Player is attempting to login.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void AccountLogin(IAsyncResult ar, Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par le serveur main
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening != 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }
        
        var state = (StateObject?)ar.AsyncState;
        if (state is null) // Checking for errors.
        {
            // Setting the error value.
            // TODO : state is null
            return;
        }

        var db = new Database();
        try
        {
            // Check if the input data correspond to a user.
            var result = db.Identification(packetReceived.Data[0], packetReceived.Data[1]);
            // Data does not correspond to a user.
            if (result == -1)
                packet.Error = Tools.Errors.Database;
            // Data does correspond : return the user's IdPlayer.
            else
                state.IdPlayer = packet.IdPlayer = (ulong)result;
        }
        catch (Exception ex)
        {
            // Something went wrong.
            Console.WriteLine("ERROR: Login : " + ex);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.Database;
        }
    }
    /// <summary>
    ///     Player is attempting to logout.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void AccountLogout(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par le serveur main
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening != 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attempt to remove a player from all the rooms.
        gestionnaire.LogoutPlayer(packetReceived.IdPlayer);
    }
    /// <summary>
    ///     Get the player's statistics from the database.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void AccountStatistics(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par le serveur main
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening != 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }

        var db = new Database();
        try
        {
            // Put the statistics in the data field.
            packet.Data = db.GetStatistics(packetReceived.IdPlayer);
        }
        catch (Exception ex)
        {
            // Something went wrong.
            Console.WriteLine("ERROR: Statistics : " + ex);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.Database;
        }
    }



    /// <summary>
    ///     List of rooms.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void RoomList(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par le serveur main
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening != 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attempt to get the list of rooms and some data for each room.
        var result = gestionnaire.GetRoomList();
        var db = new Database();
        
        try
        {
            // Retrieve the hosts pseudo's from the database.
            for(var i=0; i < result.Length; i+=5)
                result[i+1] = db.GetPseudo(int.Parse(result[i+1]));
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: RoomList : " + ex);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.Database;
            return;
        }
        
        // Copy the list of rooms in packet.Data
        packet.Data = result;
    }
    /// <summary>
    ///     User is creating a room.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void RoomCreate(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par le serveur main
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening != 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attempt to insert a new room.
        List<int> result = gestionnaire.CreateRoom(packetReceived.IdPlayer, socket);

        if (result[0] == -1 || result[1] == -1 || result.Count != 2)
        {
            // Something went wrong.
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.RoomCreate;
        }
        else
        {
            packet.Data = new string[2];
            // Sending the room's ID back to the client. 
            packet.Data[0] = result[0].ToString();
            // Sending the new server's port (i.e. room port) back to the client.
            packet.Data[1] = result[1].ToString();
        }
    }
    /// <summary>
    ///     Get the settings of a specific room.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void RoomSettingsGet(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }
        
        if (packetReceived.Data.Length < 1)
        {
            packet.Error = Tools.Errors.BadData;
            return;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attempt to get the room settings.
        var data = gestionnaire.SettingsRoom(packetReceived.IdRoom);

        if (data.Length > 0)
        {
            // Copy the list of rooms in packet.Data
            packet.Data = data;
        }
        else
        {
            // Something went wrong.
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.RoomSettings;
        }
    }
    /// <summary>
    ///     Update the settings of a specific room.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void RoomSettingsSet(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }
        
        if (packetReceived.Data.Length < 8)
        {
            packet.Error = Tools.Errors.BadData;
            return;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attempt to update a room.
        gestionnaire.UpdateRoom(packetReceived.IdRoom, packetReceived.IdPlayer, packetReceived.Data.Skip(0).ToArray());
    }
    /// <summary>
    ///     Client is asking the port of a room
    /// </summary>
    /// <param name="packetReceived"></param>
    /// <param name="packet"></param>
    /// <param name="socket"></param>
    public static void AskPort(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par le serveur main
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening != 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }

        if (packetReceived.Data.Length < 1)
        {
            packet.Error = Tools.Errors.BadData;
            return;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attempt to add a player to the room.
        var port = gestionnaire.CallAskPort(packetReceived.IdRoom);

        if (port != -1)
        {
            packet.Data = new string[1];
            packet.Data[0] = port.ToString();
            // TODO : ensuite client switch port, thread serveur detecte nouveau joueur et broadcast
        }
        else
        {
            // Something went wrong.
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.RoomJoin;
        }
    }



    /// <summary>
    ///     Player is joining the room.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void PlayerJoin(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par le thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Serveur_main received message instead of thread_com, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }
        
        if (packetReceived.Data.Length < 1)
        {
            packet.Error = Tools.Errors.BadData;
            return;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attempt to add a player to the room.
        Tools.Errors errors = gestionnaire.JoinPlayer(packetReceived.IdRoom, packetReceived.IdPlayer, socket);

        packet.Error = errors;
    }
    /// <summary>
    ///     Player is leaving the room/game.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void PlayerLeave(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }
        
        if (packetReceived.Data.Length < 1)
        {
            packet.Error = Tools.Errors.BadData;
            return;
        }
        
        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();
        
        // Attempt from a player to leave the room.
        var playerStatus = gestionnaire.RemovePlayer(packetReceived.IdRoom, packetReceived.IdPlayer);
        if (playerStatus != Tools.PlayerStatus.Success)
        {
            // Something went wrong.
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.RoomLeave;
        }
        // TODO : ensuite thread serveur detecte fin client et broadcast
    }
    /// <summary>
    ///     Player is being kicked out of the room/game.
    /// </summary>
    /// <remarks>Player might also be forcefully removed.</remarks>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void PlayerKick(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }
        
        if (packetReceived.Data.Length < 2)
        {
            packet.Error = Tools.Errors.BadData;
            return;
        }
        
        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();
        
        // Parse idPlayerToKick from string to ulong.
        ulong idPlayerToKick;
        try
        {
            idPlayerToKick = ulong.Parse(packetReceived.Data[1]);
        }
        catch (Exception e)
        {
            // Something went wrong.
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.RoomLeave;
            return;
        }
        
        // Attempt to kick a player from the room.
        var playerStatus = gestionnaire.KickPlayer(packetReceived.IdRoom, packetReceived.IdPlayer, idPlayerToKick);
        if (playerStatus != Tools.PlayerStatus.Success)
        {
            // Something went wrong.
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.RoomLeave;
        }
        // TODO : ensuite thread serveur detecte fin client et broadcast
    }
    /// <summary>
    ///     Player switched its status.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void PlayerReady(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }

        if (packetReceived.Data.Length < 1)
        {
            packet.Error = Tools.Errors.BadData;
            return;
        }
        
        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attempt to update a player status within the room.
        var playerStatus = gestionnaire.ReadyPlayer(packetReceived.IdRoom, packetReceived.IdPlayer);
        if (playerStatus != Tools.PlayerStatus.Success)
        {
            // Something went wrong.
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.PlayerReady;
            return;
        }

        
        // Check if everyone is ready and starts the game.
        packet.Error = gestionnaire.StartGame(packetReceived.IdRoom);
    }
    /// <summary>
    ///     Player cheated.
    /// </summary>
    /// <remarks>Player might get kicked out the game in case he cheated too much.</remarks>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void PlayerCheat(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();
        
        // Attempt to update the number of times a player has cheated in a specific game.
        // Count will be incremented but player can keep playing until the limit is reached.
        var playerStatus = gestionnaire.CheatPlayer(packetReceived.IdRoom, packetReceived.IdPlayer);
        
        // Player has reached the limit and must be kicked out.
        if (playerStatus == Tools.PlayerStatus.Kicked)
        {
            // TODO : player is kicked
        }
        else if (playerStatus != Tools.PlayerStatus.Success)
        {
            // Something went wrong.
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.IllegalPlay;
        }
    }

    /// <summary>
    /// Lists the id's of each player in the game and its name
    /// </summary>
    /// <param name="packetReceived"></param>
    /// <param name="packet"></param>
    /// <param name="socket"></param>
    public static void PlayerList(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par le thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Serveur_main received message instead of thread_com, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }

        if (packetReceived.Data.Length < 1)
        {
            packet.Error = Tools.Errors.BadData;
            return;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        string[] listOfPlayers = gestionnaire.CallPlayerList(packetReceived.IdRoom);

        if(listOfPlayers.Length == 0)
        {
            packet.Error = Tools.Errors.NotFound;
            return;
        }
        else
        {
            packet.Data = listOfPlayers;
            packet.Error = Tools.Errors.None;
        }

    }

    /// <summary>
    /// Asks the idPlayer of the actual player
    /// </summary>
    /// <param name="packetReceived"></param>
    /// <param name="packet"></param>
    /// <param name="socket"></param>
    public static void PlayerCurrent(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par le thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Serveur_main received message instead of thread_com, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }

        if (packetReceived.Data.Length < 1)
        {
            packet.Error = Tools.Errors.BadData;
            return;
        }

        Tools.Errors error = Tools.Errors.Unknown;

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Get the actual player
        ulong idActualPlayer = gestionnaire.CallPlayerCurrent(packetReceived.IdRoom);

        if(idActualPlayer != 0)
        {
            packet.IdPlayer = idActualPlayer;
            error = Tools.Errors.None;
        }

        packet.Error = error;
        
    }



    // useless
    /// <summary>
    ///     Starts a room.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> to received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void GameStart(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }
        
        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attemp to start the game.
        Tools.Errors errors = gestionnaire.StartGame(packetReceived.IdRoom);

        packet.Error = errors;
    }
    /// <summary>
    ///     Ends the game.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> to received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void GameEnd(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // joueur ou serveur ? si serveur alors à modifier
        
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }
        
        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attemp to start the game.
        gestionnaire.EndGame(packetReceived.IdRoom, packetReceived.IdPlayer);
        
        // TODO : check le retour de EndGame
    }
    
    
    
    /// <summary>
    ///     Ends a player round.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> to received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void EndTurn(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }
        
        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attemp to start the game.
        Tools.Errors errors = gestionnaire.CallEndTurn(packetReceived.IdPlayer, packetReceived.IdRoom);

        packet.Error = errors;
    }
    /// <summary>
    ///     Player's timer has expired.
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> to received.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void TimerPlayer(ref Packet packet, Socket socket)
    {
        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Attempt to stop the timer and close the round.
        gestionnaire.PlayerTimer(packet.IdRoom, packet.IdPlayer);
        
        packet.IdMessage = Tools.IdMessage.TimerPlayer;
    }
    
    
    
    // id room, id tuile, x, y, rot

    /// <summary>
    ///     tirage d'une tuile
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> to received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static void TuileDraw(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            packet.Data = Array.Empty<string>();
            packet.Error = Tools.Errors.BadPort;
            return;
        }

        _ = packetReceived;
        // tirage de 3 tuiles pour le joueur
        var list = new List<string>(packet.Data.ToList())
        {
            "numero tuile", "numero tuile", "numero tuile"
        };

        packet.Data = list.ToArray();
        packet.Error = Tools.Errors.None; // remplacer par Unknown si erreur
    }

    /// <summary>
    ///     placement d'une tuile
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> to received.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static Tools.Errors TuilePlacement(Packet packetReceived, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            return Tools.Errors.BadPort;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Vérification du coup
        Tools.Errors errors = gestionnaire.CallVerifyTilePlacement(packetReceived.IdPlayer, socket, packetReceived.IdRoom, packetReceived.Data[1], packetReceived.Data[2], packetReceived.Data[3], packetReceived.Data[4]);


        return errors; 
    }

    /// <summary>
    ///     placement d'un pion
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> to received.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static Tools.Errors PionPlacement(Packet packetReceived, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            return Tools.Errors.BadPort;
        }


        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Vérification du coup
        Tools.Errors errors = gestionnaire.CallVerifyPionPlacement(packetReceived.IdPlayer, socket, packetReceived.IdRoom, packetReceived.Data[1], packetReceived.Data[2], packetReceived.Data[3]);

        return errors; 
    }

    /// <summary>
    ///     annulation d'une tuile
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> to received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static Tools.Errors CancelTuilePlacement(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            return Tools.Errors.BadPort;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Vérification du coup
        Tools.Errors errors = gestionnaire.CallCancelTuilePlacement(packetReceived.IdPlayer, socket, packetReceived.IdRoom);

        return errors;
    }

    /// <summary>
    ///     annulation d'un pion
    /// </summary>
    /// <param name="packetReceived">Instance of <see cref="Packet" /> to received.</param>
    /// <param name="packet">Instance of <see cref="Packet" /> to send.</param>
    /// <param name="socket">Socket <see cref="Socket" />.</param>
    public static Tools.Errors CancelPionPlacement(Packet packetReceived, ref Packet packet, Socket socket)
    {
        // Vérification que la communication est reçue par un thread de com
        int portListening = ((IPEndPoint)socket.LocalEndPoint).Port;
        if (portListening == 10000)
        {
            Console.WriteLine("ERROR: Thread_com received message instead of serveur_main, IdMessage : " + packetReceived.IdMessage);
            return Tools.Errors.BadPort;
        }

        // Récupération du singleton gestionnaire
        GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

        // Vérification du coup
        Tools.Errors errors = gestionnaire.CallCancelPionPlacement(packetReceived.IdPlayer, socket, packetReceived.IdRoom);

        return errors;
    }
}
