﻿/// <summary>
/// The interface, implementing the necessary properties for the controls
/// </summary>

public interface IInput
{
    float Horizontal { get; }

    float Vertical { get; }

    int Jump { get; }

    bool Shout { get; }

    bool Crouch { get; }

    bool Interact { get; }
}
