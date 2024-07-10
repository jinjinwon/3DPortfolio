using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Icons : MonoBehaviour
{
    [SerializeField]
    private Entity entity;

    [SerializeField]
    private UserSetting userSetting;

    public void OnClickSkillSystem()
    {
        var skillTreeView = SkillTreeView.Instance;
        if (!skillTreeView.gameObject.activeSelf)
            skillTreeView.Show(entity, entity.SkillSystem.DefaultSkillTree);
        else
            skillTreeView.Hide();
    }

    public void OnClickSetting()
    {
        userSetting.Active();
    }
}
