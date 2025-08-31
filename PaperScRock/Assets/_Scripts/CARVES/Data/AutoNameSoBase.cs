using System.IO;
using CARVES.Utls;
using UnityEditor;
using UnityEngine;

namespace CARVES.Data
{
    /// <summary>
    /// 自动文件名SO
    /// </summary>
    public class AutoUnderscoreNamingObject : AutoNameWithSeparatorSoBase
    {
        const char Underscore = '_';
        protected override char Separator => Underscore;
    }

    public class AutoDashNamingObject : AutoNameWithSeparatorSoBase
    {
        const char Dash = '-';
        protected override char Separator => Dash;
    }

    public class AutoBacktickNamingObject : AutoNameWithSeparatorSoBase
    {
        const char Backtick = '`';
        protected override char Separator => Backtick;
    }

    public class AutoHashNamingObject : AutoNameWithSeparatorSoBase
    {
        const char Hash = '#';
        protected override char Separator => Hash;
    }

    public class AutoAtNamingObject : AutoNameWithSeparatorSoBase
    {
        const char At = '@';
        protected override char Separator => At;
    }

    public abstract class AutoNameWithSeparatorSoBase : AutoNameSoBase, IDataElement
    {
        public virtual int Id => id;
        [SerializeField] public int id;

        protected abstract char Separator { get; }
        protected override string GetName() => string.Join(Separator, id, base.GetName());
    }

    public abstract class AutoNameSoBase : ScriptableObject
    {
#if UNITY_EDITOR
        [XReadOnly,SerializeField] ScriptableObject referenceSo;

        void OnValidate()
        {
            if (!referenceSo) referenceSo = this;
            if (!Application.isPlaying)
                RenameAssetIfNeeded();
        }

        void RenameAssetIfNeeded()
        {
            var desiredName = GetName();
            if (string.IsNullOrWhiteSpace(desiredName)) return;

            var path = AssetDatabase.GetAssetPath(this);
            var currentName = Path.GetFileNameWithoutExtension(path);

            // ***关键：只有名字不同才动手改***
            if (currentName == desiredName) return;

            // 用 delayCall 避免因重载再触发 OnValidate 的嵌套
            EditorApplication.delayCall += () =>
            {
                var err = AssetDatabase.RenameAsset(path, desiredName);
                if (!string.IsNullOrEmpty(err))
                    Debug.LogError($"Auto rename failed: {err}");
            };
        }
#endif

        protected virtual string Prefix  => string.Empty;
        protected virtual string Suffix  => string.Empty;

        [SerializeField] public string _name;
        public virtual  string  Name => _name;

        protected virtual string GetName() => $"{Prefix}{Name}{Suffix}";
    }
    public abstract class ReferenceSoBase : ScriptableObject
    {
#if UNITY_EDITOR
        [XReadOnly,SerializeField] ScriptableObject referenceSo;
        void OnValidate()
        {
            // 1. 把自身记录到 referenceSo，起到原来 GetReference 的作用
            if (referenceSo == null) referenceSo = this;
        }
#endif
    }
}