using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RengeGames.HealthBars {

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Health Bars/Compound Health Bar")]
    
    public class CompoundHealthBar : MonoBehaviour, ISegmentedHealthBar {


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

        [SerializeField] private List<ISegmentedHealthBar> healthBars;
        [Space]
        [SerializeField] bool segmentsToggle = true;
        [Range(0, 1)][SerializeField] private float value;
        [SerializeField] private int removedSegments = 0;
        [SerializeField] bool fillFrontToBack = true;
        [SerializeField] bool useGradient = true;
        [SerializeField] Gradient gradient;

        [Space]
        [SerializeField] private List<string> healthBarNames;

        private void Awake() {
            if (Application.isPlaying)
                StatusBarsManager.AddHealthBar(this);
        }

        private void Start() {
            Populate();
        }

        private void Update() {
#if UNITY_EDITOR
            if (healthBars == null || transform.childCount != healthBars.Count) {
                Populate();
            }
            if (Application.isPlaying && (oldParentName != parentName || oldHbName != hbName)) {
                StatusBarsManager.RemoveHealthBar(this, oldParentName, oldHbName, false);
                StatusBarsManager.AddHealthBar(this);
                oldParentName = parentName;
                oldHbName = hbName;
            }
#endif
        }

        private void OnValidate() {
            Populate();
            if (healthBars != null) {
                if(segmentsToggle)
                    SetRemovedSegments(removedSegments);
                else {
                    SetPercent(value);
                }
            }
        }

        private void Populate() {
            healthBars = new List<ISegmentedHealthBar>();
            healthBarNames = new List<string>();
            foreach(Transform healthBar in transform) {
                var hb = healthBar.GetComponent<RadialSegmentedHealthBar>();
                hb.ParentName = "";
                hb.Name = "";
                healthBars.Add(hb);
                healthBarNames.Add(healthBar.gameObject.name);
            }
            if (!fillFrontToBack) {
                healthBars.Reverse();
                healthBarNames.Reverse();
            }
        }


        public void AddRemovePercent(float value) {
            this.value = Mathf.Clamp01(this.value + value);
            SetPercent(this.value);
        }


        public string GetName() {
            return hbName;
        }

        public string GetParentName() {
            return parentName;
        }


        public void SetPercent(float value) {
            if (useGradient && value != 0) {
                foreach (var healthBar in healthBars) {
                    var hb = healthBar as RadialSegmentedHealthBar;
                    hb.InnerColor.Value = gradient.Evaluate(value);
                }
            }
            float division = 1.0f / healthBars.Count;
            int activeBarIndex = (int)Mathf.Clamp(Mathf.Ceil(value / division) - 1, 0, float.MaxValue);

            float v1 = 1.0f / division;
            float v2 = value % division == 0 ? (value == 0 ? 0 : 1) : value % division;

            healthBars[activeBarIndex].SetPercent(v1 * v2);
            for(int i = 0; i < activeBarIndex; i++) {
                healthBars[i].SetPercent(1);
            }
            for (int i = activeBarIndex + 1; i < healthBars.Count; i++) {
                healthBars[i].SetPercent(0);
            }
        }
        public void AddRemoveSegments(float value) {
            removedSegments = (int)Mathf.Clamp(removedSegments + value, 0, TotalSegmentCount());
        }

        public void SetRemovedSegments(float value) {
            int remSegCount = (int)value;
            for(int i = healthBars.Count-1; i >= 0; i--) {
                var hb = healthBars[i];
                if (remSegCount == 0) {
                    hb.SetRemovedSegments(0);
                    break;
                }
                hb.SetRemovedSegments(remSegCount);
                float remainingSegments = SegmentCount(hb);
                remSegCount = (int)Mathf.Clamp(remSegCount - remainingSegments, 0, float.MaxValue);
            }
        }

        float TotalSegmentCount() {
            return healthBars.Aggregate(0, (sum, next) => sum += (int)(next as RadialSegmentedHealthBar).SegmentCount.Value);
        }

        float SegmentCount(ISegmentedHealthBar hb) {
            return (hb as RadialSegmentedHealthBar).SegmentCount.Value;
        }



        #region unused methods
        public bool SetShaderKeywordValue(string propertyName, bool value) {
            return false;
        }

        public bool SetShaderPropertyValue<T>(string propertyName, T value) {
            return false;
        }
        public void SetSegmentCount(float value) {
            
        }
        public bool GetShaderKeyword(string propertyName, out ShaderKeyword shaderKeyword) {
            shaderKeyword = null;
            return false;
        }

        public bool GetShaderKeywordValue(string propertyName, out bool value) {
            value = false;
            return false;
        }

        public bool GetShaderProperty<T>(string propertyName, out ShaderProperty<T> shaderProperty) {
            shaderProperty = null;
            return false;
        }

        public bool GetShaderPropertyValue<T>(string propertyName, out T value) {
            value = default;
            return false;
        }
        #endregion
    }
}