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
        bool isPlayer = other.CompareTag("Player");
        bool isHand = other.CompareTag("RightHand") || other.CompareTag("LeftHand");

        if ((isPlayer || isHand) && !isWarning)
        {
            isWarning = true;

            // 🔊 NPC konuşur
            guideManager.Talk(warningSound);

            objectMaterial.color = warningColor;
            Invoke(nameof(FixColor), 3f);
        }
    }

    void FixColor()
    {
        objectMaterial.color = originalColor;
        isWarning = false;
    }
}
