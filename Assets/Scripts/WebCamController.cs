using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

public class WebCamController : MonoBehaviour
{
    [DllImport("__Internal")]
    static extern void SetMindARCallBack(Action<int, float[]> cb);

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        SetMindARCallBack(MindARCallBack);
    }

    static List<(int index, float[] matrix)> UpdateQue = new List<(int index, float[] matrix)>();

    [MonoPInvokeCallback(typeof(Action<int, float[]>))]
    static void MindARCallBack(int index, [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] float[] matrix)
    {
        UpdateQue.Add((index, matrix));
        Process();
    }

    //----------------------------Process static variables------------------
    static UnityEngine.UI.Text _txt;
    static Transform _Gizmo;
    static float[] floats;
    static Matrix4x4 matrix = new Matrix4x4();
    static int i;
    static Vector3 tmpVec3,zaribPos;
    //---------------------------------------------

    [SerializeField]
    UnityEngine.UI.Text txt;
    [SerializeField]
    Material camMat;
    [SerializeField]
    Transform gizmo;
    WebCamDevice env;
    WebCamTexture texture;

    void Start()
    {
        Application.targetFrameRate = 30;
        env = WebCamTexture.devices[!WebCamTexture.devices[0].isFrontFacing ? 0 : 1];
        
        texture = new WebCamTexture(env.name);
        camMat.mainTexture = texture;
        texture.Play();
        
        //init for Process
        zaribPos.Set(100f,100f,-100f);
        _txt = txt;
        _Gizmo = gizmo;
    }

    static void Process() //processed every call back
    {
        floats = UpdateQue[0].matrix;
        _Gizmo.gameObject.SetActive(floats != null);

        if (floats != null)
        {

            for (i = 0; i < floats.Length; i++)
                matrix[i] = floats[i];

            _Gizmo.transform.localRotation = matrix.rotation;
            tmpVec3 = matrix.GetPosition();

            tmpVec3.x /= zaribPos.x;
            tmpVec3.y /= zaribPos.y;
            tmpVec3.z /= zaribPos.z;
            _Gizmo.transform.localPosition = tmpVec3;

            tmpVec3 = _Gizmo.eulerAngles;
            tmpVec3.x *= -1;
            tmpVec3.y *= -1;
            _Gizmo.eulerAngles = tmpVec3;

            _txt.text = $"{_Gizmo.transform.localPosition}";
        }


            UpdateQue.Clear();
    }
}
