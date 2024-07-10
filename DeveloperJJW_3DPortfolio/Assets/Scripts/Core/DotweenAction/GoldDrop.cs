using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using JetBrains.Annotations;
using Unity.VisualScripting;

[System.Serializable]
public class GoldDrop : DGAction
{
    public GameObject goldPrefab; // 골드 프리팹
    private Transform goldUI;      // UI 상의 골드 이미지 위치

    // 몬스터가 죽었을 때 호출되는 함수
    public override void Start(Entity entiy, Vector3 monsterPosition)
    {
        goldUI = GameObject.Find("Money").transform;
        // 골드 프리팹을 몬스터 위치에 생성
        GameObject gold = ObjectPool.Spawn(goldPrefab, GameObject.Find("Canvas").transform, monsterPosition, Quaternion.identity);

        // 화면 좌표로 변환
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(monsterPosition);

        // Canvas 하위로 이동
        gold.transform.position = screenPoint;

        // 골드 UI 위치로 이동 애니메이션 // 곡선으로 해봣는데 직선이 더 나은듯..
        gold.transform.DOMove(goldUI.position, 1.0f).OnComplete(() =>
        {
            // 골드 도착 후 처리
            ObjectPool.Recycle(gold);
            // 골드 획득 처리
            UpdateGoldUI(entiy);
        });
    }

    private void UpdateGoldUI(Entity entity)
    {
        // 골드 UI 업데이트 로직 작성
        if(entity.ControlType == EntityControlType.Player)
        {
            // 몬스터 비율로 ㅇㅇ..
            entity.GetGold(10);
        }
    }

    public override object Clone() => new GoldDrop();

}
