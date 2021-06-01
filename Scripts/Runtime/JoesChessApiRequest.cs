using System;
using System.IO;
using System.Net;
using UnityEditor.PackageManager;
using UnityEngine;

namespace DoubTech.JoesChessApi
{
    public class JoesChessApiRequest
    {
        private HttpWebRequest request;
        private string endpoint;
        public HttpStatusCode statusCode;

        private JoesChessApiRequest()
        {

        }

        public static JoesChessApiRequest RequestNextBest(string moves, Action<NextBest> result)
        {
            var request = new JoesChessApiRequest()
            {
                endpoint = NextBest.END_POINT
            };

            request.ChessApiRequest(moves, result);

            return request;
        }

        public static JoesChessApiRequest RequestStatus(string moves, Action<Status> result)
        {
            var request = new JoesChessApiRequest()
            {
                endpoint = Status.END_POINT
            };

            request.ChessApiRequest(moves, result);

            return request;
        }

        public static JoesChessApiRequest RequestValidMove(string moves, Action<ValidMove> result)
        {
            var request = new JoesChessApiRequest()
            {
                endpoint = ValidMove.END_POINT
            };

            request.ChessApiRequest(moves, result);

            return request;
        }

        private void ChessApiRequest<T>(string moves, Action<T> result)
        {
            var request =
                (HttpWebRequest) WebRequest.Create(
                    $"http://chess-api.herokuapp.com/{endpoint}/{moves}");
            request.BeginGetResponse(HandleResponse, result);
        }

        private void HandleResponse(IAsyncResult ar)
        {
            var response = (HttpWebResponse) request.EndGetResponse(ar);
            bool error = true;
            statusCode = response.StatusCode;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var responseValue = streamReader.ReadToEnd();
                    switch (endpoint)
                    {
                        case NextBest.END_POINT:
                            var nextBest = JsonUtility.FromJson<NextBest>(responseValue);
                            ((Action<NextBest>) ar.AsyncState).Invoke(nextBest);
                            error = false;
                            break;
                        case ValidMove.END_POINT:
                            var validMove = JsonUtility.FromJson<ValidMove>(responseValue);
                            ((Action<ValidMove>) ar.AsyncState).Invoke(validMove);
                            error = false;
                            break;
                        case Status.END_POINT:
                            var status = JsonUtility.FromJson<Status>(responseValue);
                            ((Action<Status>) ar.AsyncState).Invoke(status);
                            error = false;
                            break;
                    }
                }
            }

            response.Close();

            if (error)
            {
                ((Action<Status>) ar.AsyncState).Invoke(null);
            }
        }
    }

    [Serializable]
    public class NextBest
    {
        public const string END_POINT = "next_best";

        public string bestNext;
    }

    [Serializable]
    public class ValidMove
    {
        public const string END_POINT = "valid_move";
        public string validMove;
    }

    [Serializable]
    public class Status
    {
        public const string END_POINT = "status";
        public string gameStatus;

        private GameStatus status;

        public GameStatus CurrentStatus
        {
            get
            {
                if (status == GameStatus.Unknown)
                {
                    return Enum.TryParse<GameStatus>(gameStatus.Replace("_", ""), true,
                        out var status)
                        ? status
                        : GameStatus.Unknown;
                }

                return status;
            }
        }
    }

    public enum GameStatus
    {
        InProgress,
        WhiteWon,
        BlackWon,
        WhiteWonResign,
        BlackWonResign,
        StaleMate,
        InsufficientMaterial,
        FiftyRuleMove,
        ThreefoldRepitition,
        Unknown,
    }
}
