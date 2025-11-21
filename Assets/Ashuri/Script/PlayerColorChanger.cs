using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorChanger : NetworkBehaviour
{
    [Header("マテリアルを変更したいプレイヤーのパーツ")]
    [Tooltip("自動で子の MeshRenderer を取得します")]
    private List<MeshRenderer> _parts = new List<MeshRenderer>();

    [Header("利用可能なマテリアルリスト")]
    [SerializeField] private Material[] availableMaterials;

    private void Awake()
    {
        // 子にある MeshRenderer をすべて取得する
        _parts.AddRange(GetComponentsInChildren<MeshRenderer>());
    }

    // インデックスで指定されたマテリアルを全パーツに適用する
    [Command]
    public void CmdChangeMaterial(int materialIndex)
    {
        if (materialIndex < 0 || materialIndex >= availableMaterials.Length)
            return;

        foreach (var part in _parts)
        {
            part.material = availableMaterials[materialIndex];
        }
    }
}
