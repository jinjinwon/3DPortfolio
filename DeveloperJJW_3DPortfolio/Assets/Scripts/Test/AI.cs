using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    [SerializeField]
    private Entity target;

    public Entity Target { get => target; set => target = value; }

    private void Start()
    {
        var entity = GetComponent<Entity>();
        entity.Target = target;

        // 스킬 사용 부분
        //var registedSkill = entity.SkillSystem.Register(skill);
        //registedSkill.Use();
    }
}
