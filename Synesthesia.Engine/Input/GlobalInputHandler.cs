// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using SDL3;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Logging;

namespace Synesthesia.Engine.Input;

public static class GlobalInputHandler
{
    public static void HandleKeyboardInput(SDL.KeyboardEvent keyboardEvent)
    {
        var key = keyboardEvent.ToKey();
        if (keyboardEvent.Down)
        {
            Logger.Verbose($"Key Down - {key}");
        }
        else
        {
            Logger.Verbose($"Key Up - {key}");
        }

    }
}
