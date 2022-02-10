using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamewise.GameTemplate
{

    public struct GenericEvent
    {
        public string eventName;

        public GenericEvent(string eventName)
        {
            this.eventName = eventName;
        }
    }
}