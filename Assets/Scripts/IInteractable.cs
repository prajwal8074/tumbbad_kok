using UnityEngine;

public interface IInteractable
{
    bool buttonDown {get;}
    bool buttonPressed {get;}
    bool buttonUp {get;}
    string InteractionPrompt { get; } // Property for the interaction prompt
    void Interact(GameObject interactor); // Method for handling interaction
}