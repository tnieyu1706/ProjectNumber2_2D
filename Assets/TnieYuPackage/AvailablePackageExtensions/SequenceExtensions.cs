using System;
using DG.Tweening;
using UnityEngine;

namespace TnieYuPackage.AvailablePackageExtensions
{
    public static class SequenceExtensions
    {
        public static Sequence IntervalAction(this Sequence sequence, Action action, float duration, float delay)
        {
            int repeatTimes = Mathf.RoundToInt(duration / delay);
            for (int i = 0; i < repeatTimes; i++)
            {
                sequence.AppendCallback(action.Invoke);
                sequence.AppendInterval(delay);
            }

            return sequence;
        }
    }
}