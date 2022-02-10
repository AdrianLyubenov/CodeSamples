    public sealed class ApplicationFlow : StateMachine
    {
        private GameComponents gameComponents;

        private InitState initState;
        private GameplayState gameplay;
        
        public ApplicationFlow(GameComponents gameComponents)
        {
            this.gameComponents = gameComponents;

            initState = new InitState("InitState", this, gameComponents);
            gameplay = new GameplayState("Gameplay", this, gameComponents);

            Transition initToGameplay = new Transition(initState, gameplay);
            Transition gameplayToGameplay = new Transition(gameplay, gameplay);

            initState.OnComplete = initToGameplay;
            gameplay.OnRestartGameplay = gameplayToGameplay;
        }
    }
