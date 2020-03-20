using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Physics;
using Xamarin.Essentials;

namespace Planer
{
    public class Game : Application
    {
  
        
        private Scene _commonScene = null;
        private int _lastScore = 0;
        private bool _isNewGameStarted = false;
        private string _gameOverString = "";
        private string _welcomeString = "Tap to play";

        Menu _menu;
        GameSession _gameSession;

        public Game(ApplicationOptions opts) : base(opts) { }

        protected override void Start()
        {

            base.Start();
            _menu = new Menu();
            _gameSession = new GameSession();
            _menu.AddScene(ref _commonScene);
            _menu.WelcomeString = _welcomeString;
            _menu.CreateMenu();
            _gameSession.AddScene(ref _commonScene);
            _gameSession.GameOvered += onGameOver;
        }

        protected override async void OnUpdate(float timeStep)
        {
            if (_isNewGameStarted)
                return;
            var input = Input;
            if (input.GetMouseButtonDown(MouseButton.Left) || input.NumTouches > 0)
            {
                _isNewGameStarted = true;
                _menu.RemoveMenu();
                _gameSession.StartGame();
            }
        }
        private void onGameOver()
        {
            _lastScore = _gameSession.Scores;
            _gameOverString = "GameOvered with " + _lastScore.ToString() + " scores";
            _menu.ResultString = _gameOverString;
            _menu.CreateMenu();
            _isNewGameStarted = false;
        }
        
    }
}
