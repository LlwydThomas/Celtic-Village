 using System.Collections;
 using UnityEngine.Events;
 using UnityEngine.EventSystems;
 using UnityEngine.UI;
 using UnityEngine;

 [AddComponentMenu("Event/ButtonClickEvent")]
 public class RightClickEvent : MonoBehaviour, IPointerClickHandler {
     // Start is called before the first frame update
     public UnityAction onLeftClick, onRightClick;
     private bool isOver = false;

     public void OnPointerClick(PointerEventData eventData) {
         if (eventData.button == PointerEventData.InputButton.Left) {
             if (onLeftClick != null) {
                 onLeftClick.Invoke();
             }
         } else if (eventData.button == PointerEventData.InputButton.Right) {
             if (onRightClick != null) {
                 onRightClick.Invoke();
             }
         }
     }

     public void SetEvents(UnityAction leftClick = null, UnityAction rightClick = null) {
         onLeftClick = leftClick;
         onRightClick = rightClick;
     }
 }