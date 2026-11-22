// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Audio;

public interface IPlaybackAudioControl : IAudioControl
{
    bool IsPaused { get; }

    void Pause();

    void Resume();

    void Seek(double time);

    void Seek(long bytes);

}
