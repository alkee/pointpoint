using pp;
using UnityEngine;

public class Sample2
    : MonoBehaviour
{
    [Header("Play")]
    public Stage playingStage;

    [Header("Debugging")]
    public bool fixRandomSeed = false;

    [aus.Property.ConditionalHide(nameof(fixRandomSeed))]
    public int randomSeed = 4321;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
    }
}