using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectReason
{
    /// <summary>
    /// When a disconnect is detected (or expected), set this to provide some context for why it occurred.
    /// </summary>
    public void SetDisconnectReason(ConnectStatus reason)
    {
        //using an explicit setter here rather than the auto-property, to make the code locations where disconnect information is set more obvious.
        Debug.Assert(reason != ConnectStatus.Success);
        Reason = reason;
    }

    /// <summary>
    /// The reason why a disconnect occurred, or Undefined if not set.
    /// </summary>
    public ConnectStatus Reason { get; private set; } = ConnectStatus.Undefined;

    /// <summary>
    /// Clear the DisconnectReason, returning it to Undefined.
    /// </summary>
    public void Clear()
    {
        Reason = ConnectStatus.Undefined;
    }

    /// <summary>
    /// Has a TransitionReason already be set? (The TransitionReason provides context for why someone transition back to the MainMenu, and is a one-use item
    /// that is unset as soon as it is read).
    /// </summary>
    public bool HasTransitionReason => Reason != ConnectStatus.Undefined;
}