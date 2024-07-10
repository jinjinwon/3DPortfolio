using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StageData
{
    public int floor;

    [UnderlineTitle("맵 프리팹")]
    public GameObject mapPrefab;

    [UnderlineTitle("등장 몬스터")] // 원래는 Monster 클래스를 넣어야 함 일단 스테이지를 먼저 작업하므로 게임 오브젝트로 ㅇㅇ;
    [SerializeReference]
    public Monster[] monsters;

    [UnderlineTitle("몬스터 생성 개수 보스 라운드의 경우 +1")]
    public int regenCount;

    [UnderlineTitle("보스 라운드 여부")]
    public bool isBossRound;

    [UnderlineTitle("보스 몬스터 등장 시 일반 몬스터 제거 여부")]
    public bool bossMonsterGen;

    [UnderlineTitle("보스 몬스터")] // 원래는 Monster 클래스를 넣어야 함 일단 스테이지를 먼저 작업하므로 게임 오브젝트로 ㅇㅇ;
    [SerializeReference]
    public Monster bossmonster;

    [UnderlineTitle("다음 층까지 필요한 킬 횟수")]
    public int nextFloorKill;

    [UnderlineTitle("BGM")]
    public AudioClip audioClip;

    [UnderlineTitle("다음 층으로 넘어갈 때 보여 줄 액션")]
    [SerializeReference, SubclassSelector]
    public NextFloorAction[] customActionsFade;

}
