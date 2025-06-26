
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class calcFuncs : UdonSharpBehaviour
{
    /// <summary>
    /// ２つのgameObjectが接触しているかを調べる関数
    /// </summary>
    public bool isBounds(GameObject obj1, GameObject obj2)
    {
        Collider collider1 = obj1.GetComponent<Collider>();
        Collider collider2 = obj2.GetComponent<Collider>();

        if (collider1.bounds.Intersects(collider2.bounds)) return true;
        else return false;
    }





    /// <summary>
    ///  カプセル同士のこすれ速度の計算関数
    ///  obj1,obj2: 対象のカプセル
    ///  v1,v2; 対象の速度
    ///  ang1,ang2: 対象の角速度
    /// </summary>
    public Vector3 FrictionCalc(GameObject obj1, GameObject obj2, Vector3 v1, Vector3 v2, Vector3 angv1, Vector3 angv2)//カプセル同士のこすれ速度を計算する
    {

        Collider collider1 = obj1.GetComponent<Collider>();
        Collider collider2 = obj2.GetComponent<Collider>();

        
        
        //重なってる点を取得(中央にしてみる)
        Vector3 closest1 = collider1.ClosestPoint(collider2.transform.position);
        Vector3 closest2 = collider2.ClosestPoint(collider1.transform.position);
        Vector3 centerPoint = (closest1 + closest2) / 2;

        //各種情報取得
        //接触点への相対座標
        Vector3 r1 = centerPoint - collider1.transform.position;
        Vector3 r2 = centerPoint - collider2.transform.position;

        //計算
        //それぞれのカプセルの接触点での速度
        Vector3 v_r1 = v1 + Vector3.Cross(angv1, r1);
        Vector3 v_r2 = v2 + Vector3.Cross(angv2, r2);

        //接触点での相対速度
        Vector3 v_r = v_r1 - v_r2;

        return v_r;
        
    }

    /// <summary>
    /// ねじれの計算関数
    /// angp: 親boneにあたるカプセルの角速度
    /// angc: 子boneにあたるカプセルの角速度
    /// </summary>
    public Vector3 CrumpingCalc(Vector3 angp, Vector3 angc)
    {
        return angc - angp;
    }



}
