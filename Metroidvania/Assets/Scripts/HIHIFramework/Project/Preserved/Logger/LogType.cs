using System;

[Flags]
public enum LogType : long {

    // Remarks :
    // For long enum, at most 64 flags enum values can be set. The 0th to 19th is preserved by framework
    None = 0,
    General = 1 << 0,        // From Framework. Do not remove
    Input = 1 << 1,          // From Framework. Do not remove
    UI = 1 << 2,             // From Framework. Do not remove
    Animation = 1 << 3,      // From Framework. Do not remove
    Asset = 1 << 4,          // From Framework. Do not remove
    Audio = 1 << 5,          // From Framework. Do not remove
    Collision = 1 << 6,      // From Framework. Do not remove
    GameFlow = 1 << 7,       // From Framework. Do not remove
    Lang = 1 << 8,           // From Framework. Do not remove
    IO = 1 << 9,             // From Framework. Do not remove
    UserData = 1 << 10,      // From Framework. Do not remove

    MapData = 1 << 20,
    Life = 1 << 21,
    Char = 1 << 22,
    Enemy = 1 << 23,
}