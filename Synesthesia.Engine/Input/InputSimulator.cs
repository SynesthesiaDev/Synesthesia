// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Raylib_cs;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Input.Events;

namespace Synesthesia.Engine.Input;

public static class InputSimulator
{
    private static DragSimulation? currentDragSimulation;

    private static TypingSimulation? currentTypingSimulation;

    public static bool SimulatingInput => currentDragSimulation != null;

    public static void SimulateKeyboard(KeyboardKey key, bool isDown)
    {
        InputManager.EnqueueEvent(new KeyInputEvent
        {
            Key = key,
            IsDown = isDown,
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        });
    }

    public static void SimulateKeyboardPress(KeyboardKey key)
    {
        SimulateKeyboard(key, true);
        SimulateKeyboard(key, false);
    }

    public static void SimulateMove(Vector2 position)
    {
        var mouseEvent = new MouseMoveInputEvent
        {
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Position = position,
            PositionDelta = Vector2.Zero
        };

        InputManager.EnqueueEvent(mouseEvent);
    }

    public static void SimulateTextInput(char character)
    {
        InputManager.EnqueueEvent(new TextInputEvent
        {
            Character = character,
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        });
    }

    public static void SimulateTyping(string text, double time)
    {
        if (time < 0) throw new ArgumentException("Time must be non-negative", nameof(time));

        currentTypingSimulation = new TypingSimulation(text, time);
    }

    public static void SimulateClickState(MouseButton mouseButton, bool down)
    {
        InputManager.EnqueueEvent(new MouseButtonInputEvent
        {
            Button = mouseButton,
            IsDown = down,
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        });
    }

    public static void SimulateClick(MouseButton mouseButton, Vector2 position)
    {
        SimulateMove(position);
        SimulateClickState(mouseButton, true);
        SimulateClickState(mouseButton, false);
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

        if (currentTypingSimulation != null)
        {
            if (!currentTypingSimulation.Started)
            {
                currentTypingSimulation.Started = true;
            }

            currentTypingSimulation.Elapsed += delta;

            if (currentTypingSimulation.IsComplete())
            {
                currentTypingSimulation = null;
            }
            else
            {
                var next = currentTypingSimulation.GetNextCharIfChanged();
                if(next != null) SimulateTextInput(next.Value);
            }
        }

        if (currentDragSimulation != null)
        {
            if (!currentDragSimulation.Started)
            {
                SimulateMove(currentDragSimulation.Start);
                SimulateClickState(currentDragSimulation.Button, true);
                currentDragSimulation.Started = true;
            }

            currentDragSimulation.Elapsed += delta;

            if (currentDragSimulation.IsComplete())
            {
                SimulateMove(currentDragSimulation.End);
                SimulateClickState(currentDragSimulation.Button, false);
                currentDragSimulation = null;
            }
            else
            {
                var currentPosition = currentDragSimulation.GetCurrentPosition();
                SimulateMove(currentPosition);
            }
        }
    }

    private class TypingSimulation(string text, double duration)
    {
        public string Target { get; set; } = text;

        public double Elapsed { get; set; }

        public bool Started { get; set; }

        private int lastIndex = -1;
        public double Duration { get; } = duration;

        public bool IsComplete() => Elapsed >= Duration;

        public char? GetNextCharIfChanged()
        {
            if (string.IsNullOrEmpty(Target)) return null;

            float time = (float)(Elapsed / Duration);
            int currentIndex = Math.Clamp((int)(Target.Length * time), 0, Target.Length - 1);

            if (currentIndex > lastIndex)
            {
                lastIndex = currentIndex;
                return Target[currentIndex];
            }

            return null;
        }
    }

    private class DragSimulation(Vector2 start, Vector2 end, double duration, MouseButton button)
    {
        public Vector2 Start { get; } = start;
        public Vector2 End { get; } = end;
        public double Duration { get; } = duration;
        public MouseButton Button { get; } = button;
        public double Elapsed { get; set; }
        public bool Started { get; set; }

        public Vector2 GetCurrentPosition()
        {
            if (Duration <= 0) return End;
            var t = Math.Min(Elapsed / Duration, 1.0);
            return Vector2.Lerp(Start, End, (float)t);
        }

        public bool IsComplete() => Elapsed >= Duration;
    }
}
