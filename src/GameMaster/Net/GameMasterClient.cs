﻿using System;
using System.Net;
using System.Net.Sockets;
using Common;
using Common.Connection;
using Common.Connection.EventArg;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using GameMaster.Logic;
using Common.Config;

namespace GameMaster.Net
{
    
    public class GameMasterClient
    {
        private IConnection connection;

        //Contents of configuration file
        GameMasterSettings settings;



        public GameMasterClient(IConnection connection, GameMasterSettings settings)
        {
            this.connection = connection;
            this.settings = settings;

            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;
        }

        public void Connect()
        {
            connection.StartClient();
        }

        public void Disconnect()
        {
            connection.StopClient();
        }


        private void OnConnection(object sender, ConnectEventArgs eventArgs)
        {
            var address = eventArgs.Handler.GetRemoteAddress();
            ConsoleDebug.Ordinary("Successful connection with address " + address.ToString());
            var socket = eventArgs.Handler as Socket;

            //at the beginning both teams have same number of open player slots
            ulong noOfPlayersPerTeam = ulong.Parse(settings.GameDefinition.NumberOfPlayersPerTeam);

            RegisterGame registerGameMessage = new RegisterGame()
            {
                NewGameInfo = new GameInfo()
                {
                    gameName = settings.GameDefinition.GameName,
                    blueTeamPlayers = noOfPlayersPerTeam,
                    redTeamPlayers = noOfPlayersPerTeam
                }
            };


            string registerGameString = XmlMessageConverter.ToXml(registerGameMessage);
            connection.SendFromClient(socket, registerGameString);
            
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            var socket = eventArgs.Handler as Socket;

            ConsoleDebug.Message("New message from:" + socket.GetRemoteAddress() + "\n" + eventArgs.Message);

            BehaviorChooser.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message));
            
            string xmlMessage = XmlMessageConverter.ToXml(XmlMessageGenerator.GetXmlMessage());

           // connection.SendFromClient(socket, xmlMessage);


        }


        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            //System.Console.WriteLine("New message sent to {0}", address.ToString());
            //var socket = eventArgs.Handler as Socket;

        }





    }//class
}//namespace
