using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Point : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseUp()
    {
        _spriteRenderer.color = Color.white;
    }

    private void OnMouseDown()
    {
        _spriteRenderer.color = Color.gray;
    }

    private void OnMouseDrag()
    {
        var screenPos = Input.mousePosition;
        transform.position = (Vector2) _camera.ScreenToWorldPoint(screenPos);
    }

    public override bool Equals(object other)
    {
        if (!(other is Point p)) return false;
        return transform.position.Equals(p.transform.position);
    }
}
