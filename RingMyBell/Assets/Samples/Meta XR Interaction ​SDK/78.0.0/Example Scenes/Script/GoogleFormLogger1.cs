using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoogleFormLogger1 : MonoBehaviour
{
    // ✅ /u/0 제거한 formResponse 엔드포인트 사용
    // 지연언니 계정있는 기기용
    private const string BASE_URL =
        "https://docs.google.com/forms/d/e/1FAIpQLScaS4kPk948vA8TTvgfefreuw88kJmi2_xx5de9-K56C8hkqA/formResponse";

    // ✅ 본인 폼의 entry.* 키와 정확히 매칭
    private const string F_TRIAL            = "entry.935044011";
    private const string F_START_ID         = "entry.1974380846";
    private const string F_MANIP_TIME       = "entry.732245233";
    private const string F_COARSE_TIME      = "entry.513025879";
    private const string F_REPOSITION_TIME  = "entry.1352978043";
    private const string F_HAND_MOVE_DIST   = "entry.1686857216";
    private const string F_HAND_ROT_ANGLE   = "entry.674085472";
    private const string F_HEAD_DISTANCE    = "entry.79567436";
    private const string F_SUCCESS          = "entry.1148993313";

    public IEnumerator PostTrialData(
        int trial, int startID,
        float manipulationTime, float coarseTime, float repositionTime,
        float handMoveDist, float handRotAngle,
        float headDistance, int success
    )
    {
        WWWForm form = new WWWForm();
        form.AddField(F_TRIAL,           trial.ToString());
        form.AddField(F_START_ID,        startID.ToString());
        form.AddField(F_MANIP_TIME,      manipulationTime.ToString("F3"));
        form.AddField(F_COARSE_TIME,     coarseTime.ToString("F3"));
        form.AddField(F_REPOSITION_TIME, repositionTime.ToString("F3"));
        form.AddField(F_HAND_MOVE_DIST,  handMoveDist.ToString("F3"));
        form.AddField(F_HAND_ROT_ANGLE,  handRotAngle.ToString("F3"));
        form.AddField(F_HEAD_DISTANCE,   headDistance.ToString("F3"));
        form.AddField(F_SUCCESS,         success.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success || www.responseCode == 200)
            {
                Debug.Log("[GoogleFormLogger] Uploaded.");
            }
            else
            {
                Debug.LogError($"[GoogleFormLogger] Upload error: {www.error}, code={www.responseCode}, body={www.downloadHandler.text}");
            }
        }
    }
}
