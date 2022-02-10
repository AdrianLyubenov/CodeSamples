using System;
using System.Threading;
using Events;
using UnityEngine;

    public class GameplayState : State
    {
        public Transition OnRestartGameplay;
        public Transition OnInitNewGame;
        private GameComponents gameComponents;
        private bool isFirstEnter = false;
        private AdsManager adsManager;
        private AdsInterstitialsController adsInterstitialsController;
        private GameplayView gameplayView;
        private DataBank dataBank;
        private CardLogic[] dailyChallengeDeck = null;

        private CountdownEvent challengeStartCounter = null;

        public GameplayState(string name, StateMachine machine, GameComponents gameComponents) : base(name, machine)
        {
            this.gameComponents = gameComponents;
        }

        protected override void OnFirstEnter()
        {
            base.OnFirstEnter();

            DebugUtils.Log("Gameplay - OnFirstEnter");
            ViewManager simpleViewManager = gameComponents.Get<ViewManager>();
            simpleViewManager.LoadView(ViewName.GAMEPLAY);

            isFirstEnter = true;
            this.adsManager = gameComponents.Get<AdsManager>();
            this.gameplayView = gameComponents.Get<GameplayView>();
            this.dataBank = gameComponents.Get<DataBank>();
            this.adsInterstitialsController = gameComponents.Get<AdsInterstitialsController>();

            EventManager.Subscribe<GenericEvent>(OnMessage);
            EventManager.Subscribe<EvStartChallenge>(OnStartChallenge);
            EventManager.Subscribe<EvDailyChallengesCancelled>(OnDailyChallengesCancelled);

            if (dataBank.openDailyChallengesAfterLaunch)
                gameComponents.Get<PopupManager>().Show(PopupNames.POPUP.DC_POPUP, new object[] { dataBank.dailyChallengesModel });
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            DebugUtils.Log("Gameplay - OnEnter");
            gameplayView.OnInitialized += OnGameplayInitialized;
            gameplayView.LoadGameplay(false, gameComponents, isFirstEnter);
        }

        private void OnGameplayInitialized()
        {
            if (isFirstEnter)
            {
                DebugUtils.Log("OnGameplayInitialized");
                isFirstEnter = false;
                adsManager.InitializeBannerAds();
                adsManager.ShowBanner();
            }
        }

        private void OnMessage(GenericEvent ev)
        {
            if (ev.eventName == Const.EV_RESTART_GAME)
            {
                Restart();
            }
            else if (ev.eventName == Const.EV_NEW_GAME)
            {
                NewGame();
            }
            else if (ev.eventName == Const.EV_NEW_GAME_AFTER_AD)
            {
                dataBank.dailyChallengeActive = false;
                adsInterstitialsController.OnAdCompleted += OnInterstitialNewGameCompleted;
                adsInterstitialsController.ShowInterstitial(AdsInterstitialsController.AD_TYPE.GAME_START, dataBank.dailyChallengeActive);
            }
            else if (ev.eventName == Const.EV_RESTART_AFTER_AD)
            {
                adsInterstitialsController.OnAdCompleted += OnInterstitialReplayCompleted;
                adsInterstitialsController.ShowInterstitial(AdsInterstitialsController.AD_TYPE.GAME_START, dataBank.dailyChallengeActive);
            }
        }

        private void OnDailyChallengesCancelled(EvDailyChallengesCancelled ev)
        {
            dataBank.dailyChallengeActive = false;
            adsInterstitialsController.OnAdCompleted += OnInterstitialNewGameCompleted;
            adsInterstitialsController.ShowInterstitial(AdsInterstitialsController.AD_TYPE.GAME_START, dataBank.dailyChallengeActive);
        }

        private void OnStartChallenge(EvStartChallenge ev)
        {
            EventManager.Broadcast(new EvShowShroud());
            dataBank.dailyChallengeDay = ev.dayIdx;
            dataBank.dailyChallengeActive = true;
            dataBank.dailyChallengeStartTime = DateTime.Now;
            dataBank.dailyChallengesModel.AddAttempt(dataBank.dailyChallengeDay);
            DailyChallengesLogger.LogGameStart(dataBank);

            challengeStartCounter = new CountdownEvent(2);

            if (CalendarModel.IsDayIdxToday(ev.dayIdx))
            {
                PlayerPrefs.SetInt(Const.PREFS_DC_TODAY_PLAYED_IDX, ev.dayIdx);
            }

            adsInterstitialsController.OnAdCompleted += OnInterstitialChallengeCompleted;
            adsInterstitialsController.ShowInterstitial(AdsInterstitialsController.AD_TYPE.GAME_START, dataBank.dailyChallengeActive);

            PrepareDailyChallengeDeck();
        }

        private void OnInterstitialReplayCompleted(bool isShown)
        {
            adsInterstitialsController.OnAdCompleted -= OnInterstitialReplayCompleted;
            Restart();
        }

        private void OnInterstitialNewGameCompleted(bool isShown)
        {
            adsInterstitialsController.OnAdCompleted -= OnInterstitialNewGameCompleted;
            NewGame();
        }

        private void OnInterstitialChallengeCompleted(bool isShown)
        {
            adsInterstitialsController.OnAdCompleted -= OnInterstitialChallengeCompleted;
            challengeStartCounter.Signal();
        }

        private void NewGame()
        {
            gameplayView.UnloadGameplay();
            dataBank.dailyChallengeActive = false;
            gameplayView.LoadGameplay(true, gameComponents, false);
        }

        private void Restart()
        {
            if (dataBank.dailyChallengeActive)
            {
                dataBank.dailyChallengesModel.AddAttempt(dataBank.dailyChallengeDay);
                DailyChallengesLogger.LogGameRestart(dataBank);
            }

            gameplayView.UnloadGameplay();
            dataBank.GamesPlayed += 1;
            OnRestartGameplay?.Run();
        }

        private void PrepareDailyChallengeDeck()
        {
            int drawMode = dataBank.DrawMode;
            int seed = 10 * dataBank.dailyChallengeDay + drawMode;
            TextAsset textAsset = Resources.Load<TextAsset>("Challenges/Android/d" + seed.ToString());
            if (textAsset == null)
            {
                Thread t = new Thread( () => PrepareDailyChallengeSolverDeck(drawMode, seed) );
                t.Start();
            }
            else
            {
                dailyChallengeDeck = CardLogic.DeserializeDeck(textAsset);
                challengeStartCounter.Signal();
            }
        }

        private void PrepareDailyChallengeSolverDeck(int drawMode, int seed)
        {
            Solver solver = new Solver();
            dailyChallengeDeck = solver.GetWinnableDeck(seed, drawMode);
            challengeStartCounter.Signal();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (challengeStartCounter != null && challengeStartCounter.CurrentCount == 0)
            {
                EventManager.Broadcast(new EvHideShroud());
                gameplayView.UnloadGameplay();
                gameplayView.LoadGameplay(true, gameComponents, false, dailyChallengeDeck);

                challengeStartCounter.Dispose();
                challengeStartCounter = null;
            }
        }
    }
