# Phase 4: Gameplay Loop (Playback Script)

## Goal
Translate server packets into a visual and auditory sequence of events.

## What to do
1.  **Packet 91 Parser**: Decode the list of actions sent by the server during a turn.
2.  **Action Queue**: Create a manager that executes these actions sequentially.
3.  **Audio Integration**: Trigger sounds at specific points in the action playback.

## How to do it
*   **The Action Class**: Create an abstract `BaseAction` in Godot C#.
    *   `MoveAction`: Moves a sprite.
    *   `BombAction`: Handles explosion visuals and calls `SoundManager.Play("bomb_id")`.
    *   `ChangeHealthAction`: Updates HP bars, shows floating numbers, and plays "Hurt" sounds.
*   **The Controller**: Create a `BattleController` that holds a `Queue<BaseAction>`.
    *   In the `_Process` loop, if no action is currently running, pop the next action and start it.
    *   Use `await ToSignal(animation_player, "animation_finished")` to ensure the next action only starts when the current one's visuals AND sounds are complete.
*   **Synchronization**: Ensure that the "Turn End" packet is only processed after all visual/audio actions in the queue are finished.

## What to expect
*   **Verification**: When a bomb explodes, the visual effect and the sound effect should trigger simultaneously according to the server's script.
*   **Success Metric**: Visual and audio state matches the Server state after every turn.