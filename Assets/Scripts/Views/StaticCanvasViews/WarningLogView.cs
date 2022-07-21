using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WarningLogView : MonoBehaviour {
    public ManagerReferences managerReferences;
    private UiManagement uiManagement;

    public Animator expandedMessagesAnimator;
    private ControllerManager controllerManager;
    public GameObject messagePrefab, messageParent, baseMessage, scrollRect;
    private LinkedList<WarningMessage> warningMessages = new LinkedList<WarningMessage>();
    private List<WarningMessage> decayingMessages = new List<WarningMessage>();
    private List<WarningMessage> pendingMessages = new List<WarningMessage>();
    PointerEventData eventData;
    private int decayTimer;
    public bool debug;
    public int messageLimit;
    // Start is called before the first frame update
    void Start() {
        uiManagement = managerReferences.uiManagement;
        controllerManager = managerReferences.controllerManager;
        eventData = new PointerEventData(EventSystem.current);
        StructureMessageLog();
        expandedMessagesAnimator.gameObject.SetActive(true);
    }

    private void Awake() {
        baseMessage.SetActive(true);
    }

    // Update is called once per frame
    void FixedUpdate() {
        decayTimer += 1;

        // Periodically check to see whether any notifications should expire, and remove them from the log.
        if (decayingMessages.Count > 0) {
            float currentTime = controllerManager.dateController.ReturnCurrentDateTime().rawTime;
            WarningMessage lastMessage = decayingMessages[decayingMessages.Count - 1];

            if (currentTime - lastMessage.startDateTime.rawTime >= lastMessage.duration && decayTimer >= 50) {
                RemoveMessageFromLog(lastMessage);
                decayTimer = 0;
            }
        }

        if (debug && Input.GetKeyDown(KeyCode.RightShift)) {
            AppendMessageToLog(Random.Range(0, 1000).ToString(), new Vector3(-5f, 10f, 1f));
        }
    }

    // Create a warning message object to be displayed from the UI.
    public void AppendMessageToLog(string messageVariable, Vector3 target, float duration = 100f, bool debug = false, GameObject targetObj = null, string[] insertionStrings = null) {
        string message = controllerManager.settingsController.TranslateString(messageVariable);
        if (insertionStrings != null) message = string.Format(message, insertionStrings);
        Debug.Log("Attempting to add message of text " + message);
        if (CheckForMessages(message)) return;
        DateTimeObject startRawTime = controllerManager.dateController.ReturnCurrentDateTime();
        WarningMessage newMessage = new WarningMessage(startRawTime, message, duration, target, debug, targetObj);

        if (newMessage.debug) {
            if (debug) {
                warningMessages.AddFirst(newMessage);
                if (duration != -1) decayingMessages.Add(newMessage);
            }
        } else {
            warningMessages.AddFirst(newMessage);
            if (duration != -1) decayingMessages.Add(newMessage);
        }

        StructureMessageLog();
    }

    private void RemoveMessageFromLog(WarningMessage warningMessage) {
        warningMessages.Remove(warningMessage);
        if (warningMessage.messageGameObject != baseMessage) Destroy(warningMessage.messageGameObject);
        if (warningMessage.duration > -1) decayingMessages.Remove(warningMessage);
        StructureMessageLog();
    }

    private bool CheckForMessages(string message) {
        foreach (WarningMessage warningMessage in warningMessages) {
            if (warningMessage.message == message) return true;
        }
        return false;
    }

    public void ToggleExpandedMessages() {
        GameObject parentPanel = expandedMessagesAnimator.gameObject;
        Animator animator = expandedMessagesAnimator;
        if (animator != null) {
            bool openMenu = animator.GetBool("open");
            Debug.Log("Menu open is now: " + openMenu);
            animator.SetBool("open", !openMenu);
            //scrollRect.SetActive(!openMenu);
        } else Debug.Log("Cannot find animator");
    }

    public void ToggleScreenPan(WarningMessage warningMessage) {
        Debug.Log("Warning button is pressed");
        if (warningMessage.targetGameObject == null) uiManagement.scrollView.PanToLocation(warningMessage.realWorldPosition);
        else uiManagement.scrollView.PanToLocation(warningMessage.targetGameObject.transform.position);

    }

    public void FormatWarningItem(WarningMessage warningMessage = null) {
        SettingsController settings = controllerManager.settingsController;
        if (warningMessage != null) {
            Debug.Log(warningMessage.messageGameObject.name);
            TextMeshProUGUI warningText = warningMessage.messageGameObject.GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log("Setting Text to " + warningMessage.message);
            warningText.SetText(warningMessage.message);
            UnityAction leftClick = new UnityAction(() => ToggleScreenPan(warningMessage));
            UnityAction rightClick = new UnityAction(() => RemoveMessageFromLog(warningMessage));
            warningMessage.messageGameObject.GetComponent<RightClickEvent>().SetEvents(leftClick, rightClick);
            if (warningMessage.message.Length >= 25) warningText.fontSize = 20;
            else warningText.fontSize = 25;
        } else {
            baseMessage.GetComponentInChildren<TextMeshProUGUI>().SetText(settings.TranslateString("NoWarningLogItems"));
            baseMessage.GetComponent<RightClickEvent>().SetEvents();
        }

    }

    private void StructureMessageLog() {
        Debug.Log("Structuring " + warningMessages.Count + " messages in log.");
        // If there are no messages to display, clear the log.
        if (warningMessages.Count == 0) {
            FormatWarningItem();
            return;
        }

        // Set the base message to display the most recent warning message and remove old log.
        WarningMessage firstMessage = warningMessages.First.Value;
        if (firstMessage.messageGameObject != baseMessage) {
            Destroy(firstMessage.messageGameObject);
        }
        firstMessage.messageGameObject = baseMessage;
        FormatWarningItem(firstMessage);

        messageParent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 70 * (warningMessages.Count - 1));
        foreach (WarningMessage warningMessage in warningMessages) {
            if (warningMessages.First.Value == warningMessage || warningMessage.messageGameObject != baseMessage) continue;
            warningMessage.messageGameObject = GameObject.Instantiate(messagePrefab, Vector3.zero, Quaternion.identity, messageParent.transform);
            warningMessage.messageGameObject.name = warningMessage.message;
            FormatWarningItem(warningMessage);
        }
        string debugText = "ProperListOrder = {";
        foreach (WarningMessage warning in warningMessages) {
            debugText += warning.message + ",";
        }
        debugText = debugText.Substring(0, debugText.Length - 1);
        debugText += "}";
        Debug.Log(debugText);
        // Set the base message, which is always displayed

    }

    public class WarningMessage {
        public GameObject messageGameObject;
        public GameObject targetGameObject;
        public Vector3 realWorldPosition;
        public DateTimeObject startDateTime;
        public float duration;
        public string message;
        public bool debug;

        public WarningMessage(DateTimeObject _startTime, string _message, float _duration, Vector3 _realWorldPosition, bool _debug = false, GameObject _gameObject = null) {
            targetGameObject = _gameObject;
            realWorldPosition = _realWorldPosition;
            startDateTime = _startTime;
            message = _message;
            duration = _duration;
            debug = _debug;
        }

    }
}