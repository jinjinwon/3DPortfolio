using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Test/EntityMovementTest")]
[RequireComponent(typeof(EntityMovement))]
public class EntityMovementTest : MonoBehaviour
{
    [Header("Roll")]
    public float rollDistance;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
            GetComponent<EntityMovement>().Roll(rollDistance);
    }
}
