using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Category : IdentifiedObject
{
    public override bool Equals(object other)
        => base.Equals(other);

    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(Category lhs, string rhs)
    {
        if (lhs is null)
            return rhs is null;
        return lhs.CodeName == rhs;
    }
    
    public static bool operator !=(Category lhs, string rhs) => !(lhs == rhs);

    #region °ª º¯°æ
    public void SetID_(int value) => SetID(value);
    public void SetCodeName_(string value) => SetCodeName(value);
    public void SetDisplayName_(string value) => SetDisplayName(value);
    public void SetDescription_(string value) => SetDescription(value);
    public void SetIcon_(Sprite value) => SetIcon(value);
    #endregion
}
