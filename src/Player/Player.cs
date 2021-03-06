﻿using Common.Config;
using Common.Connection;
using Common.IO.Console;
using Player.Net;
using Player.Strategy;

namespace Player
{
    public class Player
    {
        public static void Main(string[] args)
        {
            AgentCommandLineOptions options = CommandLineParser.ParseArgs<AgentCommandLineOptions>(args, new AgentCommandLineOptions());

            PlayerSettings settings = Configuration.FromFile<PlayerSettings>(options.Conf);

            PlayerClient client = new PlayerClient(new Connection(options.Address, options.Port), settings, options, new Game());
            IController playerController = new Controller().Possess(client);
            client.Connect();
            client.Disconnect();

        }
    }
}