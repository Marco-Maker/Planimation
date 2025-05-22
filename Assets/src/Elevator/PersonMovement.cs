using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonMovement : MonoBehaviour
{
    private bool isMoving = false;
    private Vector3 startPosition;
    private Animator animator;
    void Awake()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isMoving)
        {
            Vector3 direction = (transform.position - startPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));

        }
    }

    public void SetMoving(bool m)
    {
        isMoving = m;
        animator.SetBool("Walk", m);
    }

    public bool GetMoving()
    {
        return isMoving;
    }
}
