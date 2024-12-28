using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WMSModManager.Patches
{
    public class FixShaders 
    {
        public static void FixShadersOnObject(GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                FixShaderForMaterials(renderer.sharedMaterials);
            }

            var tmps = gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                FixShaderForMaterials(tmp.material);
                FixShaderForMaterials(tmp.materialForRendering);
            }

            var spritesRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var spriteRenderer in spritesRenderers)
            {
                FixShaderForMaterials(spriteRenderer.sharedMaterials);
            }

            var images = gameObject.GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                FixShaderForMaterials(image.material);
            }

            var particleSystemRenderers = gameObject.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var particleSystemRenderer in particleSystemRenderers)
            {
                FixShaderForMaterials(particleSystemRenderer.sharedMaterials);
            }

            var particles = gameObject.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var particle in particles)
            {
                var renderer = particle.GetComponent<Renderer>();
                if (renderer != null) FixShaderForMaterials(renderer.sharedMaterials);
            }
        }

        public static void FixShaderForMaterials(Material[] sharedMaterials)
        {
            foreach (var sharedMaterial in sharedMaterials)
            {
                sharedMaterial.shader = Shader.Find(sharedMaterial.shader.name);
            }
        }

        public static void FixShaderForMaterials(Material sharedMaterial)
        {
            sharedMaterial.shader = Shader.Find(sharedMaterial.shader.name);
        }
    }
}
