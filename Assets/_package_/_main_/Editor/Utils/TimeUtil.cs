using System;
using System.Threading.Tasks;

namespace UPMTool
{
    public class TimeUtil
    {
        public static Func<float, Action, Task> DoActionWaitAtTime = async (time, action) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(time));
            action();
        };

        /// <summary>
        /// 等待直到条件成立
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static async Task WaitUntilConditionSet(TaskCondition condition, int millisecondsDelay = 100)
        {
            while (condition.Value == false)
            {
                await Task.Delay(millisecondsDelay);
            }
        }
    }
    
    public class TaskCondition
    {
        public bool Value;

        public TaskCondition()
        {
            Value = false;
        }

        public TaskCondition(bool value)
        {
            Value = value;
        }
    }
}