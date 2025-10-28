using UnityEngine;

public class Timer
{
    // The time that each repeat should last
    private float currentTime;
    private float resetTime;

    // The amount of times the timer should repeat
    private float repeatCount;
    private float repeats;

    // Is the timer currently running
    public bool isRunning { get; private set; }

    // Callbacks that can trigger methods in other classes
    public delegate void CallbackDelegate();
    private CallbackDelegate endCallback;
    private CallbackDelegate tickCallback;
    
    public Timer(CallbackDelegate tickCallback = null, CallbackDelegate endCallback = null)
    {
        this.tickCallback = tickCallback;
        this.endCallback = endCallback;
    }

    public void Update(float dt)
    {
        if (isRunning)
        {
            // Check if timer should end
            if (currentTime > 0)
                currentTime -= dt;
            else
            {
                // Check if this timer should repeat itself, and if so, do that
                if(repeatCount > 0)
                {
                    currentTime = resetTime;
                    repeatCount--;
                    tickCallback?.Invoke();
                }
                else
                {
                    isRunning = false;
                    tickCallback?.Invoke();
                    endCallback?.Invoke();
                }
            }
        }
    }

    public void Start(float time, int repeats)
    {
        // Start the timer fresh with new inputs
        resetTime = time;
        currentTime = resetTime;
        this.repeats = repeats;
        repeatCount = repeats;

        isRunning = true;
    }
    public void Stop()
    {
        currentTime = 0;
        repeatCount = 0;
        isRunning = false;
    }
    public void Restart()
    {
        // Restart the timer from where it was started
        currentTime = resetTime;
        repeatCount = repeats;
        isRunning = true;
    }

    public void Pause()
    {
        isRunning = false;
    }
    public void Resume()
    {
        isRunning = true;
    }
}
