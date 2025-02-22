using UnityEngine;

public interface IInteractable
{
    bool buttonClicked {get;}
    string InteractionPrompt { get; } // Property for the interaction prompt
    void Interact(GameObject interactor); // Method for handling interaction
}