using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class MainMenuState : State
    {
        public Transition OnGameplayEnter;
        private GameComponents gameComponents;

        public MainMenuState(string name, StateMachine machine, GameComponents gameComponents) : base(name, machine)
        {
            this.gameComponents = gameComponents;
        }

        protected override void OnFirstEnter()
        {
            base.OnFirstEnter();
            
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            DebugUtils.Log("Main Menu - OnEnter");
            ViewManager simpleViewManager = gameComponents.Get<ViewManager>();
            simpleViewManager.LoadView(ViewName.MAIN_MENU);

            EventManager.Subscribe<GenericEvent>(OnMessage);
        }

        private void OnMessage(GenericEvent ev)
        {
            if (ev.eventName == "PlayButton")
            {
                OnGameplayEnter?.Run();
            }
        }
    }
