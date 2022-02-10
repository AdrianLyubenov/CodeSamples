using System;
using System.Collections.Generic;

    public class StateMachine
    {
        public Action onEnter;
        public Action onExit;
        public Action onUpdate;
        public Action onStateChanged;

        private List<State> states = new List<State>();
        private State current = null;
        private Dictionary<object, object> cache = new Dictionary<object, object>();
        private Dictionary<string, State> statesByName = new Dictionary<string, State>();

        private bool isActive = false;

        public void Update()
        {
            if (isActive)
            {
                onUpdate?.Invoke();
                current.Update();
            }
        }

        public void Enter()
        {
            if (!isActive)
            {
                isActive = true;
                onEnter?.Invoke();
                states[0].Enter();
            }
        }

        internal void StateEnter(State state)
        {
            if (state == null)
                throw new ArgumentException("StateEnter - Missing state!");
            if (current == null)
            {
                DebugUtils.Log("SM - Enter state " + state.name);
                current = state;
                onStateChanged?.Invoke();
            }
            else
            {
                throw new Exception("Can't enter state " + state.name + ". Current one " + current.name + " is still in progress!");
            }
        }

        internal void StateExit(State state)
        {
            if (state == null)
                throw new ArgumentException("StateExit - Missing state!");
            if (ReferenceEquals(current, state))
            {
                DebugUtils.Log("SM - Exit from state " + state.name);
                current = null;
            }
            else
                throw new Exception("StateExit - current state is " + current.name + ". Can't exit state " + state.name + "!");
        }

        public void Exit()
        {
            if (isActive)
            {
                current.Exit();
                isActive = false;
                onExit?.Invoke();
                cache.Clear();
            }
        }

        public void Add(State state)
        {
            states.Add(state);
            state.SetStateMachine(this);
        }

        public State GetState(string name)
        {
            State state = null;
            statesByName.TryGetValue(name, out state);
            return state;
        }

        public string CurrentStateName
        {
            get { return current != null ? current.name : ""; }
        }

        public State CurrentState
        {
            get { return current; }
        }

        public void StoreValue(object key, object value)
        {
            if (cache.ContainsKey(key))
                cache[key] = value;
            else
                cache.Add(key, value);
        }

        public void CleanValue(object key)
        {
            if (cache.ContainsKey(key))
                cache.Remove(key);
        }
    }
