using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class CurvedText : MonoBehaviour
{
    [SerializeField] AnimationCurve curve = new AnimationCurve(
        new Keyframe(0f, 0f),      // trái thấp
        new Keyframe(0.5f, 0.5f),    // giữa cao
        new Keyframe(1f, 0f)       // phải thấp
    );
    [SerializeField] float curveMultiplier = 12f;
    [SerializeField] float tiltMultiplier = 0.2f; // thử 0.5 hoặc thấp hơn

    private TMP_Text text;
    private Mesh mesh;
    private Vector3[] vertices;

    void OnEnable()
    {
        text = GetComponent<TMP_Text>();
        text.ForceMeshUpdate();
        mesh = text.mesh;
        ApplyCurve();
    }

    void ApplyCurve()
    {
        text.ForceMeshUpdate();
        var textInfo = text.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            Vector3[] verts = textInfo.meshInfo[materialIndex].vertices;

            // Tính tâm của ký tự
            Vector3 charMid = (verts[vertexIndex] + verts[vertexIndex + 2]) / 2;
            float x0 = charMid.x / text.rectTransform.rect.width + 0.5f;

            // Tính độ cao offset dựa theo curve
            float offsetY = curve.Evaluate(x0) * curveMultiplier;

            // Tính độ dốc tại x0 (dùng để xác định góc xoay)
            float dx = 0.001f;
            float dy = curve.Evaluate(x0 + dx) - curve.Evaluate(x0 - dx);
            float angle = Mathf.Atan2(dy, 2 * dx) * Mathf.Rad2Deg * tiltMultiplier;
            // Tạo ma trận dịch chuyển + xoay + trở lại
            Matrix4x4 matrix = Matrix4x4.TRS(
                new Vector3(0, offsetY, 0),
                Quaternion.Euler(0, 0, angle),
                Vector3.one
            );

            for (int j = 0; j < 4; j++)
            {
                verts[vertexIndex + j] -= charMid;
                verts[vertexIndex + j] = matrix.MultiplyPoint3x4(verts[vertexIndex + j]);
                verts[vertexIndex + j] += charMid;
            }
        }

        // Cập nhật lại mesh
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
