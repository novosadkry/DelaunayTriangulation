using System;
using UnityEditor;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class PointHandler : MonoBehaviour
{
    public Point point;

    private SpriteRenderer _spriteRenderer;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        point.Position = transform.position;
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

    private void OnDrawGizmos()
    {
        Handles.Label(transform.position, name);
    }
}
