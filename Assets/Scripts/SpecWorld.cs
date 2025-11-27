using UnityEngine;

public class SpecWorld : MonoBehaviour
{
    [SerializeField] private StoryController story;
    private void Start()
    {
        if (story == null)
            story = FindFirstObjectByType<StoryController>();

        story?.StartStory();
    }
}
