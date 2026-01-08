using Raylib_cs;

namespace Synesthesia.Engine.Input;

public record HotKey(KeyboardKey Key, params KeyboardKey[] Modifiers);