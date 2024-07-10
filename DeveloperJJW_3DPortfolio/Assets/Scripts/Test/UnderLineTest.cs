using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderLineTest : MonoBehaviour
{
    [UnderlineTitle("Option")]
    [SerializeField]
    private bool isOn;

    [UnderlineTitle("Number")]
    [SerializeField]
    private int primary;

    [UnderlineTitle("String")]
    [SerializeField]
    private string str;
}
