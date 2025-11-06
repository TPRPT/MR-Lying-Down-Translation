using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoogleFormLogger : MonoBehaviour
{
    // ✅ /u/0 제거한 formResponse 엔드포인트 사용
    // 지연언니 계정있는 기기용
    private const string BASE_URL =
        "https://docs.google.com/forms/d/e/1FAIpQLSftrQd3WC5TJFcRI97qxFYIPLA6tDTE4-t1dt-0Rv5HwjA8Cw/formResponse";

    // ✅ 본인 폼의 entry.* 키와 정확히 매칭
    private const string F_TRIAL            = "entry.1343484476";
    private const string F_START_ID         = "entry.2065641889";
    private const string F_MANIP_TIME       = "entry.1223258186";
    private const string F_COARSE_TIME      = "entry.1264442913";
    private const string F_REPOSITION_TIME  = "entry.1950498385";
    private const string F_HAND_MOVE_DIST   = "entry.1596669470";
    private const string F_HAND_ROT_ANGLE   = "entry.1766752849";
    private const string F_HEAD_DISTANCE    = "entry.1351673156";
    private const string F_SUCCESS          = "entry.459857123";

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
