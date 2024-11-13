using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject hostButton;
    [SerializeField] GameObject hostMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenHostMatchMenu()
    {
        hostButton.SetActive(false);
        hostMenu.SetActive(true);
    }

    public void CreateMatch(string username)
    {

    }
}
