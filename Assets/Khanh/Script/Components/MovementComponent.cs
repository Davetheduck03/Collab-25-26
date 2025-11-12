using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MovementComponent : UnitComponent
{
    public float movement_Speed;


    protected override void OnInitialize()
    {
        movement_Speed = data.speed;
    }

    public float rayDistance = 0.5f;
    public LayerMask wallLayer;
    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        rb.MovePosition(rb.position + direction * movement_Speed * Time.deltaTime);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayDistance, wallLayer);
        Debug.DrawRay(transform.position, direction * rayDistance, Color.red);
        if (hit.collider != null)
        {
            Flip();
        }
    }

    void Flip()
    {
        direction *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }


}