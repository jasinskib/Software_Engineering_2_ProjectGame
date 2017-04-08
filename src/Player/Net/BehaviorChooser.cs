﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using System.Net.Sockets;

namespace Player.Net
{
    public static class BehaviorChooser /*: IMessageHandler<ConfirmGameRegistration>*/
    {
        public static void HandleMessage(RegisteredGames message, PlayerMessageHandleArgs args)
        {
            if (message.GameInfo == null || message.GameInfo.Length == 0)
            {
                Task.Run(() =>
                {
                    Thread.Sleep((int) args.Settings.RetryJoinGameInterval);
                    string xmlMessage = XmlMessageConverter.ToXml(new GetGames());
                    args.Connection.SendFromClient(args.Socket, xmlMessage);
                });
            }
            else
            {
                ConsoleDebug.Good("Games available");
                if (args.Options.GameName == null)
                {
                    ConsoleDebug.Warning("Game name not specified");
                    return;
                }
                if (message.GameInfo.Count(info => info.gameName == args.Options.GameName) == 1)
                {
                    string xmlMessage = XmlMessageConverter.ToXml(new JoinGame()
                    {
                        gameName = args.Options.GameName,
                        playerIdSpecified = false,
                        preferredRole = args.Options?.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
                        teamColour = args.Options?.PreferredTeam == "red" ? TeamColour.red : TeamColour.blue
                    });
                    args.Connection.SendFromClient(args.Socket, xmlMessage);
                }
            }
        }

        public static void HandleMessage(ConfirmJoiningGame message, PlayerMessageHandleArgs args)
        {
            if (message == null)
                return;

            args.PlayerClient.Id = message.playerId;
            args.PlayerClient.GameId = message.gameId;
            args.PlayerClient.Guid = message.privateGuid;
            args.PlayerClient.Team = message.PlayerDefinition.team;
            args.PlayerClient.Type = message.PlayerDefinition.type;

            return;
        }

        public static void HandleMessage(RejectJoiningGame message, PlayerMessageHandleArgs args)
        {
            if (message == null)
                return;
            //try to connect again
            args.Connection.SendFromClient(args.Socket, XmlMessageConverter.ToXml(new GetGames()));
            
            return;
        }

        public static void HandleMessage(Game message, PlayerMessageHandleArgs args)
        {
            args.PlayerClient.Players = message.Players;
            args.PlayerClient.Board = message.Board;
            args.PlayerClient.Location = message.PlayerLocation;
            args.PlayerClient.Fields = new Common.SchemaWrapper.Field[message.Board.width, 2*message.Board.goalsHeight + message.Board.tasksHeight];
            ConsoleDebug.Good("Game started");
            args.PlayerClient.Play();
            
        }

        public static void HandleMessage(Data message, PlayerMessageHandleArgs args)
        {
            if (message.PlayerLocation != null)
            {
                args.PlayerClient.Location = message.PlayerLocation;
                ConsoleDebug.Message($"My location: ({message.PlayerLocation.x}, {message.PlayerLocation.y})");
            }
            if (message.TaskFields != null)
            {
                Common.SchemaWrapper.TaskField[] taskFields = new Common.SchemaWrapper.TaskField[message.TaskFields.Length];
                for (int i = 0; i < taskFields.Length; i++)
                    taskFields[i] = new Common.SchemaWrapper.TaskField(message.TaskFields[i]);
                FieldsUpdater(args.PlayerClient.Fields, taskFields); ;
            }
            if (message.GoalFields != null)
            {
                Common.SchemaWrapper.GoalField[] goalFields = new Common.SchemaWrapper.GoalField[message.GoalFields.Length];
                for (int i = 0; i < goalFields.Length; i++)
                    goalFields[i] = new Common.SchemaWrapper.GoalField(message.GoalFields[i]);
                FieldsUpdater(args.PlayerClient.Fields, goalFields);
            }
            if (message.Pieces != null)
            {
                foreach (Piece piece in message.Pieces)
                {
                    (args.PlayerClient.Fields[args.PlayerClient.Location.x, args.PlayerClient.Location.y] as Common.SchemaWrapper.TaskField).PieceId = piece.id;
                    (args.PlayerClient.Fields[args.PlayerClient.Location.x, args.PlayerClient.Location.y] as Common.SchemaWrapper.TaskField).Timestamp = piece.timestamp;
                    (args.PlayerClient.Fields[args.PlayerClient.Location.x, args.PlayerClient.Location.y] as Common.SchemaWrapper.TaskField).PlayerId = piece.playerId;
                    (args.PlayerClient.Fields[args.PlayerClient.Location.x, args.PlayerClient.Location.y] as Common.SchemaWrapper.TaskField).DistanceToPiece = 0;
                }
            }
            if (message.gameFinished == true)
            {
                args.PlayerClient.Disconnect();
            }

            args.PlayerClient.Play();
        }

        public static void HandleMessage(KnowledgeExchangeRequest message, PlayerMessageHandleArgs args)
        {
            var fromPlayer = args.PlayerClient.Players.Where(p => p.id == message.senderPlayerId).Single();

            bool accept = false;
            //it is our leader, we have to listen to him or we are the leader
            if (fromPlayer.team == args.PlayerClient.Team 
                && (fromPlayer.type == PlayerType.leader || args.PlayerClient.Type == PlayerType.leader))
                accept = true;
            else    //decide if we really want to exchange information
                accept = true;

            if (accept)
            {
                //when you accept an information exchange you have to send an AuthorizeKnowledgeExchange to the gm
                var exchange = new AuthorizeKnowledgeExchange()
                {
                    gameId = args.PlayerClient.GameId,
                    playerGuid = args.PlayerClient.Guid,
                    withPlayerId = fromPlayer.id
                };
                args.Connection.SendFromClient(args.Socket, XmlMessageConverter.ToXml(exchange));

            }
            else
            {
                //otherwise send a RejectKnowledgeExchange directly to the player
                var reject = new RejectKnowledgeExchange()
                {
                    playerId = fromPlayer.id,
                    senderPlayerId = args.PlayerClient.Id
                };
                args.Connection.SendFromClient(args.Socket, XmlMessageConverter.ToXml(reject));
            }

        }

        public static void HandleMessage(AcceptExchangeRequest message, PlayerMessageHandleArgs args)
        {

        }

        public static void HandleMessage(RejectKnowledgeExchange message, PlayerMessageHandleArgs args)
        {

        }

        public static void HandleMessage(object message, PlayerMessageHandleArgs args)
        {
            ConsoleDebug.Warning("Unknown Type");
        }

        private static void FieldsUpdater(Common.SchemaWrapper.Field[,] oldTaskFields, Common.SchemaWrapper.Field[] newTaskFields)
        {
            foreach (Common.SchemaWrapper.Field taskField in newTaskFields)
            {
                oldTaskFields[taskField.X, taskField.Y] = taskField;
            }
        }

    }
}