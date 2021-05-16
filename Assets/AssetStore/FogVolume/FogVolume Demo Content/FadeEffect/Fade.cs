using UnityEngine;
[ExecuteInEditMode]
public class Fade : MonoBehaviour
{

    public Color _Color = Color.white;
    Shader _FadeShader = null;
    [HideInInspector]
    public Material _FadeMaterial = null;
    public Material FadeMaterial
    {
        get
        {
            if (_FadeMaterial == null)
            {
                _FadeMaterial = new Material(_FadeShader);
                _FadeMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return _FadeMaterial;
        }
    }

    void OnEnable()
    {
        _FadeShader = Shader.Find("Hidden/Fade");

        if (_FadeShader == null) print("Hidden/Fade #SHADER ERROR#");
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (FadeMaterial)
        {
            FadeMaterial.SetColor("_Color",_Color);
            Graphics.Blit(source, destination, FadeMaterial);
        }
        else
            Graphics.Blit(source, destination);
    }
    void OnDisable()
    {
        DestroyImmediate(_FadeMaterial);
    }
}
