using UnityEngine.UI;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 类似NGUI 的 UISprite。
/// 实现方式是在编辑器模式下，会把图集里的小图事先序列化到一个UGUIAtlas类里。节省了在运行时动态load小图的性能消耗。
/// 使用指南：
/// 1、只需要设置 SpriteName 为你想要的小图名字即可实现动态换图（只限本图集内换图）。
/// 2、把图集拖入 _atlasTexture 字段，如果没有拖入图集，编辑器强制不让拖入任何小图。
/// 3、强制只能拖入本图集里的小图，如果拖入别的图集的小图，会有警告，而且不会生效。
/// 4、如果在运行期要使用别的图集的小图，本类提供 SetSpriteFromAnotherAtlas 方法来达到这个效果。
/// 
/// 如果有bug或拓展需求，请联系 古光仁
/// </summary>
[AddComponentMenu("UI/17zuoye/UIImage")]
public class UIImage : Image
{
    [SerializeField]
    private UGUIAtlas _uiAtlas;
    public UGUIAtlas UIAtlas
    {
        get { return _uiAtlas; }
        set { _uiAtlas = value; }
    }

    private string _spriteName;

    public string SpriteName
    {
        get
        {
            return _spriteName;
        }
        set
        {
            if (null == _uiAtlas)
            {
                sprite = null;
                return;
            }
            _spriteName = value;
            sprite = _uiAtlas.GetSprite(_spriteName);
        }
    }

    public float Alpha
    {
        get
        {
            return color.a;
        }
        set
        {
            color = new Color(color.r, color.g, color.b, value);
        }
    }

    /// <summary>
    /// 跨图集换图
    /// </summary>
    /// <param name="fullAtlasPath">图集在Assetbundle的全路径</param>
    /// <param name="spriteName">小图的名字</param>
    public void SetSpriteFromAnotherAtlas(string fullAtlasPath, string spriteName)
    {
        //TODO:具体功能需要等AssetBundle功能完善后补上
    }

    //只在编辑器下能够有访问权限
    public Texture _atlasTexture;
}