using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace DoubTech.JoesChessApi
{
    public class Chessgame : MonoBehaviour
    {
        public string moves;

        private NextBest nextBest;
        private JoesChessApiRequest<NextBest> nextBestRequest;
        private List<Action<NextBest>> pendingNextBestResult = new List<Action<NextBest>>();

        private ValidMove validMove;
        private JoesChessApiRequest<ValidMove> validMoveResult;
        private List<Action<ValidMove>> pendingValidMoveResults = new List<Action<ValidMove>>();

        public void RecordMove(int sourceRank, char sourceFile, int destRank, char destFile)
        {
            moves += sourceFile;
            moves += sourceRank;
            moves += destFile;
            moves += destRank;
        }

        public void FindNextMove(Action<NextBest> result)
        {
            // The result is not returned on the main thread. When we get a result it must be queued
            // to be picked up in the next update.
            pendingNextBestResult.Add(result);
            nextBestRequest =
                JoesChessApiRequestFactory.RequestNextBest(moves, (n) => nextBest = n);
        }

        public void ValidateMove(string piecePosition, string potentialPosition,
            Action<ValidMove> result)
        {
            // The result is not returned on the main thread. When we get a result it must be queued
            // to be picked up in the next update.
            pendingValidMoveResults.Add(result);
            validMoveResult =
                JoesChessApiRequestFactory.RequestValidMove(
                    moves + piecePosition + potentialPosition, (v) => validMove = v);
        }

        public void CheckLastMove(Action<ValidMove> result)
        {
            if (moves.Length > 4)
            {
                // The result is not returned on the main thread. When we get a result it must be queued
                // to be picked up in the next update.
                pendingValidMoveResults.Add(result);
                validMoveResult =
                    JoesChessApiRequestFactory.RequestValidMove(moves, (v) => validMove = v);
            }
            else
            {
                Debug.LogError("Can't check move. No moves recorded yet.");
            }
        }

        private void Update()
        {
            if (null != nextBest)
            {
                var move = nextBest;
                nextBest = null;

                if (pendingNextBestResult.Count > 0)
                {
                    var queue = pendingNextBestResult;
                    pendingNextBestResult = new List<Action<NextBest>>();

                    for (int i = 0; i < queue.Count; i++)
                    {
                        queue[i].Invoke(move);
                    }
                }
            }

            if (null != validMove)
            {
                var move = validMove;
                validMove = null;

                if (pendingValidMoveResults.Count > 0)
                {
                    var queue = pendingValidMoveResults;
                    pendingValidMoveResults = new List<Action<ValidMove>>();

                    for (int i = 0; i < queue.Count; i++)
                    {
                        queue[i].Invoke(move);
                    }
                }
            }
        }

        public void Reset()
        {
            moves = "";
        }
    }
}
