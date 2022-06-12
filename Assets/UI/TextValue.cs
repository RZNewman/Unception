using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface TextValue
{
    public struct TextData
    {
        public Color color;
        public string text;
    }
    public TextData getText();
}