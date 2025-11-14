using UnityEngine;

public class Timer
{
    // The time that each repeat should last
    private float currentTime;
    private float resetTime;

    // The amount of times the timer should repeat
    private float currentRepeats;
    private float resetRepeats;

    // Is the timer currently running
    public bool isRunning { get; private set; }

    // Callbacks that can trigger methods in other classes
    public delegate void CallbackDelegate();
    private CallbackDelegate beginCallback;
    private CallbackDelegate endCallback;
    private CallbackDelegate tickCallback;
    
    public Timer(CallbackDelegate beginCallback = null, CallbackDelegate tickCallback = null, CallbackDelegate endCallback = null)
    {
        this.beginCallback = beginCallback;
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
                // Works like this to allow -1 to make timer run infinitely
                if(currentRepeats != 0)
                {
                    currentTime = resetTime;
                    currentRepeats--;
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

    public void Start(float time, int repeats, bool overrideTimer = false)
    {
        // Check to see if the timer should be overriden to restart
        if(!isRunning || overrideTimer)
        {
            // Start the timer fresh with new inputs
            resetTime = time;
            currentTime = resetTime;
            this.resetRepeats = repeats;
            currentRepeats = repeats;

            isRunning = true;
        }
        else
        {
            // This will add the next timer to the current one to avoid skipping the proc time
            resetTime = time;
            this.resetRepeats = repeats;

            // This accounts for the one that is currently running
            currentRepeats = repeats + 1;
        }

        beginCallback?.Invoke();
    }
    public void Stop()
    {
        currentTime = 0;
        currentRepeats = 0;
        isRunning = false;
    }
    public void Restart()
    {
        // Restart the timer from where it was started
        currentTime = resetTime;
        currentRepeats = resetRepeats;
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

    /// <summary>
    /// Gets the amount of time remaining on this timer including all ticks
    /// </summary>
    /// <returns>The amount of time remaining</returns>
    public float GetRemainingTime()
    {
        // If this source is set to infinite loop, return infinity
        if (resetRepeats == -1)
            return float.PositiveInfinity;

        // Otherwise, return the current tick's remaining time plus all additional ticks' times
        return currentTime + (currentRepeats * resetTime);
    }
}
