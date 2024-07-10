using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "IODatabase")]
public class IODatabase : ScriptableObject
{
    [SerializeField]
    private List<IdentifiedObject> datas = new();

    public IReadOnlyList<IdentifiedObject> Datas => datas;
    public int Count => datas.Count;

    public IdentifiedObject this[int index] => datas[index];

    private void SetID(IdentifiedObject target, int id)
    {
        // IdentifiedObject의 id 변수 찾아옴
        // BindingFlags.NonPublic = 한정자가 public이 아니여야함, BindingFlags.Instance = static type이 아니여야함
        var field = typeof(IdentifiedObject).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
        // 가져온 id 변수 정보로 토대로 target의 id 변수 값을 수정함
        field.SetValue(target, id);
        // Serialize 변수(여기서는 id 변수)를 code상에서 수정했을 경우 EditorUtility.SetDirty를 통해서 Serailize 변수가 수정되었음을 Unity에 알려줘야함
        // 그렇지 않으면 수정한 값이 반영되지 않고 이전 값으로 돌아감
        // 여기서는 값이 수정되었다고 Unity에 알려줄뿐, 실제로 값이 저장될려면 Editor Code에서 ApplyModifiedProperties 함수 혹은
        // 프로젝트 전체를 저장하는 AssetDatabase.SaveAssets 함수가 호출되어야함
        // 여기서는 나중에 다른 곳에서 AssetDatabase.SaveAssets를 호출 할 것이기 따로 작성하지 않음.
#if UNITY_EDITOR
    EditorUtility.SetDirty(target);
#endif
    }

    // index 순서로 IdentifiedObjects의 id를 재설정함
    private void ReorderDatas()
    {
        var field = typeof(IdentifiedObject).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < datas.Count; i++)
        {
            field.SetValue(datas[i], i);
#if UNITY_EDITOR
        EditorUtility.SetDirty(datas[i]);
#endif
        }
    }

    public void Add(IdentifiedObject newData)
    {
        datas.Add(newData);
        SetID(newData, datas.Count - 1);
    }

    public void Add_Sheet(IdentifiedObject newData)
    {
        datas.Add(newData);
    }

    public void Remove(IdentifiedObject data)
    {
        datas.Remove(data);
        ReorderDatas();
    }

    public void Clear()
    {
        for(int i = 0; i < datas.Count; i++)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(datas[i]));
        }
        datas.Clear();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public IdentifiedObject GetDataByID(int id) => datas[id];
    public T GetDataByID<T>(int id) where T : IdentifiedObject => GetDataByID(id) as T;

    public IdentifiedObject GetDataCodeName(string codeName) => datas.Find(item => item.CodeName == codeName);
    public T GetDataCodeName<T>(string codeName) where T : IdentifiedObject => GetDataCodeName(codeName) as T;

    public bool Contains(IdentifiedObject item) => datas.Contains(item);

    public IdentifiedObject GetNextData(int index) => datas.ElementAtOrDefault(index + 1);

    public IdentifiedObject GetPrevData(int index) => datas.ElementAtOrDefault(index - 1);

    public int IndexOf(IdentifiedObject item) => datas.IndexOf(item);

    // Data를 CodeName을 기준으로 오름차순으로 정렬함
    public void SortByCodeName()
    {
        datas.Sort((x, y) => x.CodeName.CompareTo(y.CodeName));
        ReorderDatas();
    }
}
