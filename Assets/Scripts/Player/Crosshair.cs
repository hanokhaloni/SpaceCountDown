using UnityEngine;

public class Crosshair : MonoBehaviour
{
    SpriteRenderer sr;
    Camera cam;

    public void Init(Sprite sprite, Color color)
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : Shapes.Circle(Color.white);
        sr.color = color;
        sr.sortingOrder = 50;
        transform.localScale = Vector3.one * 0.3f;

        cam = Camera.main;
        Cursor.visible = false;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        transform.position = mouseWorld;
    }

    void OnDestroy()
    {
        Cursor.visible = true;
    }
}
