using System;
using System.Collections;
using System.Collections.Generic;
using RengeGames.HealthBars.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RengeGames.HealthBars {
    #region Property Names
    public static class RadialHealthBarProperties {
        public const string OverlayColor = "OverlayColor";
        public const string InnerColor = "InnerColor";
        public const string BorderColor = "BorderColor";
        public const string EmptyColor = "EmptyColor";
        public const string SpaceColor = "SpaceColor";
        public const string SegmentCount = "SegmentCount";
        public const string RemoveSegments = "RemoveSegments";
        public const string SegmentSpacing = "SegmentSpacing";
        public const string Arc = "Arc";
        public const string Radius = "Radius";
        public const string LineWidth = "LineWidth";
        public const string Rotation = "Rotation";
        public const string Offset = "Offset";
        public const string BorderWidth = "BorderWidth";
        public const string BorderSpacing = "BorderSpacing";
        public const string RemoveBorder = "RemoveBorder";
        public const string OverlayNoiseEnabled = "OverlayNoiseEnabled";
        public const string OverlayNoiseScale = "OverlayNoiseScale";
        public const string OverlayNoiseStrength = "OverlayNoiseStrength";
        public const string OverlayNoiseOffset = "OverlayNoiseOffset";
        public const string EmptyNoiseEnabled = "EmptyNoiseEnabled";
        public const string EmptyNoiseScale = "EmptyNoiseScale";
        public const string EmptyNoiseStrength = "EmptyNoiseStrength";
        public const string EmptyNoiseOffset = "EmptyNoiseOffset";
        public const string ContentNoiseEnabled = "ContentNoiseEnabled";
        public const string ContentNoiseScale = "ContentNoiseScale";
        public const string ContentNoiseStrength = "ContentNoiseStrength";
        public const string ContentNoiseOffset = "ContentNoiseOffset";
        public const string PulsateWhenLow = "PulsateWhenLow";
        public const string PulseColor = "PulseColor";
        public const string PulseSpeed = "PulseSpeed";
        public const string PulseActivationThreshold = "PulseActivationThreshold";
        public const string OverlayTexture = "OverlayTexture";
        public const string OverlayTextureOpacity = "OverlayTextureOpacity";
        public const string OverlayTextureTiling = "OverlayTextureTiling";
        public const string OverlayTextureOffset = "OverlayTextureOffset";
        public const string InnerTexture = "InnerTexture";
        public const string AlignInnerTexture = "AlignInnerTexture";
        public const string InnerTextureScaleWithSegments = "InnerTextureScaleWithSegments";
        public const string InnerTextureOpacity = "InnerTextureOpacity";
        public const string InnerTextureTiling = "InnerTextureTiling";
        public const string InnerTextureOffset = "InnerTextureOffset";
        public const string BorderTexture = "BorderTexture";
        public const string AlignBorderTexture = "AlignBorderTexture";
        public const string BorderTextureScaleWithSegments = "BorderTextureScaleWithSegments";
        public const string BorderTextureOpacity = "BorderTextureOpacity";
        public const string BorderTextureTiling = "BorderTextureTiling";
        public const string BorderTextureOffset = "BorderTextureOffset";
        public const string EmptyTexture = "EmptyTexture";
        public const string AlignEmptyTexture = "AlignEmptyTexture";
        public const string EmptyTextureScaleWithSegments = "EmptyTextureScaleWithSegments";
        public const string EmptyTextureOpacity = "EmptyTextureOpacity";
        public const string EmptyTextureTiling = "EmptyTextureTiling";
        public const string EmptyTextureOffset = "EmptyTextureOffset";
        public const string SpaceTexture = "SpaceTexture";
        public const string AlignSpaceTexture = "AlignSpaceTexture";
        public const string SpaceTextureOpacity = "SpaceTextureOpacity";
        public const string SpaceTextureTiling = "SpaceTextureTiling";
        public const string SpaceTextureOffset = "SpaceTextureOffset";
        public const string InnerGradient = "InnerGradient";
        public const string InnerGradientEnabled = "InnerGradientEnabled";
        public const string ValueAsGradientTimeInner = "ValueAsGradientTimeInner";
        public const string EmptyGradient = "EmptyGradient";
        public const string EmptyGradientEnabled = "EmptyGradientEnabled";
        public const string ValueAsGradientTimeEmpty = "ValueAsGradientTimeEmpty";
        public const string VariableWidthCurve = "VariableWidthCurve";
        public const string FillClockwise = "FillClockwise";
    }

    public static class RadialHealthBarKeywords {
        public const string OverlayTextureEnabled = "OverlayTextureEnabled";
        public const string InnerTextureEnabled = "InnerTextureEnabled";
        public const string BorderTextureEnabled = "BorderTextureEnabled";
        public const string EmptyTextureEnabled = "EmptyTextureEnabled";
        public const string SpaceTextureEnabled = "SpaceTextureEnabled";
    }

    #endregion

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Health Bars/Circular Segmented Health Bar")]
    public class RadialSegmentedHealthBar : MonoBehaviour, ISegmentedHealthBar {
        private string oldParentName = "Player";
        [SerializeField] private string parentName = "Player";

        public string ParentName {
            get => parentName;
            set {
                if (Application.isPlaying)
                    StatusBarsManager.RemoveHealthBar(this, false);
                parentName = value;
                if (Application.isPlaying)
                    StatusBarsManager.AddHealthBar(this);
            }
        }

        private string oldHbName = "Primary";
        [SerializeField] private string hbName = "Primary";

        public string Name {
            get => hbName;
            set {
                if (Application.isPlaying)
                    StatusBarsManager.RemoveHealthBar(this, false);
                hbName = value;
                if (Application.isPlaying)
                    StatusBarsManager.AddHealthBar(this);
            }
        }

        [SerializeField] private bool usingSpriteRenderer;

        public bool UsingSpriteRenderer {
            get => usingSpriteRenderer;
            set {
                usingSpriteRenderer = value;
                GenerateRequiredComponents(false);
            }
        }

        [SerializeField] private bool forceBuiltInShader = false;

        public bool ForceBuiltInShader {
            get => forceBuiltInShader;
            set {
                forceBuiltInShader = value;
                GenerateRequiredComponents(true);
            }
        }

        #region Properties

        [field: SerializeField] public ShaderPropertyColor OverlayColor { get; private set; }
        [field: SerializeField] public ShaderPropertyColor InnerColor { get; private set; }
        [field: SerializeField] public ShaderPropertyColor BorderColor { get; private set; }
        [field: SerializeField] public ShaderPropertyColor EmptyColor { get; private set; }
        [field: SerializeField] public ShaderPropertyColor SpaceColor { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat SegmentCount { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat RemoveSegments { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat SegmentSpacing { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat Arc { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat Radius { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat LineWidth { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat Rotation { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 Offset { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat BorderWidth { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat BorderSpacing { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat RemoveBorder { get; private set; }
        [field: SerializeField] public ShaderPropertyBool OverlayNoiseEnabled { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat OverlayNoiseScale { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat OverlayNoiseStrength { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 OverlayNoiseOffset { get; private set; }
        [field: SerializeField] public ShaderPropertyBool EmptyNoiseEnabled { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat EmptyNoiseScale { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat EmptyNoiseStrength { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 EmptyNoiseOffset { get; private set; }
        [field: SerializeField] public ShaderPropertyBool ContentNoiseEnabled { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat ContentNoiseScale { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat ContentNoiseStrength { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 ContentNoiseOffset { get; private set; }
        [field: SerializeField] public ShaderPropertyBool PulsateWhenLow { get; private set; }
        [field: SerializeField] public ShaderPropertyColor PulseColor { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat PulseSpeed { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat PulseActivationThreshold { get; private set; }
        [field: SerializeField] public ShaderKeyword OverlayTextureEnabled { get; private set; }
        [field: SerializeField] public ShaderPropertyTexture2D OverlayTexture { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat OverlayTextureOpacity { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 OverlayTextureTiling { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 OverlayTextureOffset { get; private set; }
        [field: SerializeField] public ShaderKeyword InnerTextureEnabled { get; private set; }
        [field: SerializeField] public ShaderPropertyTexture2D InnerTexture { get; private set; }
        [field: SerializeField] public ShaderPropertyBool AlignInnerTexture { get; private set; }
        [field: SerializeField] public ShaderPropertyBool InnerTextureScaleWithSegments { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat InnerTextureOpacity { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 InnerTextureTiling { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 InnerTextureOffset { get; private set; }
        [field: SerializeField] public ShaderKeyword BorderTextureEnabled { get; private set; }
        [field: SerializeField] public ShaderPropertyTexture2D BorderTexture { get; private set; }
        [field: SerializeField] public ShaderPropertyBool AlignBorderTexture { get; private set; }
        [field: SerializeField] public ShaderPropertyBool BorderTextureScaleWithSegments { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat BorderTextureOpacity { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 BorderTextureTiling { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 BorderTextureOffset { get; private set; }
        [field: SerializeField] public ShaderKeyword EmptyTextureEnabled { get; private set; }
        [field: SerializeField] public ShaderPropertyTexture2D EmptyTexture { get; private set; }
        [field: SerializeField] public ShaderPropertyBool AlignEmptyTexture { get; private set; }
        [field: SerializeField] public ShaderPropertyBool EmptyTextureScaleWithSegments { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat EmptyTextureOpacity { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 EmptyTextureTiling { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 EmptyTextureOffset { get; private set; }
        [field: SerializeField] public ShaderKeyword SpaceTextureEnabled { get; private set; }
        [field: SerializeField] public ShaderPropertyTexture2D SpaceTexture { get; private set; }
        [field: SerializeField] public ShaderPropertyBool AlignSpaceTexture { get; private set; }
        [field: SerializeField] public ShaderPropertyFloat SpaceTextureOpacity { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 SpaceTextureTiling { get; private set; }
        [field: SerializeField] public ShaderPropertyVector2 SpaceTextureOffset { get; private set; }
        [field: SerializeField] public ShaderPropertyGradient InnerGradient { get; private set; }
        [field: SerializeField] public ShaderPropertyBool InnerGradientEnabled { get; private set; }
        [field: SerializeField] public ShaderPropertyBool ValueAsGradientTimeInner { get; private set; }
        [field: SerializeField] public ShaderPropertyGradient EmptyGradient { get; private set; }
        [field: SerializeField] public ShaderPropertyBool EmptyGradientEnabled { get; private set; }
        [field: SerializeField] public ShaderPropertyBool ValueAsGradientTimeEmpty { get; private set; }
        [field: SerializeField] public ShaderPropertyAnimationCurve VariableWidthCurve { get; private set; }
        [field: SerializeField] public ShaderPropertyBool FillClockwise { get; private set; }

        #endregion

        private Material currentMaterial;
        private SpriteRenderer spriteRenderer;
        private Image image;

        private bool materialAssigned = false;

        private Material ActiveMaterial {
            get {
                if (!usingSpriteRenderer) {
                    return image.materialForRendering;
                }
                return currentMaterial;
            }
        }

        private string BaseMaterialName {
            get {
                if (!forceBuiltInShader && GraphicsSettings.renderPipelineAsset && Int32.Parse(Application.unityVersion.Split('.')[0]) > 2019)
                    return "RadialSegmentedHealthBarMaterial";

                return "RadialSegmentedHealthBarMaterialBuiltIn";
            }
        }

        private const string MaterialName = "radialSegmentedHealthBarInstance";
        private const string PlaceholderSpriteName = "rshb_placeholderSprite";

        private SortedDictionary<string, IShaderProperty> properties = new SortedDictionary<string, IShaderProperty>();

        private void Awake() {
            if (Application.isPlaying)
                StatusBarsManager.AddHealthBar(this);

            InitProperties();

            GenerateRequiredComponents(true);
        }

        private void Update() {
            if (usingSpriteRenderer) {
                if (!spriteRenderer || spriteRenderer && (!spriteRenderer.sprite || !spriteRenderer.sharedMaterial || spriteRenderer.sharedMaterial.name != MaterialName)) {
                    GenerateRequiredComponents(false);
                }
            } else if (!image || image && (!image.material || image.material.name != MaterialName)) {
                GenerateRequiredComponents(false);
            }

            if (materialAssigned) {
#if UNITY_EDITOR
                if (oldParentName != parentName || oldHbName != hbName) {
                    StatusBarsManager.RemoveHealthBar(this, oldParentName, oldHbName, false);
                    StatusBarsManager.AddHealthBar(this);
                    oldParentName = parentName;
                    oldHbName = hbName;
                }
#endif
                ApplyToShader(false);
                //ResetMaterialWithSerializedFields(false);
            }
        }

        private void OnValidate() {
            if (properties.Count == 0) {
                InitProperties();
            }
            ApplyToShader(true);
        }

        private void GenerateRequiredComponents(bool useExisting) {
            spriteRenderer = GetComponent<SpriteRenderer>();
            image = GetComponent<Image>();
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            //set up image
            if (useExisting && image || useExisting && parentCanvas || !useExisting && !usingSpriteRenderer) {
                if (parentCanvas?.renderMode == RenderMode.ScreenSpaceOverlay) {
                    forceBuiltInShader = true;
                }
                usingSpriteRenderer = false;
                if (spriteRenderer) DestroyImmediate(spriteRenderer);

                if (!image) {
                    gameObject.AddComponent(typeof(Image));
                    image = GetComponent<Image>();
                    //img.hideFlags = HideFlags.HideInInspector;
                }

                image.sprite = Resources.Load<Sprite>(PlaceholderSpriteName);
                AssignMaterial(image);
            }
            //set up sprite renderer
            else {
                usingSpriteRenderer = true;
                if (image) {
                    DestroyImmediate(image);
                    DestroyImmediate(GetComponent<CanvasRenderer>());
                }

                if (spriteRenderer == null) {
                    gameObject.AddComponent(typeof(SpriteRenderer));
                    spriteRenderer = GetComponent<SpriteRenderer>();
                    //sr.hideFlags = HideFlags.HideInInspector;
                }

                spriteRenderer.sprite = Resources.Load<Sprite>(PlaceholderSpriteName);
                AssignMaterial(spriteRenderer);
            }
        }

        private void InitProperties() {
            bool shouldPassProps = SegmentCount != null && SegmentCount.Value != 0;
            properties = new SortedDictionary<string, IShaderProperty> {
                [RadialHealthBarProperties.OverlayColor] = OverlayColor = new ShaderPropertyColor("_" + RadialHealthBarProperties.OverlayColor, ColorPropertyFunc, Color.white, shouldPassProps ? OverlayColor : null),
                [RadialHealthBarProperties.InnerColor] = InnerColor = new ShaderPropertyColor("_" + RadialHealthBarProperties.InnerColor, ColorPropertyFunc, Color.red, shouldPassProps ? InnerColor : null),
                [RadialHealthBarProperties.BorderColor] = BorderColor = new ShaderPropertyColor("_" + RadialHealthBarProperties.BorderColor, ColorPropertyFunc, Color.white, shouldPassProps ? BorderColor : null),
                [RadialHealthBarProperties.EmptyColor] = EmptyColor = new ShaderPropertyColor("_" + RadialHealthBarProperties.EmptyColor, ColorPropertyFunc, Color.gray, shouldPassProps ? EmptyColor : null),
                [RadialHealthBarProperties.SpaceColor] = SpaceColor = new ShaderPropertyColor("_" + RadialHealthBarProperties.SpaceColor, ColorPropertyFunc, Color.clear, shouldPassProps ? SpaceColor : null),
                [RadialHealthBarProperties.SegmentCount] = SegmentCount = new ShaderPropertyFloat("_" + RadialHealthBarProperties.SegmentCount, FloatPropertyFunc, 5, shouldPassProps ? SegmentCount : null),
                [RadialHealthBarProperties.RemoveSegments] = RemoveSegments = new ShaderPropertyFloat("_" + RadialHealthBarProperties.RemoveSegments, FloatPropertyFunc, 1, shouldPassProps ? RemoveSegments : null),
                [RadialHealthBarProperties.SegmentSpacing] = SegmentSpacing = new ShaderPropertyFloat("_" + RadialHealthBarProperties.SegmentSpacing, FloatPropertyFunc, 0.02f, shouldPassProps ? SegmentSpacing : null),
                [RadialHealthBarProperties.Arc] = Arc = new ShaderPropertyFloat("_" + RadialHealthBarProperties.Arc, FloatPropertyFunc, 0f, shouldPassProps ? Arc : null),
                [RadialHealthBarProperties.Radius] = Radius = new ShaderPropertyFloat("_" + RadialHealthBarProperties.Radius, FloatPropertyFunc, .35f, shouldPassProps ? Radius : null),
                [RadialHealthBarProperties.LineWidth] = LineWidth = new ShaderPropertyFloat("_" + RadialHealthBarProperties.LineWidth, FloatPropertyFunc, .06f, shouldPassProps ? LineWidth : null),
                [RadialHealthBarProperties.Rotation] = Rotation = new ShaderPropertyFloat("_" + RadialHealthBarProperties.Rotation, FloatPropertyFunc, 0, shouldPassProps ? Rotation : null),
                [RadialHealthBarProperties.Offset] = Offset = new ShaderPropertyVector2("_" + RadialHealthBarProperties.Offset, VectorPropertyFunc, new Vector2(), shouldPassProps ? Offset : null),
                [RadialHealthBarProperties.BorderWidth] = BorderWidth = new ShaderPropertyFloat("_" + RadialHealthBarProperties.BorderWidth, FloatPropertyFunc, 0.01f, shouldPassProps ? BorderWidth : null),
                [RadialHealthBarProperties.BorderSpacing] = BorderSpacing = new ShaderPropertyFloat("_" + RadialHealthBarProperties.BorderSpacing, FloatPropertyFunc, 0.01f, shouldPassProps ? BorderSpacing : null),
                [RadialHealthBarProperties.RemoveBorder] = RemoveBorder = new ShaderPropertyFloat("_" + RadialHealthBarProperties.RemoveBorder, FloatPropertyFunc, 1.0f, shouldPassProps ? RemoveBorder : null),
                [RadialHealthBarProperties.OverlayNoiseEnabled] = OverlayNoiseEnabled = new ShaderPropertyBool("_" + RadialHealthBarProperties.OverlayNoiseEnabled, BoolPropertyFunc, false, shouldPassProps ? OverlayNoiseEnabled : null),
                [RadialHealthBarProperties.OverlayNoiseScale] = OverlayNoiseScale = new ShaderPropertyFloat("_" + RadialHealthBarProperties.OverlayNoiseScale, FloatPropertyFunc, 100, shouldPassProps ? OverlayNoiseScale : null),
                [RadialHealthBarProperties.OverlayNoiseStrength] = OverlayNoiseStrength = new ShaderPropertyFloat("_" + RadialHealthBarProperties.OverlayNoiseStrength, FloatPropertyFunc, 0.5f, shouldPassProps ? OverlayNoiseStrength : null),
                [RadialHealthBarProperties.OverlayNoiseOffset] = OverlayNoiseOffset = new ShaderPropertyVector2("_" + RadialHealthBarProperties.OverlayNoiseOffset, VectorPropertyFunc, new Vector2(), shouldPassProps ? OverlayNoiseOffset : null),
                [RadialHealthBarProperties.EmptyNoiseEnabled] = EmptyNoiseEnabled = new ShaderPropertyBool("_" + RadialHealthBarProperties.EmptyNoiseEnabled, BoolPropertyFunc, false, shouldPassProps ? EmptyNoiseEnabled : null),
                [RadialHealthBarProperties.EmptyNoiseScale] = EmptyNoiseScale = new ShaderPropertyFloat("_" + RadialHealthBarProperties.EmptyNoiseScale, FloatPropertyFunc, 100, shouldPassProps ? EmptyNoiseScale : null),
                [RadialHealthBarProperties.EmptyNoiseStrength] = EmptyNoiseStrength = new ShaderPropertyFloat("_" + RadialHealthBarProperties.EmptyNoiseStrength, FloatPropertyFunc, 0.5f, shouldPassProps ? EmptyNoiseStrength : null),
                [RadialHealthBarProperties.EmptyNoiseOffset] = EmptyNoiseOffset = new ShaderPropertyVector2("_" + RadialHealthBarProperties.EmptyNoiseOffset, VectorPropertyFunc, new Vector2(), shouldPassProps ? EmptyNoiseOffset : null),
                [RadialHealthBarProperties.ContentNoiseEnabled] = ContentNoiseEnabled = new ShaderPropertyBool("_" + RadialHealthBarProperties.ContentNoiseEnabled, BoolPropertyFunc, false, shouldPassProps ? ContentNoiseEnabled : null),
                [RadialHealthBarProperties.ContentNoiseScale] = ContentNoiseScale = new ShaderPropertyFloat("_" + RadialHealthBarProperties.ContentNoiseScale, FloatPropertyFunc, 100, shouldPassProps ? ContentNoiseScale : null),
                [RadialHealthBarProperties.ContentNoiseStrength] = ContentNoiseStrength = new ShaderPropertyFloat("_" + RadialHealthBarProperties.ContentNoiseStrength, FloatPropertyFunc, 0.5f, shouldPassProps ? ContentNoiseStrength : null),
                [RadialHealthBarProperties.ContentNoiseOffset] = ContentNoiseOffset = new ShaderPropertyVector2("_" + RadialHealthBarProperties.ContentNoiseOffset, VectorPropertyFunc, new Vector2(), shouldPassProps ? ContentNoiseOffset : null),
                [RadialHealthBarProperties.PulsateWhenLow] = PulsateWhenLow = new ShaderPropertyBool("_" + RadialHealthBarProperties.PulsateWhenLow, BoolPropertyFunc, true, shouldPassProps ? PulsateWhenLow : null),
                [RadialHealthBarProperties.PulseColor] = PulseColor = new ShaderPropertyColor("_" + RadialHealthBarProperties.PulseColor, ColorPropertyFunc, Color.white, shouldPassProps ? PulseColor : null),
                [RadialHealthBarProperties.PulseSpeed] = PulseSpeed = new ShaderPropertyFloat("_" + RadialHealthBarProperties.PulseSpeed, FloatPropertyFunc, 10, shouldPassProps ? PulseSpeed : null),
                [RadialHealthBarProperties.PulseActivationThreshold] = PulseActivationThreshold = new ShaderPropertyFloat("_" + RadialHealthBarProperties.PulseActivationThreshold, FloatPropertyFunc, 0.25f, shouldPassProps ? PulseActivationThreshold : null),
                [RadialHealthBarKeywords.OverlayTextureEnabled] = OverlayTextureEnabled = new ShaderKeyword("OVERLAY_TEXTURE_ON", KeywordFunc, false, shouldPassProps ? OverlayTextureEnabled : null),
                [RadialHealthBarProperties.OverlayTexture] = OverlayTexture = new ShaderPropertyTexture2D("_" + RadialHealthBarProperties.OverlayTexture, TexturePropertyFunc, null, shouldPassProps ? OverlayTexture : null),
                [RadialHealthBarProperties.OverlayTextureOpacity] = OverlayTextureOpacity = new ShaderPropertyFloat("_" + RadialHealthBarProperties.OverlayTextureOpacity, FloatPropertyFunc, 1, shouldPassProps ? OverlayTextureOpacity : null),
                [RadialHealthBarProperties.OverlayTextureTiling] = OverlayTextureTiling = new ShaderPropertyVector2("_" + RadialHealthBarProperties.OverlayTextureTiling, VectorPropertyFunc, new Vector2(1, 1), shouldPassProps ? OverlayTextureTiling : null),
                [RadialHealthBarProperties.OverlayTextureOffset] = OverlayTextureOffset = new ShaderPropertyVector2("_" + RadialHealthBarProperties.OverlayTextureOffset, VectorPropertyFunc, new Vector2(), shouldPassProps ? OverlayTextureOffset : null),
                [RadialHealthBarKeywords.InnerTextureEnabled] = InnerTextureEnabled = new ShaderKeyword("INNER_TEXTURE_ON", KeywordFunc, false, shouldPassProps ? InnerTextureEnabled : null),
                [RadialHealthBarProperties.InnerTexture] = InnerTexture = new ShaderPropertyTexture2D("_" + RadialHealthBarProperties.InnerTexture, TexturePropertyFunc, null, shouldPassProps ? InnerTexture : null),
                [RadialHealthBarProperties.AlignInnerTexture] = AlignInnerTexture = new ShaderPropertyBool("_" + RadialHealthBarProperties.AlignInnerTexture, BoolPropertyFunc, true, shouldPassProps ? AlignInnerTexture : null),
                [RadialHealthBarProperties.InnerTextureScaleWithSegments] = InnerTextureScaleWithSegments = new ShaderPropertyBool("_" + RadialHealthBarProperties.InnerTextureScaleWithSegments, BoolPropertyFunc, true, shouldPassProps ? InnerTextureScaleWithSegments : null),
                [RadialHealthBarProperties.InnerTextureOpacity] = InnerTextureOpacity = new ShaderPropertyFloat("_" + RadialHealthBarProperties.InnerTextureOpacity, FloatPropertyFunc, 1, shouldPassProps ? InnerTextureOpacity : null),
                [RadialHealthBarProperties.InnerTextureTiling] = InnerTextureTiling = new ShaderPropertyVector2("_" + RadialHealthBarProperties.InnerTextureTiling, VectorPropertyFunc, new Vector2(1, 1), shouldPassProps ? InnerTextureTiling : null),
                [RadialHealthBarProperties.InnerTextureOffset] = InnerTextureOffset = new ShaderPropertyVector2("_" + RadialHealthBarProperties.InnerTextureOffset, VectorPropertyFunc, new Vector2(), shouldPassProps ? InnerTextureOffset : null),
                [RadialHealthBarKeywords.BorderTextureEnabled] = BorderTextureEnabled = new ShaderKeyword("BORDER_TEXTURE_ON", KeywordFunc, false, shouldPassProps ? BorderTextureEnabled : null),
                [RadialHealthBarProperties.BorderTexture] = BorderTexture = new ShaderPropertyTexture2D("_" + RadialHealthBarProperties.BorderTexture, TexturePropertyFunc, null, shouldPassProps ? BorderTexture : null),
                [RadialHealthBarProperties.AlignBorderTexture] = AlignBorderTexture = new ShaderPropertyBool("_" + RadialHealthBarProperties.AlignBorderTexture, BoolPropertyFunc, true, shouldPassProps ? AlignBorderTexture : null),
                [RadialHealthBarProperties.BorderTextureScaleWithSegments] = BorderTextureScaleWithSegments = new ShaderPropertyBool("_" + RadialHealthBarProperties.BorderTextureScaleWithSegments, BoolPropertyFunc, true, shouldPassProps ? BorderTextureScaleWithSegments : null),
                [RadialHealthBarProperties.BorderTextureOpacity] = BorderTextureOpacity = new ShaderPropertyFloat("_" + RadialHealthBarProperties.BorderTextureOpacity, FloatPropertyFunc, 1, shouldPassProps ? BorderTextureOpacity : null),
                [RadialHealthBarProperties.BorderTextureTiling] = BorderTextureTiling = new ShaderPropertyVector2("_" + RadialHealthBarProperties.BorderTextureTiling, VectorPropertyFunc, new Vector2(1, 1), shouldPassProps ? BorderTextureTiling : null),
                [RadialHealthBarProperties.BorderTextureOffset] = BorderTextureOffset = new ShaderPropertyVector2("_" + RadialHealthBarProperties.BorderTextureOffset, VectorPropertyFunc, new Vector2(), shouldPassProps ? BorderTextureOffset : null),
                [RadialHealthBarKeywords.EmptyTextureEnabled] = EmptyTextureEnabled = new ShaderKeyword("EMPTY_TEXTURE_ON", KeywordFunc, false, shouldPassProps ? EmptyTextureEnabled : null),
                [RadialHealthBarProperties.EmptyTexture] = EmptyTexture = new ShaderPropertyTexture2D("_" + RadialHealthBarProperties.EmptyTexture, TexturePropertyFunc, null, shouldPassProps ? EmptyTexture : null),
                [RadialHealthBarProperties.AlignEmptyTexture] = AlignEmptyTexture = new ShaderPropertyBool("_" + RadialHealthBarProperties.AlignEmptyTexture, BoolPropertyFunc, true, shouldPassProps ? AlignEmptyTexture : null),
                [RadialHealthBarProperties.EmptyTextureScaleWithSegments] = EmptyTextureScaleWithSegments = new ShaderPropertyBool("_" + RadialHealthBarProperties.EmptyTextureScaleWithSegments, BoolPropertyFunc, true, shouldPassProps ? EmptyTextureScaleWithSegments : null),
                [RadialHealthBarProperties.EmptyTextureOpacity] = EmptyTextureOpacity = new ShaderPropertyFloat("_" + RadialHealthBarProperties.EmptyTextureOpacity, FloatPropertyFunc, 1, shouldPassProps ? EmptyTextureOpacity : null),
                [RadialHealthBarProperties.EmptyTextureTiling] = EmptyTextureTiling = new ShaderPropertyVector2("_" + RadialHealthBarProperties.EmptyTextureTiling, VectorPropertyFunc, new Vector2(1, 1), shouldPassProps ? EmptyTextureTiling : null),
                [RadialHealthBarProperties.EmptyTextureOffset] = EmptyTextureOffset = new ShaderPropertyVector2("_" + RadialHealthBarProperties.EmptyTextureOffset, VectorPropertyFunc, new Vector2(), shouldPassProps ? EmptyTextureOffset : null),
                [RadialHealthBarKeywords.SpaceTextureEnabled] = SpaceTextureEnabled = new ShaderKeyword("SPACE_TEXTURE_ON", KeywordFunc, false, shouldPassProps ? SpaceTextureEnabled : null),
                [RadialHealthBarProperties.SpaceTexture] = SpaceTexture = new ShaderPropertyTexture2D("_" + RadialHealthBarProperties.SpaceTexture, TexturePropertyFunc, null, shouldPassProps ? SpaceTexture : null),
                [RadialHealthBarProperties.AlignSpaceTexture] = AlignSpaceTexture = new ShaderPropertyBool("_" + RadialHealthBarProperties.AlignSpaceTexture, BoolPropertyFunc, true, shouldPassProps ? AlignSpaceTexture : null),
                [RadialHealthBarProperties.SpaceTextureOpacity] = SpaceTextureOpacity = new ShaderPropertyFloat("_" + RadialHealthBarProperties.SpaceTextureOpacity, FloatPropertyFunc, 1, shouldPassProps ? SpaceTextureOpacity : null),
                [RadialHealthBarProperties.SpaceTextureTiling] = SpaceTextureTiling = new ShaderPropertyVector2("_" + RadialHealthBarProperties.SpaceTextureTiling, VectorPropertyFunc, new Vector2(1, 1), shouldPassProps ? SpaceTextureTiling : null),
                [RadialHealthBarProperties.SpaceTextureOffset] = SpaceTextureOffset = new ShaderPropertyVector2("_" + RadialHealthBarProperties.SpaceTextureOffset, VectorPropertyFunc, new Vector2(), shouldPassProps ? SpaceTextureOffset : null),
                [RadialHealthBarProperties.InnerGradient] = InnerGradient = new ShaderPropertyGradient("_" + RadialHealthBarProperties.InnerGradient, GradientPropertyFunc, null, shouldPassProps ? InnerGradient : null),
                [RadialHealthBarProperties.InnerGradientEnabled] = InnerGradientEnabled = new ShaderPropertyBool("_" + RadialHealthBarProperties.InnerGradientEnabled, BoolPropertyFunc, true, shouldPassProps ? InnerGradientEnabled : null),
                [RadialHealthBarProperties.ValueAsGradientTimeInner] = ValueAsGradientTimeInner = new ShaderPropertyBool("_" + RadialHealthBarProperties.ValueAsGradientTimeInner, BoolPropertyFunc, false, shouldPassProps ? ValueAsGradientTimeInner : null),
                [RadialHealthBarProperties.EmptyGradient] = EmptyGradient = new ShaderPropertyGradient("_" + RadialHealthBarProperties.EmptyGradient, GradientPropertyFunc, null, shouldPassProps ? EmptyGradient : null),
                [RadialHealthBarProperties.EmptyGradientEnabled] = EmptyGradientEnabled = new ShaderPropertyBool("_" + RadialHealthBarProperties.EmptyGradientEnabled, BoolPropertyFunc, false, shouldPassProps ? EmptyGradientEnabled : null),
                [RadialHealthBarProperties.ValueAsGradientTimeEmpty] = ValueAsGradientTimeEmpty = new ShaderPropertyBool("_" + RadialHealthBarProperties.ValueAsGradientTimeEmpty, BoolPropertyFunc, false, shouldPassProps ? ValueAsGradientTimeEmpty : null),
                [RadialHealthBarProperties.VariableWidthCurve] = VariableWidthCurve = new ShaderPropertyAnimationCurve("_" + RadialHealthBarProperties.VariableWidthCurve, CurvePropertyFunc, new AnimationCurve(new[] { new Keyframe(0, 1) }), shouldPassProps ? VariableWidthCurve : null),
                [RadialHealthBarProperties.FillClockwise] = FillClockwise = new ShaderPropertyBool("_" + RadialHealthBarProperties.FillClockwise, BoolPropertyFunc, false, shouldPassProps ? FillClockwise : null)
            };
        }

        #region Getters and Setters

        public bool GetShaderProperty<T>(string propertyName, out ShaderProperty<T> shaderProperty) {
            if (properties[propertyName] is ShaderProperty<T> p) {
                shaderProperty = p;
                return true;
            }

            shaderProperty = null;
            return false;
        }

        public bool GetShaderKeyword(string propertyName, out ShaderKeyword shaderKeyword) {
            if (properties[propertyName] is ShaderKeyword p) {
                shaderKeyword = p;
                return true;
            }

            shaderKeyword = null;
            return false;
        }

        public bool GetShaderPropertyValue<T>(string propertyName, out T value) {
            if (properties[propertyName] is ShaderProperty<T> p) {
                value = p.Value;
                return true;
            }

            value = default;
            return false;
        }

        public bool SetShaderPropertyValue<T>(string propertyName, T value) {
            if (properties[propertyName] is ShaderProperty<T> p) {
                p.Value = value;
                return true;
            }

            return false;
        }

        public bool GetShaderKeywordValue(string propertyName, out bool value) {
            if (properties[propertyName] is ShaderKeyword p) {
                value = p.Value;
                return true;
            }

            value = default;
            return false;
        }

        public bool SetShaderKeywordValue(string propertyName, bool value) {
            if (properties[propertyName] is ShaderKeyword p) {
                p.Value = value;
                return true;
            }

            return false;
        }

        #endregion

        #region Property Functions
        private bool BoolPropertyFunc(int id, bool setInShader, bool value) {
            if (materialAssigned && !setInShader)
                return Convert.ToBoolean(ActiveMaterial.GetFloat(id));
            if (materialAssigned && setInShader) {
                ActiveMaterial.SetFloat(id, value ? 1 : 0);
            }

            return default;
        }

        private float FloatPropertyFunc(int id, bool setInShader, float value) {
            if (materialAssigned && !setInShader)
                return ActiveMaterial.GetFloat(id);
            if (materialAssigned && setInShader) {
                ActiveMaterial.SetFloat(id, value);
            }

            return default;
        }

        private Color ColorPropertyFunc(int id, bool setInShader, Color value) {
            if (materialAssigned && !setInShader)
                return ActiveMaterial.GetColor(id);
            if (materialAssigned && setInShader) {
                ActiveMaterial.SetColor(id, value);
            }

            return default;
        }

        private Vector2 VectorPropertyFunc(int id, bool setInShader, Vector2 value) {
            if (materialAssigned && !setInShader)
                return ActiveMaterial.GetVector(id);
            if (materialAssigned && setInShader) {
                ActiveMaterial.SetVector(id, value);
            }

            return default;
        }

        private Texture2D TexturePropertyFunc(int id, bool setInShader, Texture2D value) {
            if (materialAssigned && !setInShader)
                return (Texture2D)ActiveMaterial.GetTexture(id);
            if (materialAssigned && setInShader) {
                ActiveMaterial.SetTexture(id, value);
            }

            return default;
        }

        private Gradient GradientPropertyFunc(int id, bool setInShader, Gradient value) {
            if (materialAssigned && !setInShader)
                return value;
            if (materialAssigned && setInShader) {
                ActiveMaterial.SetTexture(id, value.ToTexture2D());
            }

            return default;
        }

        private AnimationCurve CurvePropertyFunc(int id, bool setInShader, AnimationCurve value) {
            if (materialAssigned && !setInShader)
                return value;
            if (materialAssigned && setInShader) {
                ActiveMaterial.SetTexture(id, value.ToTexture2D());
            }

            return default;
        }

        private bool KeywordFunc(string id, bool setInShader, bool value) {
            if (materialAssigned && !setInShader) 
                return ActiveMaterial.IsKeywordEnabled(id);
            if (materialAssigned && setInShader && value) 
                ActiveMaterial.EnableKeyword(id);
            else if (materialAssigned && setInShader && !value) 
                ActiveMaterial.DisableKeyword(id);
            return false;
        }

        #endregion

        void AssignMaterial(Image r) {
            //get material
            Material mat = Resources.Load<Material>(BaseMaterialName);

            if (mat != null && r != null) {
                //generate and apply the material
                currentMaterial = new Material(mat);
                currentMaterial.name = MaterialName;
                r.material = currentMaterial;
                materialAssigned = true;
                ApplyToShader(true);
#if UNITY_EDITOR
                //the scene needs to be saved
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
            } else {
                //something went wrong, remove the component
                Debug.LogError("RadialSegmentedHealthBar: Something went wrong.");
                DestroyImmediate(this);
            }
        }

        void AssignMaterial(SpriteRenderer r) {
            //get resources
            Material mat = Resources.Load<Material>(BaseMaterialName);
            Sprite sprite = Resources.Load<Sprite>(PlaceholderSpriteName);

            if (mat != null && r != null) {
                //make sure the sprite will render the shader correctly
                if (r.sprite == null && sprite != null) {
                    r.sprite = sprite;
                }

                r.drawMode = SpriteDrawMode.Simple;

                //generate and apply the material
                currentMaterial = new Material(mat);
                currentMaterial.name = MaterialName;
                r.sharedMaterial = currentMaterial;
                materialAssigned = true;
                ApplyToShader(true);
#if UNITY_EDITOR
                //the scene needs to be saved
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
            } else {
                //something went wrong, remove the component
                Debug.LogError("RadialSegmentedHealthBar: Something went wrong.");
                DestroyImmediate(this);
            }
        }

        void ApplyToShader(bool ignoreDirty) {
            foreach (var property in properties) {
                property.Value.ApplyToShader(ignoreDirty);
            }
        }

        void ResetPublicFields() {
            foreach (var property in properties) {
                property.Value.Reset();
            }
        }
        public void ResetPropertiesToDefault() {
            foreach (var item in properties) {
                item.Value.ResetToDefault();
            }
            ApplyToShader(false);
        }

        public void SetSegmentCount(float value) {
            SegmentCount.Value = Mathf.Max(0, value);
        }

        public void SetRemovedSegments(float value) {
            RemoveSegments.Value = Mathf.Clamp(value, 0, SegmentCount.Value);
        }

        public void SetPercent(float value) {
            float cVal = Mathf.Clamp(value, 0, 1);
            RemoveSegments.Value = (1 - cVal) * SegmentCount.Value;
        }

        public void AddRemoveSegments(float value) {
            RemoveSegments.Value += value;
            RemoveSegments.Value = Mathf.Clamp(RemoveSegments.Value, 0, SegmentCount.Value);
        }

        public void AddRemovePercent(float value) {
            RemoveSegments.Value += value * SegmentCount.Value;
            RemoveSegments.Value = Mathf.Clamp(RemoveSegments.Value, 0, SegmentCount.Value);
        }

        public string GetParentName() {
            return parentName;
        }

        public string GetName() {
            return hbName;
        }
    }
}