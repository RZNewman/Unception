using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
    public AudioSource menu;
    public AudioSource game;
    public AudioSource combat;

    public float maxVolume = 0.6f;
    float gameCombatGradient = 0;
    float gradientRate = 0.5f;
    bool inGame = false;
    GlobalPlayer gp;
    // Start is called before the first frame update
    void Start()
    {
        Menu();
        gp = FindObjectOfType<GlobalPlayer>(true);
    }

    public void Menu()
    {
        menu.volume = maxVolume;
        game.volume = 0;
        combat.volume = 0;
        inGame = false;
    }

    public void Game()
    {
        menu.volume = 0;
        game.volume = 1;
        inGame = true;
    }
    

    private void Update()
    {
        if (inGame)
        {
            float multi;
            if (gp.localInCombat)
            {
                multi = 1;
            }
            else
            {
                multi = -1;
            }
            gameCombatGradient += multi * gradientRate * Time.deltaTime;
            gameCombatGradient = Mathf.Clamp01(gameCombatGradient);
            game.volume = (1 - gameCombatGradient)* maxVolume;
            combat.volume = (gameCombatGradient) * maxVolume;
        }
    }
}
