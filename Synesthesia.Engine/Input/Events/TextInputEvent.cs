// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.Engine.Input.Events;

public class TextInputEvent : IInputEvent
{
    public void Reset()
    {
        Timestamp = 0;
        Text = string.Empty;
    }

    public string Text { get; set; } = string.Empty;

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public InputEventSource Source => InputEventSource.Keyboard;

    public long Timestamp { get; set; }

    public bool IsDown => true;

    public void ReturnToPool() => ReturnAction?.Invoke(this);

    public override string ToString()
    {
        return $"TextInputEvent(Text={Text},IsPooled={IsPooled},Timestamp={Timestamp})";
    }
}
