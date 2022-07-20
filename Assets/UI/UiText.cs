using UnityEngine;
using UnityEngine.UI;
using static TextValue;

public class UiText : MonoBehaviour
{
    public Text text;
    public TextValue source;
    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        if (source != null)
        {
            fill(source.getText());
        }
    }

    void fill(TextData data)
    {
        text.text = data.text;
        text.color = data.color;
    }
}
