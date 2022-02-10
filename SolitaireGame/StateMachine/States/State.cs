using System;
using System.Collections;
using System.Collections.Generic;

    public class State
    {
        public Action onEnter;
        public Action onExit;
        public Action onUpdate;
        public string name = "";

        private bool isActive = false;
        private bool isFirstEnter = true;
        private StateMachine stateMachine;
        private List<Transition> transitions = new List<Transition>();

        public State(string name, StateMachine stateMachine)
        {
            this.name = name;
            this.stateMachine = stateMachine;
            stateMachine.Add(this);
        }

        internal void Add(Transition transition)
        {
            transitions.Add(transition);
        }

        internal void SetStateMachine(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void Enter()
        {
            if (!isActive)
            {
                stateMachine.StateEnter(this);
                isActive = true;

                foreach (Transition transition in transitions)
                {
                    if (!isActive)
                        break;
                    transition.Enter();
                }

                if (isFirstEnter)
                {
                    OnFirstEnter();
                    isFirstEnter = false;
                }
                OnEnter();
            }
        }

        internal void Exit()
        {
            if (isActive)
            {
                isActive = false;

                foreach (Transition transition in transitions)
                    transition.Exit();

                OnExit();
                stateMachine.StateExit(this);
            }
        }

        public void Update()
        {
            if (isActive)
            {
                OnUpdate();

                foreach (Transition transition in transitions)
                {
                    transition.Update();
                    if (!isActive)
                        break;
                }
            }
        }

        protected virtual void OnUpdate()
        {
            onUpdate?.Invoke();
        }

        protected virtual void OnFirstEnter()
        {

        }

        protected virtual void OnEnter()
        {
            onEnter?.Invoke();
        }

        protected virtual void OnExit()
        {
            onExit?.Invoke();
        }
    }
