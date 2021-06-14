using System;
using System.IO;
using System.Net;
using UnityEngine;

namespace DoubTech.JoesChessApi
{
    public class JoesChessApiRequest<T> where T : class
    {
        private HttpWebRequest request;
        internal string endpoint;
        public HttpStatusCode statusCode;
        public T result;

        private class Actions
        {
            public Action<T> onResult;
            public Action onError;
        }

        internal JoesChessApiRequest()
        {

        }

        internal void ChessApiRequest(string moves, Action<T> result, Action onError = null)
        {
            request =
                (HttpWebRequest) WebRequest.Create(
                    $"http://chess-api.herokuapp.com/{endpoint}/{moves}");
            request.BeginGetResponse(HandleResponse, new Actions()
            {
                onResult = result,
                onError = onError
            });
        }

        private void HandleResponse(IAsyncResult ar)
        {
            var response = (HttpWebResponse) request.EndGetResponse(ar);
            bool error = true;
            statusCode = response.StatusCode;

            var actions = (Actions) ar.AsyncState;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var responseValue = streamReader.ReadToEnd();
                    result = JsonUtility.FromJson<T>(responseValue);
                    actions.onResult?.Invoke(result);
                }
            }

            response.Close();

            if (error)
            {
                Debug.Log("Error: " + response.StatusCode + ": " + response.StatusDescription);
                actions.onError?.Invoke();
            }
        }
    }

    [Serializable]
    public class NextBest
    {
        public const string END_POINT = "next_best";

        public string bestNext;

        public char CurrentFile => bestNext[0];
        public int CurrentRank => int.Parse("" + bestNext[1]);

        public char TargetFile => bestNext[2];
        public int TargetRank => int.Parse("" + bestNext[3]);

        public string TargetSquare => bestNext.Substring(2);
        public string CurrentSquare => bestNext.Substring(0, 2);
    }

    [Serializable]
    public class ValidMove
    {
        public const string END_POINT = "valid_move";
        public bool validMove;
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

    public class JoesChessApiRequestFactory
    {
        public static JoesChessApiRequest<NextBest> RequestNextBest(string moves,
            Action<NextBest> result, Action onError = null)
        {
            var request = new JoesChessApiRequest<NextBest>()
            {
                endpoint = NextBest.END_POINT
            };

            request.ChessApiRequest(moves, result, onError);

            return request;
        }

        public static JoesChessApiRequest<Status> RequestStatus(string moves, Action<Status> result,
            Action onError = null)
        {
            var request = new JoesChessApiRequest<Status>()
            {
                endpoint = Status.END_POINT
            };

            request.ChessApiRequest(moves, result, onError);

            return request;
        }

        public static JoesChessApiRequest<ValidMove> RequestValidMove(string moves,
            Action<ValidMove> result, Action onError = null)
        {
            var request = new JoesChessApiRequest<ValidMove>()
            {
                endpoint = ValidMove.END_POINT
            };

            request.ChessApiRequest(moves, result, onError);

            return request;
        }
    }
}
