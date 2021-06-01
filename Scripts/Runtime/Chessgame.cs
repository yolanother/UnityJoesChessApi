using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoubTech.JoesChessApi
{
    public class Chessgame : MonoBehaviour
    {
        public string moves;

        private HashSet<JoesChessApiRequest> requests = new HashSet<JoesChessApiRequest>();

        public void RecordMove(int sourceRank, char sourceFile, int destRank, char destFile)
        {
            moves += sourceFile;
            moves += sourceRank;
            moves += destFile;
            moves += destRank;
        }

        public void FindNextMove(Action<NextBest> result)
        {
            requests.Add(JoesChessApiRequest.RequestNextBest(moves, result));
        }
    }
}
