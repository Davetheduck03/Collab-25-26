using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TextCurved : MonoBehaviour
{
    public float radius = 150f;
    public bool clockwise = true;

    TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    void OnTextChanged(Object obj)
    {
        if (obj == text)
            UpdateArc();
    }

    void UpdateArc()
    {
        text.ForceMeshUpdate();
        var mesh = text.mesh;
        var verts = mesh.vertices;

        float textWidth = text.preferredWidth;
        float circumference = 2 * Mathf.PI * radius;
        float anglePerUnit = 360f / circumference;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 v = verts[i];
            float percent = v.x / textWidth;
            float angle = percent * textWidth * anglePerUnit;

            if (!clockwise)
                angle = -angle;

            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;

            verts[i] = new Vector3(x, y, v.z);
        }

        mesh.vertices = verts;
        text.canvasRenderer.SetMesh(mesh);
    }
}
