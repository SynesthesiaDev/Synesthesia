// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.Engine.Input.Events;

public interface IInputEvent : IPooledObject
{
    InputEventSource Source { get; }
    long Timestamp { get; }

    bool IsDown { get; }

    bool IsPrimary()
    {
        return this switch
        {
            KeyboardInputEvent keyInputEvent => keyInputEvent.Key == Key.Enter,
            MouseButtonInputEvent mouseInputEvent => mouseInputEvent.Button == MouseButton.Left,
            _ => false,
        };
    }

    void ReturnToPool();
}
