// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Audio.Controls;

namespace Synesthesia.Engine.Audio.Effect;

public interface IAudioEffect : IHasAudioHandle
{
    void Detach();

    void ApplyToStream();
}
