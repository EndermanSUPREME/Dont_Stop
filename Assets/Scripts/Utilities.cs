namespace Utilities
{
    using System;
    using System.Threading.Tasks;

    using UnityEngine;

    // custom sleep object that can be used to help with sleep operations
    public class Sleep
    {
        float timeTarget = 0;
        float timeElapsed = 0;

        public Sleep(){}
        public Sleep(float t) { timeTarget = t; _ = Update(); }
        async Task Update()
        {
            while (timeElapsed < timeTarget)
            {
                if (!PlayerManager.Instance.gamePaused)
                {
                    timeElapsed += Time.deltaTime;
                }
                await Task.Yield();
            }
        }
        public bool Finished() => timeElapsed >= timeTarget;
    }

    public class RunAfter
    {
        float timeTarget = 0;
        float timeElapsed = 0;
        Action func;
        bool finished;
        bool forceQuit = false;

        public RunAfter(){}
        public RunAfter(float t, Action targetFunction)
        {
            timeTarget = t;
            finished = false;
            func = targetFunction;
            _ = Update();
        }

        async Task Update()
        {
            while (!forceQuit && timeElapsed < timeTarget)
            {
                if (!PlayerManager.Instance.gamePaused)
                {
                    timeElapsed += Time.deltaTime;
                }
                await Task.Yield();
            }
            if (!forceQuit) Run();
            finished = true;
        }
        // execute functions that do not return anything
        void Run() => func();
        public bool Finished() => finished;
        public void Stop()
        {
            forceQuit = true;
        }
    }
}