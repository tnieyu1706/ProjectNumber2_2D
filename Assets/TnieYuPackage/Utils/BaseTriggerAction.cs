using System;

namespace TnieYuPackage.Utils
{
    public abstract class BaseTriggerAction {
        protected Func<bool> ActionEvent;

        public abstract bool TryToAddTrigger(Func<bool> trigger);

        public virtual void RemoveTrigger(Func<bool> trigger)
        {
            ActionEvent -= trigger; 
        }

        public virtual void Invoke()
        {
            if (ActionEvent == null) return;
        
            var delegates = ActionEvent.GetInvocationList();
            if (delegates == null || delegates.Length == 0) return;

            Func<bool> func;
            foreach (var d in delegates)
            {
                func = (Func<bool>)d;
                if (func.Invoke())
                {
                    ActionEvent -= func;
                } 
            }
        }
    }
    
    public abstract class BaseTriggerAction<T1>
    {
        protected Func<T1, bool> ActionEvent;

        public abstract bool TryToAddTrigger(Func<T1, bool> trigger);

        public void RemoveTrigger(Func<T1, bool> trigger)
        {
            ActionEvent -= trigger;
        }

        public void Invoke(T1 arg1)
        {
            if (ActionEvent == null) return;
        
            var delegates = ActionEvent.GetInvocationList();
            if (delegates == null || delegates.Length == 0) return;
        
            Func<T1, bool> func;
            foreach (var d in delegates)
            {
                func = (Func<T1, bool>)d;
                if (func.Invoke(arg1))
                {
                    ActionEvent -= func;
                }
            }
        }
    }
}