using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextWobble : MonoBehaviour
{
    private TMP_Text _textMesh;

    private Mesh _mesh;

    private Vector3[] _vertices;
    private List<int> _wordIndexes;
    private List<int> _wordLengths;
    public Gradient rainbow;
    public ChooseWobbleType wobbleType;
    public AnimType type;

    [Header("For Wave")] [SerializeField] private float waveSpeed = 2f;
    [SerializeField] private float waveHeight = 20f;
    [Header("Not Wave")] [SerializeField] private float xOffset = 3.3f;
    [SerializeField] private float yOffset = 2.5f;
    [SerializeField] private float xSpeed = 0.1f;
    [SerializeField] private float ySpeed = 0.1f;


    public enum ChooseWobbleType
    {
        VertexWobble,
        CharacterWobble,
        WordWobble
    }

    public enum AnimType
    {
        Wave,
        NotWave
    }

    // Start is called before the first frame update
    private void Start()
    {
        _textMesh = GetComponent<TMP_Text>();
        if (wobbleType == ChooseWobbleType.WordWobble)
            WordWobbleInit();
    }

    // Update is called once per frame
    private void Update()
    {
        _textMesh.ForceMeshUpdate();
        _mesh = _textMesh.mesh;
        _vertices = _mesh.vertices;

        if (wobbleType == ChooseWobbleType.VertexWobble)
        {
            VertexWobbleWithColor();
        }

        if (wobbleType == ChooseWobbleType.CharacterWobble)
        {
            CharacterWobbleWithColor();
        }

        if (wobbleType == ChooseWobbleType.WordWobble)
        {
            WordWobbleWithColor();
        }
    }

    private void VertexWobbleWithColor()
    {
        var colors = _mesh.colors;
        for (var i = 0; i < _vertices.Length; i++)
        {
            if (type == AnimType.Wave)
            {
                _vertices[i] += Wobble(_vertices[i]);
            }
            else
            {
                Vector3 offset = Wobble(Time.time + i);
                _vertices[i] += offset;
            }

            colors[i] = rainbow.Evaluate(Mathf.Repeat(Time.time + _vertices[i].x * 0.001f, 1f));
        }

        _mesh.vertices = _vertices;
        _mesh.colors = colors;
        _textMesh.canvasRenderer.SetMesh(_mesh);
    }

    private void CharacterWobbleWithColor()
    {
        var colors = _mesh.colors;
        for (var i = 0; i < _textMesh.textInfo.characterCount; i++)
        {
            var c = _textMesh.textInfo.characterInfo[i];
            if (!c.isVisible)
            {
                continue;
            }

            var index = c.vertexIndex;

            for (var j = 0; j < 4; j++)
            {
                if (type == AnimType.Wave)
                {
                    _vertices[index + j] += Wobble(_vertices[index + j]);
                }
                else
                {
                    Vector3 offset = Wobble(Time.time + i);
                    _vertices[index + j] += offset;
                }

                colors[index + j] = rainbow.Evaluate(Mathf.Repeat(Time.time + _vertices[index + j].x * 0.001f, 1f));
            }
        }

        _mesh.vertices = _vertices;
        _mesh.colors = colors;
        _textMesh.canvasRenderer.SetMesh(_mesh);
    }

    private void WordWobbleInit()
    {
        _wordIndexes = new List<int> {0};
        _wordLengths = new List<int>();

        var s = _textMesh.text;
        for (int index = s.IndexOf(' '); index > -1; index = s.IndexOf(' ', index + 1))
        {
            _wordLengths.Add(index - _wordIndexes[_wordIndexes.Count - 1]);
            _wordIndexes.Add(index + 1);
        }

        _wordLengths.Add(s.Length - _wordIndexes[_wordIndexes.Count - 1]);
    }

    private void WordWobbleWithColor()
    {
        var colors = _mesh.colors;

        for (var w = 0; w < _wordIndexes.Count; w++)
        {
            var wordIndex = _wordIndexes[w];


            for (var i = 0; i < _wordLengths[w]; i++)
            {
                var c = _textMesh.textInfo.characterInfo[wordIndex + i];

                var index = c.vertexIndex;
                colors[index] = rainbow.Evaluate(Mathf.Repeat(Time.time + _vertices[index].x * 0.001f, 1f));
                colors[index + 1] = rainbow.Evaluate(Mathf.Repeat(Time.time + _vertices[index + 1].x * 0.001f, 1f));
                colors[index + 2] = rainbow.Evaluate(Mathf.Repeat(Time.time + _vertices[index + 2].x * 0.001f, 1f));
                colors[index + 3] = rainbow.Evaluate(Mathf.Repeat(Time.time + _vertices[index + 3].x * 0.001f, 1f));

                for (var j = 0; j < 4; j++)
                {
                    if (type == AnimType.Wave)
                    {
                        _vertices[index + j] += Wobble(_vertices[index + j]);
                    }
                    else
                    {
                        Vector3 offset = Wobble(Time.time + w);
                        _vertices[index + j] += offset;
                    }
                }
            }
        }

        _mesh.vertices = _vertices;
        _mesh.colors = colors;
        _textMesh.canvasRenderer.SetMesh(_mesh);
    }

    Vector2 Wobble(float time)
    {
        return new Vector2(Mathf.Sin(time * xOffset) * xSpeed, Mathf.Cos(time * yOffset) * ySpeed);
    }

    Vector3 Wobble(Vector3 orig)
    {
        return new Vector3(0, Mathf.Sin(Time.time * waveSpeed + orig.x * 0.01f) * waveHeight, 0);
    }
}