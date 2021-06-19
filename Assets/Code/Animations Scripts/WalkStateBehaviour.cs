using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkStateBehaviour : StateMachineBehaviour
{
    
    // override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //     Debug.Log("<color=green>OnStateUpdate</color>");
    // }
    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("<color=green>OnStateUpdate</color>");
    }
}
