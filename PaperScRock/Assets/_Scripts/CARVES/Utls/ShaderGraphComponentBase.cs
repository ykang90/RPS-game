using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CARVES.Utls
{
    [RequireComponent(typeof(Renderer))]public class ShaderGraphComponentBase : MonoBehaviour
    {
        [Header("用于跟ShaderGraph配合，把ShaderGraph的值绑定设置"),SerializeField] protected Renderer render;
        [SerializeField] protected List<ShaderProperty> ShaderProperties;
        public bool UsePropBlock;
        void Awake()
        {
            for (var i = 0; i < ShaderProperties.Count; i++)
            {
                var prop = ShaderProperties[i];
                if (prop.UseShaderDefault)
                    switch (prop.Type)
                    {
                        case ShaderPropertyType.Float: prop.Float = GetFloat(i); break;
                        case ShaderPropertyType.Vector: prop.Vec = GetVector(i); break;
                        case ShaderPropertyType.Color: prop.Col = GetColor(i); break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }
        }

        void Start()
        {
            UpdateProperties();//update once
            OnStart();
        }

        protected virtual void OnStart()
        {
        }

        // 提供通用的设置方法
        public void SetProperty(string propertyName, object value)
        {
            var prop = ShaderProperties.FirstOrDefault(p => p.Name == propertyName);
            if (prop == null)
            {
                $"Property {propertyName} not found".Log(this, LogType.Warning);
                return;
            }
            prop.ApplyProperty(render, value, this, UsePropBlock);
        }

        public void UpdateProperties()
        {
            if (!Application.isPlaying)
            {
                "Editor mode will not update shader!".Log(this, LogType.Warning);
                return;
            }
            foreach (var prop in ShaderProperties)
                prop.ApplyProperty(render, prop.Value, this, UsePropBlock);
        }
        protected float GetFloat(int index) => render.material.GetFloat(ShaderProperties[index].Name);
        protected void SetFloat(int index, float value) => render.material.SetFloat(ShaderProperties[index].Name, value);
        protected Vector4 GetVector(int index) => render.material.GetVector(ShaderProperties[index].Name);
        protected void SetVector(int index, Vector4 value) => render.material.SetVector(ShaderProperties[index].Name, value);
        protected Color GetColor(int index) => render.material.GetColor(ShaderProperties[index].Name);
        protected void SetColor(int index, Color value) => render.material.SetColor(ShaderProperties[index].Name, value);
        protected Texture GetTexture(int index) => render.material.GetTexture(ShaderProperties[index].Name);
        protected void SetTexture(int index, Texture value) => render.material.SetTexture(ShaderProperties[index].Name, value);
        [Serializable]
        protected class ShaderProperty
        {
            public string Name;
            public ShaderPropertyType Type;
            public bool UseShaderDefault;
#if UNITY_EDITOR
            [HideInEditorMode,OnValueChanged("@((ShaderGraphComponentBase)$property.Tree.WeakTargets[0]).UpdateProperties()")]
#endif
            [ShowIf(nameof(Type),ShaderPropertyType.Float)]public float Float;
#if UNITY_EDITOR
            [HideInEditorMode,OnValueChanged("@((ShaderGraphComponentBase)$property.Tree.WeakTargets[0]).UpdateProperties()")]
#endif
            [ShowIf(nameof(Type),ShaderPropertyType.Vector)]public Vector4 Vec;
#if UNITY_EDITOR
            [HideInEditorMode,OnValueChanged("@((ShaderGraphComponentBase)$property.Tree.WeakTargets[0]).UpdateProperties()")]
#endif
            [ColorUsage(true, true),ShowIf(nameof(Type),ShaderPropertyType.Color)] public Color Col;

            public object Value => Type switch
            {
                ShaderPropertyType.Float => Float,
                ShaderPropertyType.Vector => Vec,
                ShaderPropertyType.Color => Col,
                _ => throw new ArgumentOutOfRangeException()
            };

            public void ApplyProperty(Renderer renderer, object value, Component obj,bool propertyBlock)
            {
                if (propertyBlock)
                {
                    var pb = new MaterialPropertyBlock();
                    switch (Type)
                    {
                        case ShaderPropertyType.Float:
                            if (value is float floatValue)
                                pb.SetFloat(Name, floatValue);
                            else
                                $"Value for {Name} is not a float".Log(obj);
                            break;
                        case ShaderPropertyType.Vector:
                            if (value is Vector4 vectorValue)
                                pb.SetVector(Name, vectorValue);
                            else
                                $"Value for {Name} is not a Vector4".Log(obj);
                            break;
                        case ShaderPropertyType.Color:
                            if (value is Color colorValue)
                                pb.SetColor(Name, colorValue);
                            else
                                $"Value for {Name} is not a Color".Log(obj);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    renderer.SetPropertyBlock(pb);
                }
                else
                {
                    switch (Type)
                    {
                        case ShaderPropertyType.Float:
                            if (value is float floatValue)
                                renderer.material.SetFloat(Name, floatValue);
                            else
                                $"Value for {Name} is not a float".Log(obj);

                            break;
                        case ShaderPropertyType.Vector:
                            if (value is Vector4 vectorValue)
                                renderer.material.SetVector(Name, vectorValue);
                            else
                                $"Value for {Name} is not a Vector4".Log(obj);
                            break;
                        case ShaderPropertyType.Color:
                            if (value is Color colorValue)
                                renderer.material.SetColor(Name, colorValue);
                            else
                                $"Value for {Name} is not a Color".Log(obj);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        protected enum ShaderPropertyType
        {
            Float,
            Vector,
            Color,
        }
    }
}