using System;
using TnieYuPackage.GlobalExtensions;

namespace TnieYuPackage.Utils
{
    /// <summary>
    /// TriggerAction is a normal Action with ensure when func call & return true
    /// It automatic remove from Action.DelegateList immediately.
    /// And when TryToAddTrigger it consider trigger is format.
    /// </summary>
    public class SafeTriggerAction : BaseTriggerAction
    {
        public override bool TryToAddTrigger(Func<bool> trigger)
        {
            if (ActionEvent.Contains(trigger)) return false;
            
            ActionEvent += trigger;
            return true;
        }
    }

    /// <summary>
    /// TriggerAction<T> is a normal Action<T> with ensure when func call & return true
    /// It automatic remove from Action.DelegateList immediately.
    /// And when TryToAddTrigger it consider trigger is format.
    /// </summary>
    public class SafeTriggerAction<T1> : BaseTriggerAction<T1>
    {
        public override bool TryToAddTrigger(Func<T1, bool> trigger)
        {
            if (ActionEvent.Contains(trigger)) return false;
            
            ActionEvent += trigger;
            return true;
        }
    }
}