using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface UiDraggerTarget
{
    public abstract void slotObject(GameObject uiAbil);
    public virtual void unslotObject() { }
}
