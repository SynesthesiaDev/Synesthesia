// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Raylib_cs;
using Synesthesia.Engine.Graphics;

namespace Synesthesia.Engine.Input;

public static class InputSimulator
{
    private static DragSimulation? currentDragSimulation = null;

    public static bool SimulatingInput => currentDragSimulation != null;

    public static void SimulateMove(Vector2 position)
    {
        InputManager.EnqueueEvent(new MouseMoveInputEvent(position));
    }

    public static void SimulateClickState(MouseButton mouseButton, bool down)
    {
        InputManager.EnqueueEvent(new MouseButtonInputEvent(mouseButton, down));
    }

    public static void SimulateClick(MouseButton mouseButton = MouseButton.Left)
    {
        SimulateClickState(mouseButton, true);
        SimulateClickState(mouseButton, false);
    }

    public static void SimulateDrag(Vector2 start, Vector2 end, double time, MouseButton mouseButton = MouseButton.Left)
    {
        if (time < 0) throw new ArgumentException("Time must be non-negative", nameof(time));

        currentDragSimulation = new DragSimulation(start, end, time, mouseButton);
    }

    public static void Update(FrameInfo frameInfo)
    {
        var delta = frameInfo.Delta;

        if (currentDragSimulation == null) return;

        if (!currentDragSimulation.Started)
        {
            // Start the drag
            SimulateMove(currentDragSimulation.Start);
            SimulateClickState(currentDragSimulation.Button, true);
            currentDragSimulation.Started = true;
        }

        currentDragSimulation.Elapsed += delta;

        if (currentDragSimulation.IsComplete())
        {
            // Complete the drag
            SimulateMove(currentDragSimulation.End);
            SimulateClickState(currentDragSimulation.Button, false);
            currentDragSimulation = null;
        }
        else
        {
            // Update position during drag
            var currentPosition = currentDragSimulation.GetCurrentPosition();
            SimulateMove(currentPosition);
        }
    }

    private class DragSimulation(Vector2 start, Vector2 end, double duration, MouseButton button)
    {
        public Vector2 Start { get; } = start;
        public Vector2 End { get; } = end;
        public double Duration { get; } = duration;
        public MouseButton Button { get; } = button;
        public double Elapsed { get; set; } = 0;
        public bool Started { get; set; } = false;

        public Vector2 GetCurrentPosition()
        {
            if (Duration <= 0) return End;
            var t = Math.Min(Elapsed / Duration, 1.0);
            return Vector2.Lerp(Start, End, (float)t);
        }

        public bool IsComplete() => Elapsed >= Duration;
    }
}
