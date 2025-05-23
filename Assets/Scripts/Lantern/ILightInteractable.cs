using UnityEngine;

/// <summary>
/// Interface for objects that can interact with the lantern's light
/// </summary>
public interface ILightInteractable
{
    /// <summary>
    /// Called when this object enters the lantern's light beam
    /// </summary>
    /// <param name="lantern">The lantern controller that is illuminating this object</param>
    void OnIlluminated(LanternController lantern);

    /// <summary>
    /// Called when this object leaves the lantern's light beam
    /// </summary>
    /// <param name="lantern">The lantern controller that was illuminating this object</param>
    void OnLeftLight(LanternController lantern);

    /// <summary>
    /// Whether this object is currently being illuminated
    /// </summary>
    bool IsIlluminated { get; }
}