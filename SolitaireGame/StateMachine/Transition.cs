using System;

    public class Transition
    {
        public Action onEnter;
        public Action onExit;
        public Action onUpdate;

        private State from;
        private State to;
        private bool isActive;

        public Transition(State from, State to)
        {
            this.from = from;
            this.to = to;
            from.Add(this);
        }

        public void Run()
        {
            if (isActive)
            {
                from.Exit();

                to.Enter();
            }
        }
    
        public void Update()
        {
            if (isActive)
            {
                OnUpdate();
            }
        }

        internal void Enter()
        {
            if (!isActive)
            {
                isActive = true;
                OnEnter();
            }
        }

        internal void Exit()
        {
            if (isActive)
            {
                isActive = false;
                OnExit();
            }
        }

        protected virtual void OnEnter()
        {
            onEnter?.Invoke();
        }
        
        protected virtual void OnExit()
        {
            onExit?.Invoke();
        }
        
        protected virtual void OnUpdate()
        {
            onUpdate?.Invoke();
        }
    }
