using UnityEngine;
using UnityEngine.UI;

namespace Tenacious.Shaders
{
    [ExecuteInEditMode]
    [AddComponentMenu("Tenacious/Shaders/ImageAlphaMask")]
    [RequireComponent(typeof(Image))]
    public class ImageAlphaMaskShaderController : MonoBehaviour
    {
        [SerializeField] [Range(0, 1f)] private float maskValue;
        [SerializeField] private Color maskColor = Color.black;
        [SerializeField] private Texture2D maskTexture;
        [SerializeField] private bool maskInvert;
        [SerializeField] private bool maskIgnoreImageAlpha;

        private Shader shader;
        private Material material;
        private Image image;

        private void Awake()
        {
            image = this.GetComponent<Image>();
            shader = Shader.Find("Tenacious/ImageAlphaMask");
            CreateMaterial();

            // Disable the image effect if the shader can't run on the users graphics card
            this.enabled = shader != null && shader.isSupported;
        }

        public float MaskValue 
        {
            get { return maskValue; }
            set { maskValue = value; }
        }

        public Texture2D MaskTexture
        {
            get { return maskTexture; }
            set { maskTexture = value; }
        }

        private void OnEnable()
        {
            CreateMaterial();
        }

        private void OnDisable()
        {
            if (material) DestroyImmediate(material); // we use DestroyImmediate because we allow script execution in editor mode ([ExecuteInEditMode])
            image.material = null;
        }

        private void CreateMaterial()
        {
            if (material == null)
            {
                material = new Material(shader);
                image.material = material;
                image.material.hideFlags = HideFlags.HideAndDontSave; // disable material editing in editor
            }
        }

        private void Update()
        {
            image.material.SetTexture("_MainTex", image.mainTexture);
            image.material.SetTexture("_MaskTex", maskTexture);
            image.material.SetColor("_MaskColor", maskColor);
            image.material.SetFloat("_MaskValue", maskValue);
            image.material.SetFloat("_Mask_Ignore_Image_Alpha", System.Convert.ToSingle(maskIgnoreImageAlpha));

            if (image.material.IsKeywordEnabled("INVERT_MASK") != maskInvert)
            {
                if (maskInvert)
                    image.material.EnableKeyword("INVERT_MASK");
                else
                    image.material.DisableKeyword("INVERT_MASK");
            }
        }
    }
}