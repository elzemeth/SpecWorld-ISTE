using UnityEngine;

public class DangerousObject : MonoBehaviour
{
    public GuideManager guideManager;
    public AudioClip warningSound;
    public Color warningColor = Color.red;

    Material objectMaterial;
    Color originalColor;
    bool isWarning = false;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();

        objectMaterial = rend.material;
        originalColor = objectMaterial.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isWarning)
        {
            isWarning = true;

            guideManager.Talk(warningSound);

            objectMaterial.color = warningColor;

            Invoke("FixColor", 3f);
        }
    }

    public void FixColor()
    {
        objectMaterial.color = originalColor;
        isWarning = false;
    }
}
