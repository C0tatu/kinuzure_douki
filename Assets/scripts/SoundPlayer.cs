
using System;
using UdonSharp;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

public class SoundPlayer : UdonSharpBehaviour
{
    public CapsuleApplyer[] capsuleApplyers;
    public GameObject[] CapsuleParents;
    private GameObject[][] Capsules;
    public calcFuncs calculator;
    private string[][] crumpingPairs = new string[][]
    {
        new string[] {"LeftShoulder", "LeftUpperArm"},
        new string[] {"LeftUpperArm", "LeftLowerArm"},
        new string[] {"LeftUpperLeg", "LeftLowerLeg"},

        new string[] {"RightShoulder", "RightUpperArm"},
        new string[] {"RightUpperArm", "RightLowerArm"},
        new string[] {"RightUpperLeg", "RightLowerLeg"},
    };


    void Start()
    {
        
        //各プレイヤーのカプセルを一つの多次元配列にまとめる
        Capsules = new GameObject[CapsuleParents.Length][];
        for(int i=0; i< CapsuleParents.Length; i++)
        {
            Transform p = CapsuleParents[i].GetComponent<Transform>();
            int l = p.childCount;
            Capsules[i] = new GameObject[l];
            for(int j=0; j<l; j++)
            {
                Capsules[i][j] = p.GetChild(j).gameObject;
            }

        }
    }


    private void Update()
    {
        //----------------------------------------
        //こすれ・ねじれ計算えりあ
        //自分自身: ここではこすれとねじれの両方を考える
        for (int i = 0; i < Capsules.Length; i++)
        {
            for(int j=0; j < Capsules[i].Length; j++)
            {
                for(int k = Capsules[i].Length-1; k>j; k--)
                {
                    GameObject tar1 = Capsules[i][j];
                    GameObject tar2 = Capsules[i][k];
                    if(isCrumpingPair(tar1.name, tar2.name))//ねじれ
                    {
                        if (capsuleApplyers[i].playerID != 0)//対象のカプセル追従関数がアクティブかどうか
                        {
                            Vector3 angv1;
                            Vector3 angv2;
                            if (tar1.name.Contains("Shoulder"))
                            {
                                angv1 = capsuleApplyers[i].CapsuleAngVelocities[j];
                                angv2 = capsuleApplyers[i].CapsuleAngVelocities[k];
                            }
                            else if (tar2.name.Contains("Shoulder"))
                            {
                                angv2 = capsuleApplyers[i].CapsuleAngVelocities[j];
                                angv1 = capsuleApplyers[i].CapsuleAngVelocities[k];
                            }
                            else if (tar1.name.Contains("Upper"))
                            {
                                angv1 = capsuleApplyers[i].CapsuleAngVelocities[j];
                                angv2 = capsuleApplyers[i].CapsuleAngVelocities[k];
                            }
                            else
                            {
                                angv2 = capsuleApplyers[i].CapsuleAngVelocities[j];
                                angv1 = capsuleApplyers[i].CapsuleAngVelocities[k];
                            }
                            Vector3 crumpingV = calculator.CrumpingCalc(angv1, angv2);
                            //ここで音を鳴らす
                        }

                    }
                    else
                    {
                        if (calculator.isBounds(tar1, tar2))//こすれ
                        {
                            if (capsuleApplyers[i].playerID != 0)//対象のカプセル追従がアクティブかどうか
                            {
                                Vector3 v1 = capsuleApplyers[i].CapsuleAngVelocities[j];
                                Vector3 v2 = capsuleApplyers[i].CapsuleAngVelocities[k];
                                Vector3 angv1 = capsuleApplyers[i].CapsuleAngVelocities[j];
                                Vector3 angv2 = capsuleApplyers[i].CapsuleAngVelocities[k];
                                Vector3 frictionV = calculator.FrictionCalc(tar1, tar2, v1, v2, angv1, angv2);
                                //ここで音を鳴らす
                            }

                        }
                    }
                    
                }
            }
        }
        //対他人
        //ここではねじれを考えなくていい
        for (int i = 0; i < Capsules.Length; i++)
        {
            if (Capsules[i] == null) continue;

            for (int j = 0; j < Capsules[i].Length; j++)
            {
                for (int k = i + 1; k < Capsules.Length; k++)  
                {
                    if (Capsules[k] == null) continue;

                    for (int l = 0; l < Capsules[k].Length; l++)
                    {
                        GameObject tar1 = Capsules[i][j];
                        GameObject tar2 = Capsules[k][l];
                        if (calculator.isBounds(tar1, tar2)){
                            if ((capsuleApplyers[i].playerID != 0) && (capsuleApplyers[k].playerID != 0))//対象のカプセル追従関数がアクティブなら
                            {
                                Vector3 v1 = capsuleApplyers[i].CapsuleAngVelocities[j];
                                Vector3 v2 = capsuleApplyers[k].CapsuleAngVelocities[l];
                                Vector3 angv1 = capsuleApplyers[i].CapsuleAngVelocities[j];
                                Vector3 angv2 = capsuleApplyers[k].CapsuleAngVelocities[l];
                                Vector3 frictionV = calculator.FrictionCalc(tar1, tar2, v1, v2, angv1, angv2);
                                //ここで音を鳴らす

                            }

                        }
                    }
                }
            }
        }
    }

    private bool isCrumpingPair(string name1, string name2)
    {
        for(int i=0; i<crumpingPairs.Length; i++)
        {
            if (crumpingPairs[i][0] == name1 && crumpingPairs[i][1] == name2) return true;
            if (crumpingPairs[i][1] == name1 && crumpingPairs[i][0] == name2) return true;
        }
        return false;
    }
}