using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldMovement : MonoBehaviour
{
    // Variables
    public float speed = 10f;
    public GenerationManager generationManager;
    public UIDocument tilesDoc;

    [HideInInspector]
    public Vector2 direction = Vector2.zero;

    // References
    private Controls controls;

    // UI
    private VisualElement root;
    private Button resetWorldPosButton;

    void Start()
    {
        // Cache
        controls = new Controls();

        // UI
        root = tilesDoc.rootVisualElement;
        resetWorldPosButton = root.Q<Button>("ResetWorldPosButton");

        // UI Taps
        resetWorldPosButton.clicked += () => ResetWorldPos();

        controls.Enable();
    }

    void Update()
    {
        if (!generationManager.useUpdateForMovementChecking && generationManager.initialChunksGenerated)
        {
            MoveWorld();
        }
    }

    void MoveWorld()
    {
        if (controls.Default.Movement.IsPressed())
        {
            direction = controls.Default.Movement.ReadValue<Vector2>();

            transform.Translate(-direction * speed * Time.deltaTime);

            generationManager.OnWorldMoved(transform.position);
        }
        else
        {
            direction = Vector2.zero;
        }
    }

    public void ResetWorldPos()
    {
        transform.position = Vector3.zero;

        generationManager.OnWorldMoved(Vector2.zero);
    }
}
