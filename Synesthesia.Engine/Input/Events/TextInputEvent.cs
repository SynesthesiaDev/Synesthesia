// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Pooling;

namespace Synesthesia.Engine.Input.Events;

public class TextInputEvent : IInputEvent
{
    public void Reset()
    {
        Timestamp = 0f;
        Character = char.MinValue;
    }

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public char Character { get; set; } = char.MinValue;

    public float Timestamp { get; set; }

    public InputSource Source => InputSource.Keyboard;

    public bool IsDown => true;
}
