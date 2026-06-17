using UnityEngine;

/// <summary>
/// 실시간 그림자를 끈 환경에서 "그림자가 있는 것처럼" 보이게 하는 통합 컴포넌트.
/// Blob Shadow(그림자) + AO grounding(접지감) 두 레이어를 런타임에 생성해 처리한다.
///
/// 3D 씬 안정성을 위해 Quad(Mesh) + MeshRenderer 방식을 사용한다.
/// 셰이더는 URP(Universal Render Pipeline)와 Built-in을 자동 감지한다.
///
/// 사용법:
///   1. 그림자를 그릴 대상에게 이 컴포넌트를 추가
///   2. _shadowTexture에 그림자 텍스쳐 지정(비워도 자동 생성)
///   3. _groundLayer에 바닥 레이어를 지정한다.
/// </summary>
[DisallowMultipleComponent]
public class FakeShadow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Texture2D _shadowTexture;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask _groundLayer = ~0;
    [SerializeField] private float _raycastHeight = 5f;
    [SerializeField] private float _yOffset = 0.05f;
    [SerializeField] private float _footOffset = 1f;

    [Header("Shadow (진한 그림자 레이어)")]
    [SerializeField] private float _baseSize = 1.0f;
    [SerializeField, Range(0f, 1f)] private float _baseOpacity = 0.5f;
    [SerializeField] private float _maxHeight = 3f;
    [SerializeField, Range(0f, 1f)] private float _minSizeScale = 0.5f;

    [Header("AO Grounding (넓고 옅은 접지감 레이어)")]
    [SerializeField] private bool _useAOLayer = true;
    [SerializeField] private float _aoSizeMultiplier = 1.6f;
    [SerializeField, Range(0f, 1f)] private float _aoOpacity = 0.25f;

    [Header("Render Order")]
    [SerializeField] private int _renderQueue = 3001;

    private static readonly int COLOR_ID = Shader.PropertyToID("_Color");
    private static readonly int BASE_COLOR_ID = Shader.PropertyToID("_BaseColor");
    private static readonly int MAIN_TEX_ID = Shader.PropertyToID("_MainTex");
    private static readonly int BASE_MAP_ID = Shader.PropertyToID("_BaseMap");

    private Transform _shadowTransform;
    private Transform _aoTransform;
    private MeshRenderer _shadowMr;
    private MeshRenderer _aoMr;
    private Material _shadowMat;
    private Material _aoMat;
    private Texture2D _generatedTex;

    private static Transform _container;

    private static Transform GetContainer()
    {
        if (_container == null)
        {
            var containerGo = new GameObject("====FakeShadows====");
            _container = containerGo.transform;
            // 트랜스폼이 자식 월드 좌표에 영향을 주지 않도록 기본값 고정.
            _container.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            _container.localScale = Vector3.one;
        }
        return _container;
    }

    private void Awake()
    {
        // 없으면 생성
        if (_shadowTexture == null)
        {
            _shadowTexture = CreateRadialTexture(128);
            _generatedTex = _shadowTexture;
        }

        _shadowTransform = CreateLayer("FakeShadow_Shadow", out _shadowMr, out _shadowMat);
        if (_useAOLayer)
            _aoTransform = CreateLayer("FakeShadow_AO", out _aoMr, out _aoMat);
    }

    private Transform CreateLayer(string name, out MeshRenderer mr, out Material mat)
    {
        var gObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        gObj.name = name;

        // Quad에 붙는 Collider는 그림자가 Raycast를 가로막을 수 있으므로 제거.
        var quadCollider = gObj.GetComponent<Collider>();
        if (quadCollider != null) Destroy(quadCollider);

        // 공용 컨테이너 밑에 모으기
        gObj.transform.SetParent(GetContainer(), worldPositionStays: true);

        // 바닥에 눕히기
        gObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        mr = gObj.GetComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        mat = CreateUnlitTransparentMaterial();
        mat.mainTexture = _shadowTexture;
        SetMaterialTexture(mat, _shadowTexture);
        mat.renderQueue = _renderQueue;
        mr.material = mat;

        return gObj.transform;
    }

    private void LateUpdate()
    {
        Vector3 origin = transform.position + Vector3.up * _raycastHeight;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _raycastHeight * 2f, _groundLayer))
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        // 발밑(루트에서 _footOffset 아래)을 기준으로 지면까지의 거리를 잰다.
        // 루트가 몸 중앙에 있어도 서 있을 때 distanceToGround ≈ 0 이 되도록.
        float feetY = transform.position.y - _footOffset;
        float distanceToGround = Mathf.Max(0f, feetY - hit.point.y);
        float t = Mathf.Clamp01(distanceToGround / _maxHeight);

        float sizeScale = Mathf.Lerp(1f, _minSizeScale, t);
        float opacityScale = Mathf.Lerp(1f, 0f, t);

        Vector3 pos = hit.point + Vector3.up * _yOffset;

        // 지면 법선에 맞춰 눕히고, 대상의 Y축 회전 반영.
        Quaternion lie = Quaternion.FromToRotation(Vector3.forward, -hit.normal)
                         * Quaternion.Euler(0f, 0f, transform.eulerAngles.y);

        _shadowTransform.SetPositionAndRotation(pos, lie);
        _shadowTransform.localScale = Vector3.one * (_baseSize * sizeScale);
        SetAlpha(_shadowMat, _baseOpacity * opacityScale);

        if (_aoTransform != null)
        {
            _aoTransform.SetPositionAndRotation(pos + hit.normal * 0.001f, lie);
            _aoTransform.localScale = Vector3.one * (_baseSize * _aoSizeMultiplier * sizeScale);
            SetAlpha(_aoMat, _aoOpacity * opacityScale);
        }
    }

    private void SetVisible(bool visible)
    {
        if (_shadowMr != null)
        {
            _shadowMr.enabled = visible;
        }

        if (_aoMr != null)
        {
            _aoMr.enabled = visible;
        } 
    }

    private void SetAlpha(Material mat, float a)
    {
        if (mat == null)
        {
            return;
        } 

        Color c = mat.HasProperty(BASE_COLOR_ID) ? mat.GetColor(BASE_COLOR_ID) : mat.color;
        c = new Color(0f, 0f, 0f, a);
        if (mat.HasProperty(BASE_COLOR_ID)) mat.SetColor(BASE_COLOR_ID, c);
        if (mat.HasProperty(COLOR_ID)) mat.SetColor(COLOR_ID, c);
    }

    /// <summary>
    /// URP면 URP Unlit, 아니면 Built-in Unlit/Transparent 머티리얼을 만든다.
    /// </summary>
    private static Material CreateUnlitTransparentMaterial()
    {
        bool isSRP = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;

        Shader shader = null;
        if (isSRP)
            shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Transparent");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        var mat = new Material(shader);

        // URP Unlit을 투명 모드로 전환
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // 0=Opaque, 1=Transparent
        if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);     // 0=Alpha

        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.DisableKeyword("_ALPHATEST_ON");

        return mat;
    }

    private static void SetMaterialTexture(Material mat, Texture tex)
    {
        if (mat.HasProperty(BASE_MAP_ID)) mat.SetTexture(BASE_MAP_ID, tex);
        if (mat.HasProperty(MAIN_TEX_ID)) mat.SetTexture(MAIN_TEX_ID, tex);
    }

    private void OnDestroy()
    {
        if (_shadowTransform != null) Destroy(_shadowTransform.gameObject);
        if (_aoTransform != null) Destroy(_aoTransform.gameObject);
        if (_shadowMat != null) Destroy(_shadowMat);
        if (_aoMat != null) Destroy(_aoMat);
        if (_generatedTex != null) Destroy(_generatedTex);
    }

    /// <summary>
    /// 원형 그라데이션 텍스처 자동 생성. 중심 불투명 -> 가장자리 투명.
    /// </summary>
    private static Texture2D CreateRadialTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };

        float center = size * 0.5f;
        float maxDist = center;

        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy) / maxDist;
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = alpha * alpha * (3f - 2f * alpha);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}