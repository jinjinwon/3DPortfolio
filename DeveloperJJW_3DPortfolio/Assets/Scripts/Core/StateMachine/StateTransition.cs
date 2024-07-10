using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// --- 제너릭 클래스 사용에 대한 이점
// 의도치 않은 데이터 타입 입력 방지
// 전달 시 추가적인 형변환 필요없음
// 박싱, 언박싱에 대한 비용 절감

// --- 제너릭 클래스의 Where에 대한 조건
//where T : struct는 값 타입인 형식에 대한 제약 조건이다.
//where T : class는 참조 타입인 형식에 대한 제약 조건이다.
//where T : new()는 매개 변수가 없는 기본 생성자를 가지는 형식에 대한 제약 조건이다.
//where T : ParentClass는 파생된 형식에 대한 제약 조건이다. 무조건 ParentClass를 상속해야한다.
//where T : ISomeInterface는 ISomeInterface 인터페이스를 구현하는 형식에 대한 제약 조건이다.
//where T : struct, ISomeInterface는 값 타입 형식 + 인터페이스 형식에 대한 제약 조건이다.

// EntityType은 State를 소유하는 Entity의 Type
// StateMachine의 EntityType과 일치해야함
public class StateTransition<EntityType>
{
    // Transition Command가 없음을 나타냄
    // nullable로도 넣을 수 있지만 int.MinValue로 넣어 비용 감소 (정의한 것)
    public const int kNullCommand = int.MinValue;

    // Transition을 위한 조건 함수, 인자는 현재 State, 결과값은 전이 가능 여부(bool)
    // 애니메이터 Transition을 생각하면 이해하기 쉬움 
    private Func<State<EntityType>, bool> transitionCondition;

    // 현재 State에서 다시 현재 State로 전이가 가능하지에 대한 여부
    // 애니메이터에 존재하는 Any 노드 생성하면 이해하기 쉬움
    public bool CanTrainsitionToSelf { get; private set; }
    // 현재 State
    public State<EntityType> FromState { get; private set; }
    // 전이할 State
    public State<EntityType> ToState { get; private set; }
    // 전이 명령어
    public int TransitionCommand { get; private set; }

    // 전이 가능 여부(Condition 조건 만족 여부)
    public bool IsTransferable => transitionCondition == null || transitionCondition.Invoke(FromState);

    // 생성자 함수
    public StateTransition(State<EntityType> fromState,
        State<EntityType> toState,
        int transitionCommand,
        Func<State<EntityType>, bool> transitionCondition,
        bool canTrainsitionToSelf)
    {
        // 의미없는 Transition 방지용
        Debug.Assert(transitionCommand != kNullCommand || transitionCondition != null,
            "StateTransition - TransitionCommand와 TransitionCondition은 둘 다 null이 될 수 없습니다.");

        FromState = fromState;
        ToState = toState;
        TransitionCommand = transitionCommand;
        this.transitionCondition = transitionCondition;
        CanTrainsitionToSelf = canTrainsitionToSelf;
    }
}