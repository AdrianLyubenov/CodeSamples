using Assets.Scripts.Utils.Executor;

    public class InitState : State
    {
        public Transition OnComplete;

        private GameComponents gameComponents;

        public InitState(string name, StateMachine machine, GameComponents gameComponents) : base(name, machine)
        {
            this.gameComponents = gameComponents;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            DebugUtils.Log("InitState - OnEnter");
            ViewManager simpleViewManager = gameComponents.Get<ViewManager>();
            simpleViewManager.LoadView(ViewName.LOADER);

            gameComponents.Get<PopupManager>().Init();
            SoundManager.Instance.Init(gameComponents.Get<DataBank>());

            Executor executor = new Executor("InitState");
            executor.AddCommand(new CmdInitFirebase(gameComponents));
            executor.AddCommand(new CmdGetRemoteConfig(gameComponents));
            executor.AddCommand(new CmdInitOneSignal(gameComponents));
            executor.AddCommand(new CmdInitTracking(gameComponents));
            executor.AddCommand(new CmdInitMax(gameComponents));
            executor.AddCommand(new CmdInitData(gameComponents));
            executor.AddCommand(new CmdReadLegacyDb(gameComponents));
            executor.AddCommand(new CmdInitLocalNotifications(gameComponents));
            executor.AddCommand(new CmdInitCPE(gameComponents));
            executor.AddCommand(new CmdCheckUpdate(gameComponents));
            executor.onComplete += OnExecutorComplete;
            executor.Execute();
        }

        private void OnExecutorComplete()
        {
            EventManager.Broadcast(new EvSendTracking(SolitaireTrackingEvents.appOpen));
            gameComponents.Get<DataBank>().isAppInitialized = true;
            OnComplete?.Run();
        }
    }
