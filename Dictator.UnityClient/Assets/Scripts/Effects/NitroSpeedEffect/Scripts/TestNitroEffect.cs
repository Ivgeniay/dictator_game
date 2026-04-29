using UnityEngine;

public class TestNitroEffect : MonoBehaviour
{
    [SerializeField] private NitroEffectController _nitroEffectController;
     
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace ))
        {
            Debug.Log("Activate");
            _nitroEffectController.Activate();
        }
        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            Debug.Log("Deactivate");
            _nitroEffectController.Deactivate();
        }
    }
}
