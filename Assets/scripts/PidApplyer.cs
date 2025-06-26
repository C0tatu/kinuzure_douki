
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PidApplyer : UdonSharpBehaviour
{
    public CapsuleApplyer[] applyers;
    public GameObject[] parents;
    public Material mat;
    [UdonSynced] private int index = -1;
    void Start()
    {
        
    }

    public override void Interact()
    {
        if (!Networking.IsOwner(this.gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }


        base.Interact();
        index++;

        appendPlayer();

        RequestSerialization();
        this.gameObject.GetComponent<Renderer>().material = mat;
    }

    public override void OnDeserialization()
    {
        base.OnDeserialization();
        //appendPlayer();
    }

    private void appendPlayer()
    {
        if (index < applyers.Length)
        {
            int pid = VRCPlayerApi.GetPlayerId(Networking.LocalPlayer);
            Transform p = parents[index].GetComponent<Transform>();
            GameObject[] capsules = new GameObject[p.childCount];
            for (int i = 0; i < capsules.Length; i++)
            {
                capsules[i] = p.GetChild(i).gameObject;
            }
            applyers[index].Constructor(pid, capsules);
        }

        
    }
}
