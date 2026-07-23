using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warpfloor : MonoBehaviour
{
    [Header("ワープ先候補のオブジェクトをここに登録")]
    public Transform[] warpPoints;

    private void OnTriggerEnter(Collider other)
    {
        // 触れたオブジェクトがプレイヤーかチェック
        if (other.CompareTag("Player"))
        {
            // 安全対策：もし候補地が1つも登録されていなかったら処理を中断する
            if (warpPoints == null || warpPoints.Length == 0)
            {
                Debug.LogWarning("ワープ先（Warp Points）が登録されていません！");
                return;
            }

            // 0 から「候補地の数 - 1」の間で、ランダムな数字（インデックス）を1つ選ぶ
            int randomIndex = Random.Range(0, warpPoints.Length);

            // 選ばれた番号のワープ先の位置（Transform）を取得
            Transform targetPoint = warpPoints[randomIndex];

            // プレイヤーの座標を、選んだワープ先の座標に瞬間移動させる
            other.transform.position = targetPoint.position;

            Debug.Log(targetPoint.name + " にワープしました！");
        }
    }
}
