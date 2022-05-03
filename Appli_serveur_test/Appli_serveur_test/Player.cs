﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace system
{
    /// <summary>
    ///     Information du joueur
    /// </summary>
    public class Player
    {
        /* Attributs */

        /// <summary>
        /// Id du joueur
        /// </summary>
        public ulong _id_player { get; }
        /// <summary>
        /// Score du joueur
        /// </summary>
        public uint _score { get; set; }
        /// <summary>
        /// Nombre de tentatives de triche du joueur
        /// </summary>
        public uint _triche { get; set; }
        /// <summary>
        /// Si le joueur est prêt dans le lobby
        /// </summary>
        public bool _is_ready { get; set; }
        /// <summary>
        /// Socket du joueur
        /// </summary>
        public Socket? _socket_of_player { get; set; }
        /// <summary>
        /// Nombre de meeples du joueur
        /// </summary>
        public ulong _nbMeeples { get; set; }
        /// <summary>
        /// Semaphore du joueur pour éviter les accès concurrents
        /// </summary>
        public Semaphore _s_player;

        /// <summary>
        /// Ajout de points au joueur
        /// </summary>
        /// <param name="points"></param>
        public void AddPoints(uint points)
        {
            _s_player.WaitOne();
            Console.WriteLine("Gain de points ! Joueur " + _id_player.ToString() + " a gagné " + points.ToString() + " supplémentaires ! ("
                + _score.ToString() + "->" + (_score+points).ToString());
            _score = _score + points;
            _s_player.Release();
        }

        /// <summary>
        /// Création d'un nouveau joueur
        /// </summary>
        /// <param name="id_player"></param>
        /// <param name="playerSocket"></param>
        public Player(ulong id_player, Socket? playerSocket)
        {
            _id_player = id_player;
            _score = 0; 
            _triche = 0;
            _is_ready = false;
            _socket_of_player = playerSocket;
            _nbMeeples = 0;
            _s_player = new Semaphore(1, 1);
        }

        /// <summary>
        /// Création d'un nouveau joueur
        /// </summary>
        /// <param name="id_player"></param>
        /// <param name="playerSocket"></param>
        /// <param name="nbMeeples"></param>
        public Player(ulong id_player, Socket? playerSocket, ulong nbMeeples)
        {
            _id_player = id_player;
            _score = 0;
            _triche = 0;
            _is_ready = false;
            _socket_of_player = playerSocket;
            _nbMeeples = nbMeeples;
            _s_player = new Semaphore(1, 1);
        }

    }
}
