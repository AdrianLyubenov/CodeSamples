using System.Collections.Generic;
using UnityEngine;

    public class StateMachineUpdate : MonoBehaviour
    {
        private List<StateMachine> machines = new List<StateMachine>();
        private List<StateMachine> machinesToAdd = new List<StateMachine>();
        private List<StateMachine> machinesToRemove = new List<StateMachine>();

        private bool inLoop = false;

        public void Init()
        {
            DontDestroyOnLoad(this);
        }

        public void Register(StateMachine stateMachine)
        {
            if (!machines.Contains(stateMachine))
            {
                if (inLoop)
                    machines.Add(stateMachine);
                else
                    machinesToAdd.Add(stateMachine);
            }
        }

        public void Unregister(StateMachine stateMachine)
        {
            if (machines.Contains(stateMachine))
            {
                if (!inLoop)
                    machines.Remove(stateMachine);
                else
                    machinesToRemove.Add(stateMachine);
            }
        }

        private void Update()
        {
            inLoop = true;
            machines.ForEach((machine) => machine.Update());

            machinesToRemove.ForEach((machine) => machines.Remove(machine));
            machinesToRemove.Clear();

            machinesToAdd.ForEach((machine) => machines.Add(machine));
            machinesToAdd.Clear();
            inLoop = false;
        }
    }
