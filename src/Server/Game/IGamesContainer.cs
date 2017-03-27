﻿using System.Collections.Generic;

namespace Server.Game
{
    public interface IGamesContainer : IEnumerable<IGame>
    {
        int Count { get; }
        
        void RegisterGame(IGame game);
        IGame GetGameById(int id);
        IGame GetGameByName(string name);
        void RemoveGame(IGame game);
        int NextGameId();
    }
}