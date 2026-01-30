using System.Collections.Generic;
using MackySoft.SerializeReferenceExtensions.Editor;
using UnityEditor;
using UnityEngine;

namespace HexTecGames.BuildHelper.Editor
{
    [System.Serializable]
    public abstract class SettingOverride
    {
        public abstract void ApplyBeforeBuild(BuildData buildData, bool isActive);
        public virtual void ApplyAfterBuild(BuildData buildData, bool success)
        { 
        }
        public bool CheckValidity(Platform platform)
        {
            if (!IsValid(platform))
            {
                Debug.Log(GetInvalidMessage());
                return false;
            }
            return true;
        }
        protected virtual bool IsValid(Platform platform)
        {
            return true;
        }
        protected virtual string GetInvalidMessage()
        {
            return string.Empty;
        }
    }
}