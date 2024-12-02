using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityHolder : MonoBehaviour
{
    [SerializeField] Ability ability1;
    [SerializeField] Ability ability2;
    [SerializeField] Ability ability3;
    [SerializeField] Ability ultimate;

    [Header("KeyBinds")]
    public KeyCode key_Ability1 = KeyCode.Mouse1;
    public KeyCode key_Ability2 = KeyCode.LeftShift;
    public KeyCode key_Ability3 = KeyCode.E;
    public KeyCode key_Ultimate = KeyCode.Q;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
