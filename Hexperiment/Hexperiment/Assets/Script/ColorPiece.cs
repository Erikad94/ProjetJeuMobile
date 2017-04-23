using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPiece : MonoBehaviour {

    public enum ColorType
    {
        DARKBLUE,
        LIGTHBLUE,
        PURPLE,
        RED,
        ORANGE,
        GREEN,
        PINK,
        ANY,
        COUNT
    };

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    };

    public ColorSprite[] colorSprites;

    private ColorType color;
    private SpriteRenderer sprite;
    private Dictionary<ColorType, Sprite> colorSpriteDict;

    public ColorType Color
    {
        get { return color; }
        set { SetColor (value); }
    }

    void Awake()
    {
        sprite = transform.Find("piece").GetComponent<SpriteRenderer>();

        colorSpriteDict = new Dictionary<ColorType, Sprite>();

        for(int i =0; i < colorSprites.Length; i++)
        {
            if(!colorSpriteDict.ContainsKey (colorSprites [i].color))
            {
                colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public int NumColors
    {
        get { return colorSprites.Length; }
    }

    public void SetColor(ColorType newColor)
    {
        color = newColor;

        if(colorSpriteDict.ContainsKey(newColor))
        {
            sprite.sprite = colorSpriteDict[newColor];
        }
    }
}
