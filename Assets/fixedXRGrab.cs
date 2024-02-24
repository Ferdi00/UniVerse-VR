using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class fixedXRGrab : XRGrabInteractable
{
    [SerializeField] private Transform left;
    [SerializeField] private Transform right;
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag("LeftHand"))
        {
            attachTransform = left;
        }
        if (args.interactableObject.transform.CompareTag("RightHand"))
        {
            attachTransform = right;
        }
        base.OnSelectEntered(args);
    }
}
