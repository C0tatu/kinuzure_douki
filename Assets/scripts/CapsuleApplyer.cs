
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CapsuleApplyer : UdonSharpBehaviour
{

    public int playerID = 0;
    private VRCPlayerApi player;
    private GameObject[] Capsules;
    private int[] boneCapsuleDict;
    int[] antiboneCapsuleDict;

    //速度計算用
    public Vector3[] CapsuleVelocities;
    public Vector3[] CapsuleAngVelocities;
    private Vector3[] CapsulesPreviousPos;
    private Quaternion[] capsulesPreviousRotation;


    void Start()
    {
    }

    private void FixedUpdate()
    {
        if(playerID != 0)
        {
            FollowCapsules();
            CapsuleRecorder();
        }
    }

    /// <summary>
    /// 初期設定用関数
    /// </summary>
    public void Constructor(int pid, GameObject[] Capsules)
    {
        player = VRCPlayerApi.GetPlayerById(pid);
        this.Capsules = Capsules;
        CapsuleVelocities = new Vector3[Capsules.Length];
        CapsuleAngVelocities = new Vector3[Capsules.Length];
        CapsulesPreviousPos = new Vector3[Capsules.Length];
        capsulesPreviousRotation = new Quaternion[Capsules.Length];
        boneCapsuleDict = CreateBoneToCapsuleDict(Capsules);

        //antiboneCapsuleDict = CreateAntiBoneToCapsuleDict(boneCapsuleDict);
        playerID = pid;
    }

    /// <summary>
    /// カプセルを追従させる
    /// </summary>
    private void FollowCapsules()
    {
        for(int i=0; i<(int)HumanBodyBones.LeftToes; i++)
        {
            //Enum内のi番目のbone
            HumanBodyBones bone = (HumanBodyBones)i;

            //座標と回転を取得
            Vector3 pos = player.GetBonePosition(bone);
            Quaternion ang = player.GetBoneRotation(bone);

            //カプセル表示
            /*Debug.Log(i + " " + bone + " " + pos.ToString());*/
            Vector3 diff = (pos == new Vector3(0, 0, 0)) ? new Vector3(1, 0, 0) : new Vector3(1, 0, 0);//デバッグ用の右にずらすやつ
            Vector3 posDiff = new Vector3(0, 0, 0);
            Quaternion angleDiff = Quaternion.identity;

            //部位ごとの場合分け             boneの根本にカプセルを出しても意味ないからboneの中央に出るようにするよ
            Quaternion zRotation = Quaternion.AngleAxis(90f, Vector3.forward); //z軸90度
            GameObject obj = Capsules[boneCapsuleDict[i]]; //いまいじってるカプセル
            float moveScale = 1f; //ずらす距離

            switch (bone.ToString())
            {
                case "Hips":
                    angleDiff = zRotation;
                    moveScale = obj.transform.localScale.z;
                    posDiff = ang * Vector3.down * (0.5f * moveScale);
                    break;
                case "Spine":
                    angleDiff = zRotation;

                    break;
                case "Chest":
                    angleDiff = zRotation;
                    moveScale = obj.transform.localScale.z;
                    posDiff = ang * Vector3.up * (0.5f * moveScale);
                    break;
                case "Neck":
                case "LeftHand":
                case "RightHand":
                case "LeftFoot":
                case "RightFoot":
                    //正直消してもいいけどいったんおいとく
                    break;
                default:
                    moveScale = obj.transform.localScale.y;
                    posDiff = ang * Vector3.up * (1f * moveScale);
                    break;

            }

            Capsules[boneCapsuleDict[i]].transform.position = (pos + posDiff);
            Capsules[boneCapsuleDict[i]].transform.rotation = ang * angleDiff;
        }
    }

    /// <summary>
    /// カプセル配列とHumanBodyBonesのindexが異なる可能性を考えて辞書を作る関数
    /// </summary>
    private int[] CreateBoneToCapsuleDict(GameObject[] arr)
    {
        int[] ret = new int[(int)HumanBodyBones.LeftToes];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = -1;
        }
        for (int i = 0; i < arr.Length; i++) {
            string name = arr[i].name;

            for (int j = 0; j < (int)HumanBodyBones.LeftToes; j++)
            {
                if(name == ((HumanBodyBones)j).ToString())
                {
                    ret[j] = i;
                }
            }

        }
        return ret;
    }


    private int[] CreateAntiBoneToCapsuleDict(int[] arr)
    {
        Debug.Log($"arr: {arr.Length}");
        int[] ret = new int[Capsules.Length];
        for (int i = 0; i < ret.Length; i++) {
            ret[i] = -1;
        }
        
        for(int i=0; i<arr.Length; i++)
        {
            Debug.Log($"i: {i}, arr[i]: {arr[i]}");    
            if (arr[i] != -1)
            {
                ret[arr[i]] = i;
            }
        }
        return ret;
    }
    /// <summary>
    /// カプセルの速度とかを残しておく
    /// </summary>
    private void CapsuleRecorder()
    {
        float deltaTime = Time.fixedDeltaTime;
        for (int i=0; i<(int)HumanBodyBones.LeftToes; i++)
        {
            int index = boneCapsuleDict[i];
            HumanBodyBones bone = (HumanBodyBones)i;
            Vector3 pos = player.GetBonePosition(bone);
            Quaternion ang = player.GetBoneRotation(bone);
            CapsuleVelocities[index] = (pos - CapsulesPreviousPos[index]) / deltaTime;
            CapsulesPreviousPos[index] = pos;
            Quaternion deltaRotation = ang * Quaternion.Inverse(capsulesPreviousRotation[index]);
            deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
            Vector3 angularVelocity = rotationAxis * angleInDegrees * Mathf.Deg2Rad / deltaTime;
            CapsuleAngVelocities[index] = angularVelocity;
            capsulesPreviousRotation[index] = ang;
        }
    }/////これたぶんindexがcapsulesとちがう
}
