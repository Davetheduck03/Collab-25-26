using UnityEngine;

public class Core
{
    public Rigidbody2D Rb2D { get; private set; }
    public Animator Anim { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public Collider2D Collider2D { get; private set; }

    public Core(Rigidbody2D _rb2d = null, Animator _anim = null, SpriteRenderer _sr = null, Collider2D _collider2D = null)
    {
        Rb2D = _rb2d;
        Anim = _anim;
        SpriteRenderer = _sr;
        Collider2D = _collider2D;

        if(_rb2d == null)
            Debug.Log("No _rb2d");

        if(_anim == null)
            Debug.Log("No _anim");

        if(_sr == null)
            Debug.Log("No _sr");
    }
}