using UnityEngine;
using TMPro; // TextMeshPro用

public class CameraPositionController_TMP : MonoBehaviour
{
    public TMP_InputField inputX;
    public TMP_InputField inputY;
    public TMP_InputField inputZ;

    public Transform target; // カメラが向く中心（例: Sphere）

    void Start()
    {
        // 中心ターゲットが設定されていなければ原点を作成
        if (target == null)
        {
            GameObject center = new GameObject("LookAtCenter");
            center.transform.position = Vector3.zero;
            target = center.transform;
        }

        // 🔹 初期位置を (1, 1, 1) に設定
        transform.position = new Vector3(1f, 1f, 1f);

        // 🔹 初期向き
        transform.LookAt(target);

        // 入力イベント登録（入力確定時に実行）
        inputX.onEndEdit.AddListener(delegate { OnInputChanged(); });
        inputY.onEndEdit.AddListener(delegate { OnInputChanged(); });
        inputZ.onEndEdit.AddListener(delegate { OnInputChanged(); });

        // UIにも初期値を表示しておく
        if (inputX != null) inputX.text = transform.position.x.ToString();
        if (inputY != null) inputY.text = transform.position.y.ToString();
        if (inputZ != null) inputZ.text = transform.position.z.ToString();
    }

    void OnInputChanged()
    {
        // 現在の位置
        Vector3 pos = transform.position;

        // 入力値をfloatに変換（空欄なら保持）
        if (float.TryParse(inputX.text, out float x)) pos.x = x;
        if (float.TryParse(inputY.text, out float y)) pos.y = y;
        if (float.TryParse(inputZ.text, out float z)) pos.z = z;

        // カメラ位置を更新
        transform.position = pos;

        // 中心を向く
        transform.LookAt(target);
    }
}
