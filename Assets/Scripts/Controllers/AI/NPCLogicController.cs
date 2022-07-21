using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCLogicController : MonoBehaviour {
    // Wildlife AI Script
    Rigidbody2D animalBody;
    public int speed, Proximity;
    Vector3 dest;
    public float wanderTimer = 10f;
    private float timer;
    Queue<Node> tilePath;
    private DateController dateController;
    private PathfindingController pathfinding;

    public int range;
    private bool isMoving, destReached, movementAllowed;
    private Vector3 wanderNext;

    private ManagerReferences managerReferences;
    private ControllerManager controller;
    private float timeSpeed;

    private NPC npc;
    public NPC NPC {
        get {
            return npc;
        }
        set {
            npc = value;
        }
    }
    private void Awake() {
        managerReferences = GameObject.Find("MainWorld").GetComponent<ManagerReferences>();
        controller = managerReferences.controllerManager;
        dateController = controller.dateController;
        pathfinding = controller.pathfindingController;
    }
    void Start() {
        //Instantiation of variables and Object references
        timer = Random.Range(0, 10f);
        animalBody = gameObject.GetComponent<Rigidbody2D>();
        tilePath = new Queue<Node>();
        isMoving = false;
        destReached = true;
    }

    private void OnEnable() {
        EventController.StartListening("gameSpeedChange", amendMovementSpeed);
        movementAllowed = true;
    }
    private void OnDisable() {
        EventController.StopListening("gameSpeedChange", amendMovementSpeed);
    }

    // Update is called once per frame
    void FixedUpdate() {
        //Handle the timer for destination setting
        timer += Time.deltaTime;
        //Debug.Log (timeSpeed);
        //Check if the current destination has been reached
        if (destReached) {
            //Set new destination if the timer is greater than 5 seconds
            if (timer > wanderTimer && movementAllowed) {
                setDestination(this.transform.position + new Vector3(Random.Range(-range, range), Random.Range(-range, range)));
                //If the selected node is reachable, begin movement 
                if (tilePath.Count > 0) {
                    wanderNext = tilePath.Peek().worldPosition;
                    destReached = false;
                    isMoving = true;
                    timer = 0;
                }
            }

        } else {
            if (isMoving) {
                //Move towards the next node in the sequence
                if (movementAllowed) {
                    AiFunctions.MoveTowards(animalBody, wanderNext, timeSpeed, 5f);
                    if (animalBody.position.x == wanderNext.x && animalBody.position.y == wanderNext.y) {
                        //Remove movement status if the current node is reached
                        isMoving = false;
                    }
                }
            } else {
                //Check if there are any more nodes in the sequence
                if (tilePath.Count == 0) {
                    //Set the flag for new destitation setting
                    destReached = true;
                } else {
                    //Dequeue the current node, to iterate to the next node
                    isMoving = true;
                    wanderNext = tilePath.Dequeue().worldPosition;
                }
            }

        }
    }

    private void amendMovementSpeed() {
        //Debug.Log (dateController.GameSpeedReturn ());
        timeSpeed = dateController.GameSpeedReturn();
    }

    public void CancelNPCMovement(bool cancel, float timer = 0f) {
        if (cancel) {
            movementAllowed = false;
        } else movementAllowed = true;
    }

    public void setDestination(Vector3 destination) {
        //Clear any previous nodes
        tilePath.Clear();
        //Find a path to the target node using the A* function implemented in the Pathfinding script
        Queue<Node> nodeQueue = new Queue<Node>(pathfinding.FindRoute(this.transform.position, destination));
        if (nodeQueue.Count != 0) tilePath = nodeQueue;
        else return;
        isMoving = true;
    }

    public Vector3 DeterminePosition() {
        return this.transform.position;
    }

}