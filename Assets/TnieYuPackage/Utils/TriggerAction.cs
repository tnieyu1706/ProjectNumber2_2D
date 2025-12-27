using System;

namespace TnieYuPackage.Utils
{
    /// <summary>
    /// TriggerAction is a normal Action with ensure when func call & return true
    /// It automatic remove from Action.DelegateList immediately.
    /// </summary>
    public class TriggerAction : BaseTriggerAction
    {
        public override bool TryToAddTrigger(Func<bool> trigger)
        {
            ActionEvent += trigger;
            return true;
        }
    }

    /// <summary>
    /// TriggerAction<T> is a normal Action<T> with ensure when func call & return true
    /// It automatic remove from Action.DelegateList immediately.
    /// </summary>
    public class TriggerAction<T1> : BaseTriggerAction<T1>
    {
        public override bool TryToAddTrigger(Func<T1, bool> trigger)
        {
            ActionEvent += trigger;
            return true;
        }
    }
}