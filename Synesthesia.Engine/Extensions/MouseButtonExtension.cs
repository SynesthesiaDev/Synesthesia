// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Extensions;

public static class MouseButtonExtension
{
    public static bool IsDown(this MouseButton button) => InputHandler.IsMouseButtonDown(button);
}
