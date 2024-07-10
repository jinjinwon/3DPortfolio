using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Test
{
    [AddComponentMenu("Test/StatTest")]
    public class StatTest : MonoBehaviour
    {
        [ContextMenu("Test")]
        private void Test()
        {
            Debug.Log("<color=yellow>[StatTest] Start</color>");

            var stat = ScriptableObject.CreateInstance<Stat>();
            stat.MaxValue = float.MaxValue;

            // Assert : Debug.Log와 비슷한 용도로 사용됩니다. 실무에서는 Assert 쪽이 더 많이 사용되는데 그 이유는
            // Assert문은 Edtior에서만 사용되며 다른 플랫폼에서 사용시에는 코드가 무시되어 퍼포먼스가 좋기 때문입니다.
            // Debug.Log도 무시하게끔 할 수는 있지만 따로 처리를 해야해서 Assert문 사용에 익숙해지는게 좋습니다.
            stat.SetBonusValue("Test", 10f);
            Assert.IsTrue(stat.ContainsBonusValue("Test"), "Test Bonus Value가 없습니다.");
            Assert.IsTrue(Mathf.Approximately(stat.GetBonusValue("Test"), 10f), "Stat의 Test Bonus Value가 10이 아닙니다.");
            Debug.Log($"Test Bonus Value: {stat.GetBonusValue("Test")}");

            Assert.IsTrue(stat.RemoveBonusValue("Test"), "Test Bonus Value의 삭제 실패");
            Assert.IsFalse(stat.ContainsBonusValue("Test"), "Test Bonus Value를 삭제하였으나 아직 남아있습니다.");
            Debug.Log("Remove Test Bonus Value");

            stat.SetBonusValue("Test", "Test2", 10f);
            Assert.IsTrue(stat.ContainsBonusValue("Test", "Test2"), "Test-Test2 Bonus Value가 없습니다.");
            Assert.IsTrue(Mathf.Approximately(stat.GetBonusValue("Test", "Test2"), 10f), "Test-Test2 Bonus Value가 10이 아닙니다.");
            Debug.Log($"Test-Test2 Bonus Value: {stat.GetBonusValue("Test", "Test2")}");

            Assert.IsTrue(stat.RemoveBonusValue("Test", "Test2"), "Test-Test2 Bonus Value의 삭제 실패");
            Assert.IsFalse(stat.ContainsBonusValue("Test", "Test2"), "Test-Test2 Bonus Value를 삭제하였으나 아직 남아있습니다.");
            Debug.Log("Remove Test-Test2 Bonus Value");

            stat.RemoveBonusValue("Test");
            Debug.Log("Remove Test Bonus Value");

            stat.SetBonusValue("Test", 100f);
            Debug.Log("Set Test Bonus: " + stat.GetBonusValue("Test"));
            stat.SetBonusValue("Test2", 100f);
            Debug.Log("Set Test2 Bonus: " + stat.GetBonusValue("Test2"));
            Assert.IsTrue(Mathf.Approximately(stat.BonusValue, 200f), "Bonus Value의 합계가 200이 아닙니다.");
            Debug.Log("Total Bonus Value: 200");

            stat.DefaultValue = 100f;
            Debug.Log("Set Default Value: " + stat.DefaultValue);
            Assert.IsTrue(Mathf.Approximately(stat.Value, 300f), "Total Value가 300이 아닙니다.");
            Debug.Log("Value: 300");

            if (Application.isPlaying)
                Destroy(stat);
            else
                DestroyImmediate(stat);

            Debug.Log("<color=green>[StatTest] Success</color>");
        }
    }
}