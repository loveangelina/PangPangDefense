using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieBehavior : StateMachineBehaviour
{
    private float stateEnterTime; // 상태가 시작된 시점을 저장하는 변수
    private float animationLength; // 애니메이션 길이를 저장하는 변수
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateEnterTime = Time.time;

        // 현재 애니메이션 클립의 길이를 가져옵니다.
        animationLength = stateInfo.length;

        // 현재 애니메이션 클립의 길이의 반 이상 재생되면 몬스터 비활성화
    }
    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Time.time >= stateEnterTime + animationLength)
        {
            animator.gameObject.SetActive(false);
        }
    }
}
