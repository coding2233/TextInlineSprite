using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpriteGraphic : MaskableGraphic {

    public SpriteAsset m_spriteAsset;
    public override Texture mainTexture
    {
        get
        {
            if (m_spriteAsset == null|| m_spriteAsset.texSource==null)
                return s_WhiteTexture;
            else
                return m_spriteAsset.texSource;
        }
    }
    
    protected override void OnEnable()
    {
        //不调用父类的OnEnable 他默认会渲染整张图片
        // base.OnEnable();  
    }
    

#if UNITY_EDITOR
    //在编辑器下 
    protected override void OnValidate()
    {
     //   base.OnValidate();
    }
#endif

    protected override void OnRectTransformDimensionsChange()
    {
        // base.OnRectTransformDimensionsChange();
    }

    /// <summary>
    /// 绘制后 需要更新材质
    /// </summary>
    public new void UpdateMaterial()
    {
        base.UpdateMaterial();
    }
}
