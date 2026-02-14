// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Pooling;
using Raylib_cs;

namespace Synesthesia.Engine.Input.Events;

public class KeyInputEvent : IInputEvent
{
    public void Reset()
    {
        Timestamp = 0f;
        IsDown = false;
    }

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public KeyboardKey Key { get; set; } = KeyboardKey.Null;

    public InputSource Source => InputSource.Keyboard;

    public float Timestamp { get; set; } = 0f;

    public bool IsDown { get; set; } = false;
}
